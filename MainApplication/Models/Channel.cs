using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Models
{
    /// <summary>
    /// 频道的Model。频道为近似单例模式
    /// </summary>
    public class Channel
    {
        private Channel() { }
        private static Dictionary<string, Channel> _instances = new Dictionary<string, Channel>();
        /// <summary>
        /// 根据名称和分类获得频道实例
        /// </summary>
        /// <param name="name">频道名称</param>
        /// <param name="type">频道分类</param>
        public static Channel GetChannel(string name, ChannelType type)
        {
            name = GetUniqueChannelName(name);
            var uniname = name + ((int)type).ToString(); // 电视和广播可能重名
            if (!_instances.ContainsKey(uniname))
                _instances.Add(uniname, new Channel() { Name = name, Type = type });
            return _instances[uniname];
        }
        /// <summary>
        /// 根据名称获得频道实例
        /// </summary>
        /// <param name="name">频道名称</param>
        public static Channel GetChannel(string name)
        {
            return GetChannel(name, GetChannelTypeByName(name));
        }

        /// <summary>
        /// 频道名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 频道类型
        /// </summary>
        public ChannelType Type { get; set; }

        /// <summary>
        /// 统一频道列表
        /// </summary>
        /// <param name="name">从网站扒取的频道名称</param>
        /// <returns>统一后的名称，用作字典的键</returns>
        public static string GetUniqueChannelName(string name)
        {
            // 硬编码列表，遇到特例时手动加进来
            switch (name)
            {
                case "第一剧场":
                    return "CCTV-第一剧场";
                case "世界地理":
                    return "CCTV-世界地理";
                case "风云剧场":
                    return "CCTV-风云剧场";
                case "风云音乐":
                    return "CCTV-风云音乐";
                case "风云足球":
                    return "CCTV-风云足球";
                case "国防军事":
                    return "CCTV-国防军事";
                case "怀旧剧场":
                    return "CCTV-怀旧剧场";
                default:
                    return name;
            }
        }
        /// <summary>
        /// 根据名称推测频道分类列表
        /// </summary>
        /// <param name="channel">统一后的频道名称</param>
        /// <returns>频道类型</returns>
        public static ChannelType GetChannelTypeByName(string channel)
        {
            ChannelType result = ChannelType.Standard;
            if (channel.IndexOf("高清") > -1)
                result = ChannelType.Hd;
            if (channel.IndexOf("CCTV") > -1 || channel.IndexOf("CGTN") > -1)
                return result | ChannelType.Central;
            if (channel.IndexOf("卫视") > -1)
                return result | ChannelType.Local;
            if (channel.IndexOf("北京") > -1)
                return result | ChannelType.Local | ChannelType.Beijing;
            if (channel.IndexOf("教育电视台") > -1 || channel.IndexOf("CHC") > -1 ||
                channel.IndexOf("卡通") > -1 || channel.IndexOf("党建") > -1)
                return result | ChannelType.Local;
            return result | ChannelType.Special;
        }
    }

    /// <summary>
    /// 频道类型
    /// </summary>
    [Flags]
    public enum ChannelType
    {
        Central = 0,    //中央电视台
        Local = 1,      //地方电视台
        Special = 2,    //特色频道
        Radio = 4,      //广播
        Standard = 8,   //标清
        Hd = 16,        //高清
        Beijing = 32    //北京
    }
}
