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
    }
}