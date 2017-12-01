using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System.Text
{
    /// <summary>
    /// 支持GB2312、BIG5编码
    /// </summary>
    /// <remarks> Code from https://github.com/chenrensong/Encoding.UWP </remarks>
    public sealed class SuperEncoding : Encoding
    {

        public static Encoding GB2312
        {
            get
            {
                return SuperEncoding.GetEncoding("GB2312");
            }
        }

        public static Encoding BIG5
        {
            get
            {
                return SuperEncoding.GetEncoding("BIG5");
            }
        }

        private const char LEAD_BYTE_CHAR = '\uFFFE';
        private char[] _dbcsToUnicode = null;
        private ushort[] _unicodeToDbcs = null;
        private string _webName = null;
        /// <summary>
        /// Super Encoding 支持的编码
        /// </summary>
        private static string[] _supportedEncoding = new[] { "GB2312", "BIG5" };

        private static Dictionary<string, Tuple<char[], ushort[]>> _cache = null;

        static SuperEncoding()
        {
            if (!BitConverter.IsLittleEndian)
                throw new PlatformNotSupportedException("Not supported big endian platform.");
            _cache = new Dictionary<string, Tuple<char[], ushort[]>>();
        }

        private SuperEncoding() { }


        /// <summary>
        /// 获取Encoding（非BIG5、GB2312编码调用原有的Encoding.GetEncoding）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static new Encoding GetEncoding(string name)
        {
            if (!_supportedEncoding.Any(m => m.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return Encoding.GetEncoding(name);
            }

            name = name.ToLower();
            SuperEncoding encoding = new SuperEncoding();
            encoding._webName = name;
            if (_cache.ContainsKey(name))
            {
                var tuple = _cache[name];
                encoding._dbcsToUnicode = tuple.Item1;
                encoding._unicodeToDbcs = tuple.Item2;
                return encoding;
            }
            var dbcsToUnicode = new char[0x10000];
            var unicodeToDbcs = new ushort[0x10000];
            /*
             * According to many feedbacks, add this automatic function for finding resource in revision 1.0.0.1.
             * We suggest that use the old method as below if you understand how to embed the resource.
             * Please make sure the *.bin file was correctly embedded if throw an exception at here.
             */
            using (Stream stream = typeof(SuperEncoding).GetTypeInfo().Assembly
                .GetManifestResourceStream(typeof(SuperEncoding).GetTypeInfo().Assembly.
                GetManifestResourceNames().Single(s => s.EndsWith("." + name + ".bin"))))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                for (int i = 0; i < 0xffff; i++)
                {
                    ushort u = reader.ReadUInt16();
                    unicodeToDbcs[i] = u;
                }
                for (int i = 0; i < 0xffff; i++)
                {
                    ushort u = reader.ReadUInt16();
                    dbcsToUnicode[i] = (char)u;
                }
            }

            _cache[name] = new Tuple<char[], ushort[]>(dbcsToUnicode, unicodeToDbcs);
            encoding._dbcsToUnicode = dbcsToUnicode;
            encoding._unicodeToDbcs = unicodeToDbcs;
            return encoding;
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            int byteCount = 0;
            ushort u;
            char c;

            for (int i = 0; i < count; index++, byteCount++, i++)
            {
                c = chars[index];
                u = _unicodeToDbcs[c];
                if (u > 0xff)
                    byteCount++;
            }

            return byteCount;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            int byteCount = 0;
            ushort u;
            char c;

            for (int i = 0; i < charCount; charIndex++, byteIndex++, byteCount++, i++)
            {
                c = chars[charIndex];
                u = _unicodeToDbcs[c];
                if (u == 0 && c != 0)
                {
                    bytes[byteIndex] = 0x3f;    // 0x3f == '?'
                }
                else if (u < 0x100)
                {
                    bytes[byteIndex] = (byte)u;
                }
                else
                {
                    bytes[byteIndex] = (byte)((u >> 8) & 0xff);
                    byteIndex++;
                    byteCount++;
                    bytes[byteIndex] = (byte)(u & 0xff);
                }
            }

            return byteCount;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return GetCharCount(bytes, index, count, null);
        }

        private int GetCharCount(byte[] bytes, int index, int count, SuperDecoder decoder)
        {
            int charCount = 0;
            ushort u;
            char c;

            for (int i = 0; i < count; index++, charCount++, i++)
            {
                u = 0;
                if (decoder != null && decoder.pendingByte != 0)
                {
                    u = decoder.pendingByte;
                    decoder.pendingByte = 0;
                }

                u = (ushort)(u << 8 | bytes[index]);
                c = _dbcsToUnicode[u];
                if (c == LEAD_BYTE_CHAR)
                {
                    if (i < count - 1)
                    {
                        index++;
                        i++;
                    }
                    else if (decoder != null)
                    {
                        decoder.pendingByte = bytes[index];
                        return charCount;
                    }
                }
            }

            return charCount;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            return GetChars(bytes, byteIndex, byteCount, chars, charIndex, null);
        }

        private int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, SuperDecoder decoder)
        {
            int charCount = 0;
            ushort u;
            char c;

            for (int i = 0; i < byteCount; byteIndex++, charIndex++, charCount++, i++)
            {
                u = 0;
                if (decoder != null && decoder.pendingByte != 0)
                {
                    u = decoder.pendingByte;
                    decoder.pendingByte = 0;
                }

                u = (ushort)(u << 8 | bytes[byteIndex]);
                c = _dbcsToUnicode[u];
                if (c == LEAD_BYTE_CHAR)
                {
                    if (i < byteCount - 1)
                    {
                        byteIndex++;
                        i++;
                        u = (ushort)(u << 8 | bytes[byteIndex]);
                        c = _dbcsToUnicode[u];
                    }
                    else if (decoder == null)
                    {
                        c = '\0';
                    }
                    else
                    {
                        decoder.pendingByte = bytes[byteIndex];
                        return charCount;
                    }
                }
                if (c == 0 && u != 0)
                    chars[charIndex] = '?';
                else
                    chars[charIndex] = c;
            }

            return charCount;
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
                throw new ArgumentOutOfRangeException("charCount");
            long count = charCount + 1;
            count *= 2;
            if (count > int.MaxValue)
                throw new ArgumentOutOfRangeException("charCount");
            return (int)count;

        }

        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0)
                throw new ArgumentOutOfRangeException("byteCount");
            long count = byteCount + 3;
            if (count > int.MaxValue)
                throw new ArgumentOutOfRangeException("byteCount");
            return (int)count;
        }

        public override Decoder GetDecoder()
        {
            return new SuperDecoder(this);
        }

        public override string WebName
        {
            get
            {
                return _webName;
            }
        }

        private sealed class SuperDecoder : Decoder
        {
            private SuperEncoding _encoding = null;

            public SuperDecoder(SuperEncoding encoding)
            {
                _encoding = encoding;
            }

            public override int GetCharCount(byte[] bytes, int index, int count)
            {
                return _encoding.GetCharCount(bytes, index, count, this);
            }

            public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
            {
                return _encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex, this);
            }

            public byte pendingByte;
        }

        #region 拼音首字母转换

        /// <summary> 
        /// 获取汉字字符串的拼音首字母，如果是一个英文字母则直接返回
        /// </summary> 
        /// <param name="cnstring">汉字字符串</param>
        /// <param name="upper">若返回大写则为真，小写则为假</param>
        /// <returns>相对应的汉语拼音首字母串</returns> 
        public static string GetSpellCode(string cnstring, bool upper = true) =>
            string.Concat(cnstring.Select(cnch => GetSpellCode(cnch, upper)));

        /// <summary> 
        /// 得到一个汉字的拼音第一个字母，如果是一个英文字母则直接返回
        /// </summary> 
        /// <param name="character">汉字字符</param>
        /// <param name="upper">若返回大写则为真，小写则为假</param>
        /// <returns>单个大写字母</returns> 
        public static char GetSpellCode(char character, bool upper = true)
        {
            string cstr = character.ToString();
            byte[] cnbytes = GB2312.GetBytes(cstr);
            if (cnbytes.Length == 1) // 如果是英文字母，则直接返回
            {
                if (upper) return cstr.ToUpper()[0];
                else return cstr.ToLower()[0];
            }
            
            switch(character) // 二级汉字无法通过位置来判断拼音，这里直接列出来
            {
                case '邳': return upper ? 'P' : 'p';
                case '睿': return upper ? 'R' : 'r';
                case '悚':
                case '泗':
                    return upper ? 'S' : 's';
                case '学':
                case '讯':
                    return upper ? 'X' : 'x';
                case '圳': return upper ? 'Z' : 'z';
                default:
                    int ichar = cnbytes[0] << 8 | cnbytes[1];
                    if ((ichar >= 45217) && (ichar <= 45252)) return upper ? 'A' : 'a';
                    else if ((ichar >= 45253) && (ichar <= 45760)) return upper ? 'B' : 'b';
                    else if ((ichar >= 45761) && (ichar <= 46317)) return upper ? 'C' : 'c';
                    else if ((ichar >= 46318) && (ichar <= 46825)) return upper ? 'D' : 'd';
                    else if ((ichar >= 46826) && (ichar <= 47009)) return upper ? 'E' : 'e';
                    else if ((ichar >= 47010) && (ichar <= 47296)) return upper ? 'F' : 'f';
                    else if ((ichar >= 47297) && (ichar <= 47613)) return upper ? 'G' : 'g';
                    else if ((ichar >= 47614) && (ichar <= 48118)) return upper ? 'H' : 'h';
                    else if ((ichar >= 48119) && (ichar <= 49061)) return upper ? 'J' : 'j';
                    else if ((ichar >= 49062) && (ichar <= 49323)) return upper ? 'K' : 'k';
                    else if ((ichar >= 49324) && (ichar <= 49895)) return upper ? 'L' : 'l';
                    else if ((ichar >= 49896) && (ichar <= 50370)) return upper ? 'M' : 'm';
                    else if ((ichar >= 50371) && (ichar <= 50613)) return upper ? 'N' : 'n';
                    else if ((ichar >= 50614) && (ichar <= 50621)) return upper ? 'O' : 'o';
                    else if ((ichar >= 50622) && (ichar <= 50905)) return upper ? 'P' : 'p';
                    else if ((ichar >= 50906) && (ichar <= 51386)) return upper ? 'Q' : 'q';
                    else if ((ichar >= 51387) && (ichar <= 51445)) return upper ? 'R' : 'r';
                    else if ((ichar >= 51446) && (ichar <= 52217)) return upper ? 'S' : 's';
                    else if ((ichar >= 52218) && (ichar <= 52697)) return upper ? 'T' : 't';
                    else if ((ichar >= 52698) && (ichar <= 52979)) return upper ? 'W' : 'w';
                    else if ((ichar >= 52980) && (ichar <= 53640)) return upper ? 'X' : 'x';
                    else if ((ichar >= 53689) && (ichar <= 54480)) return upper ? 'Y' : 'y';
                    else if ((ichar >= 54481) && (ichar <= 55289)) return upper ? 'Z' : 'z';
                    else return ('?');
            }

        }

        #endregion
    }
}