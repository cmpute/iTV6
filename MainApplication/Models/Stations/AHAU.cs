using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace iTV6.Models.Stations
{
    public class AHAU : ITelevisionStation
    {
        public string IdentifierName => "安徽农大";
        public bool IsScheduleAvailable => false;

        public async Task<IEnumerable<ProgramSource>> GetChannelList(bool force = false)
        {
            try
            {
                HttpClient client = new HttpClient();
                var buffer = await client.GetStreamAsync("http://itv.ahau.edu.cn/");
                HtmlDocument doc = new HtmlDocument();
                doc.Load(buffer, SuperEncoding.GB2312);
                var channelNodes = doc.DocumentNode.SelectNodes("/html/body/div[1]/div/div");

                string group = null;
                var sourceSet = new HashSet<string>();
                var result = new List<ProgramSource>();

                foreach (var cNode in channelNodes)
                {
                    if (cNode.GetAttributeValue("class", string.Empty) == "col-xs-6 col-sm-3 col-md-3 text-left")
                    {
                        string source = null;
                        var linkNode = cNode.SelectSingleNode(".//a");
                        var link = linkNode.GetAttributeValue("href", string.Empty);
                        
                        // 地址转换的列表可以参考 http://itv.ahau.edu.cn/MyScript/MyScript.js
                        // 但是视频源具体是否能用就得手测了
                        if (link.IndexOf("hoge.cn") >= 0) continue; // 外部电视台，虽然可以抓到m3u8链接，但是是IPv4的，而且节目也垃圾
                        if (link.StartsWith("noplayer")) continue; // 同上

                        // 计算频道ID
                        var chName = linkNode.InnerText.Trim();
                        string editName = null;
                        if(group.StartsWith("高清"))
                        {
                            editName = chName.Replace("卫视", "hd");
                        }
                        else
                        {
                            editName = chName.Replace("卫视", "tv");
                        }
                        var chSpell = SuperEncoding.GetSpellCode(editName, false);
                        string channelCode = link.Substring(link.LastIndexOf('=') + 1);

                        if (link.StartsWith("playcooldiao"))
                            source = "http://itv3.ahau.edu.cn/cmstop/s10001-video-{channelCode}.m3u8";
                        if (link.StartsWith("http://"))
                        {
                            var type = link.Substring(27, link.IndexOf('?', 27) - 32);
                            switch(type)
                            {
                                case "BJBGP":       // 无法播放 "http://itv.ahau.edu.cn/livestream/" + channel + ".flv"
                                case "cooldiao":    // 上面已经处理过
                                case "CQFJ":        // 无法播放 "http://itv3.ahau.edu.cn/ch" + channel + "/playlist.m3u8"
                                case "Direct":      // 也是外部站
                                case "FLV":         // 无法播放 "http://itv.ahau.edu.cn/" + channel + ".flv"
                                case "FLVsd":       // 无法播放 "http://itv.ahau.edu.cn/channels/" + channel + "/flv:sd/live.flv"
                                case "GSLZ":        // 无法播放 "http://itv3.ahau.edu.cn/seg/" + channel + ".m3u8?_upt=?ts"
                                case "GSTV":        // 无法播放 "http://itv3.ahau.edu.cn/" + channel + "/hd/live.m3u8?_upt=ts"
                                case "HeBLF":       // 无法播放 "http://itv1.ahau.edu.cn/live/ch" + channel + ".m3u8"
                                case "HeBSJZ":      // 无法播放 "http://itv.ahau.edu.cn/channels/sjztv/video_channel_0" + channel + "/flv:500k/live.flv"
                                case "MMS":         // 无法播放 "mms://wms.ahau.edu.cn/" + channel
                                case "NN_live":     // 无法播放 "http://itv2.ahau.edu.cn/nn_live.flv?id=" + channel
                                case "SCGA":        // 无法播放 "http://itv.ahau.edu.cn/flv" + channel + "00/playlist.flv"
                                case "SCGY":        // 无法播放 "http://itv1.ahau.edu.cn/streamer/gy0" + channel + ".m3u8"
                                case "SCLS":        // 无法播放 "http://itv3.ahau.edu.cn/vms/videos/channellive/channel" + channel + "/playlist.m3u8"
                                case "SCMY":        // 无法播放 "http://itv1.ahau.edu.cn/"+ channel + "/" + channel + ".flv/playlist.m3u8"
                                case "YNKM":        // 无法播放 "http://itv.ahau.edu.cn/live/" + channel + ".flv?fmt=h264_800k_flv&flv"
                                case "ZJSX":        // 无法播放 "http://itv.ahau.edu.cn/channels/shaoxing/" + channel + "/flv:500K/live.flv"
                                case "ZJXC":        // 无法播放 "http://itv3.ahau.edu.cn/channels/lantian/sx1_xinchang0" + channel + "/540p.m3u8"
                                    continue;
                                case "CQDJ":
                                    source = $"http://itv3.ahau.edu.cn/livestream/{channelCode}.flv";
                                    break;
                                case "HeNZZ":
                                    source = $"http://itv3.ahau.edu.cn/live/s{channelCode}/index.m3u8";
                                    break;
                                case "Manifest":
                                    source = $"http://itv3.ahau.edu.cn/{channelCode}/manifest.m3u8";
                                    break;
                                case "ZhouJ":
                                    source = $"http://itv3.ahau.edu.cn/{channelCode}/500/live.m3u8";
                                    break;
                                case "ZJJH":
                                    source = $"http://itv3.ahau.edu.cn/{channelCode}/app/live.m3u8";
                                    continue; // 链接能打开但是只有黑屏
                                default:
                                    System.Diagnostics.Debug.WriteLine($"不识别的安徽农大视频源: {link}");
                                    continue;
                            }
                        }
                        else // 剩下的开头都是prplayer.html
                        {
                            if (link.Substring(14, 4) != "type")
                            {
                                source = $"http://itv.ahau.edu.cn/hls/{channelCode}.m3u8";
                                if (channelCode.StartsWith("jlu"))
                                    chSpell = channelCode.Substring(4);
                                else if (!channelCode.StartsWith("hls"))
                                    chSpell = channelCode;
                                else
                                {
                                    // 这里也是频道ID的特殊处理
                                    if (group.StartsWith("高清"))
                                    {
                                        if (chName == "北京纪实")
                                            chSpell = "btv11hd";
                                        else if (chName == "北京卫视")
                                            chSpell = "btv1hd";
                                        else if (chName == "CGTN")
                                            chSpell = "cgtnhd";
                                        else if (chName.StartsWith("CCTV") || chName.StartsWith("CETV"))
                                        {
                                            if (chName[5] == '+')
                                                chSpell = "cctv5phd";
                                            else if (char.IsDigit(chName[5]))
                                                chSpell = chSpell.Substring(0, 6) + "hd";
                                            else
                                                chSpell = chSpell.Substring(0, 5) + "hd";
                                        }
                                    }
                                    // System.Diagnostics.Debug.WriteLine($"[{channelCode}] -> [{chSpell:10}]");
                                }
                            }
                            else
                            {
                                var type = link.Substring(19, link.IndexOf('&', 19) - 19);
                                var serverIdx = link.IndexOf("server=");
                                var serverCode = serverIdx > -1 ? link.Substring(serverIdx + 7, 1) : string.Empty;
                                var serverPrefix = $"http://itv{serverCode}.ahau.edu.cn";
                                switch (type)
                                {
                                    case "cbgcn":   // 无法播放 "http://itv.ahau.edu.cn/app_2/ls_{channelCode}.stream/index.m3u8"
                                    case "chxl":    // 无法播放 "http://itv.ahau.edu.cn/hls/{channelCode}.m3u8"
                                    case "cjybc":   // 无法播放 "http://itv3.ahau.edu.cn/video/s10{channelCode}/index.m3u8"
                                    case "cmccnj":  // 无法播放 "http://itv.ahau.edu.cn/hls/{channelCode}.m3u8"
                                    case "cucc":    // 无法播放 "http://itv.ahau.edu.cn/hls/{channelCode}.m3u8"
                                    case "hlsp":    // 无法播放 "http://itv.ahau.edu.cn/hls/{channelCode}.m3u8"
                                    case "nf":      // 无法播放 "http://itv.ahau.edu.cn/{channelCode}.flv";
                                    case "nnlivef": // 无法播放 "http://itv{serverCode}.ahau.edu.cn/nn_live.flv?id={channelCode}"
                                    case "tsls":    // 无法播放 "http://itv{serverCode}.ahau.edu.cn/tslslive/{channelCode}/hls/live_sd.m3u8
                                    case "xxxh":    // 无法播放 "http://itv{serverCode}.ahau.edu.cn/{channelCode}/hd/live.m3u8"
                                    case "rtmp":    // 无法播放 "rtmp://itv1.ahau.edu.cn/ahau/{channelCode}"
                                        continue;
                                    case "ah1":
                                    case "ah13":
                                    case "cooldiao":
                                        source = $"http://itv.ahau.edu.cn/hls/cctv5.m3u8";
                                        continue; // 这个是服务器上的default地址，说明视频不可用
                                    case "ah05":
                                        source = $"{serverPrefix}/channels/preview/{channelCode}/m3u8:500k/live.m3u8";
                                        break;
                                    case "ah06":
                                        source = serverPrefix + "/channels/39/500.flv";
                                        break;
                                    case "ah10":
                                        source = $"{serverPrefix}/vod/video_hls/c0{channelCode}.m3u8";
                                        break;
                                    case "ah11":
                                        source = $"{serverPrefix}/{channelCode}00/live.m3u8";
                                        break;
                                    case "ah14":
                                        source = $"{serverPrefix}/live/live10{channelCode}/1000K/tzwj_video.m3u8";
                                        break;
                                    case "ah15":
                                        source = $"{serverPrefix}/live/live10{channelCode}/500K/tzwj_video.m3u8";
                                        break;
                                    case "ah16":
                                        source = $"{serverPrefix}/live/live{channelCode}/800K/tzwj_video.m3u8";
                                        break;
                                    case "ah18":
                                        source = serverPrefix + "/videos/live/36/57/hwEHU4UVQ1Iv5/hwEHU4UVQ1Iv5.m3u8";
                                        break;
                                    case "sh05":
                                        source = serverPrefix + "/live/program/Dkankan.live.bestvcdn.com.cnD/_cFJG001TPI_keyA7C10A07E2D1B53B9C9A81EF5132C26A22B52061D01975ADC98DE389E1B533A6_/live/ysrw/400000/mnf.m3u8";
                                        break;
                                    case "sh06":
                                        source = serverPrefix + "/live/program/Dkankan.live.bestvcdn.com.cnD/_cFJG001XGz_key83204D9AB8979442DD2E24B744C0CD5281C63674E90C91A82317C464DE4486E9_/live/wypd/400000/mnf.m3u8";
                                        break;
                                    case "sh07":
                                        source = serverPrefix + "/live/program/Dkankan.live.bestvcdn.com.cnD/_cFJG001ZvT_keyC48C92682BBBD951F45FD946A535CD61DA9CC5039BC0E8E53400E8ED7AC7FE5D_/live/ylpd/400000/mnf.m3u8";
                                        break;
                                    case "sh08":
                                        source = serverPrefix + "/live/program/Dkankan.live.bestvcdn.com.cnD/_cFJG0021g9_key0D5A4F8EEFAE5892C6BC2211814D2EBE27CDCB24E066F0256D984DF08FB5E2F6_/live/jspd/400000/mnf.m3u8";
                                        break;
                                    case "sh09":
                                        source = serverPrefix + "/live/program/Dkankan.live.bestvcdn.com.cnD/_cFJG001cQ1_keyB181B2FC6B246FD263042B03C72AF28CA805FA6276D551A543927FE7CB52DE4A_/live/shss/400000/mnf.m3u8";
                                        break;
                                    case "sh10":
                                        source = serverPrefix + "/live/program/Dkankan.live.bestvcdn.com.cnD/_cFJG00230Y_key0C6FC3C6BFD8A04D702DFB49686F55BF48470CD4F15A776C1A4ADB393A66C94C_/live/dycj/400000/mnf.m3u8";
                                        break;
                                    case "cmcc":
                                        source = $"{serverPrefix}/PLTV/88888888/224/322122{channelCode}/index.m3u8";
                                        break;
                                    case "hlsi":
                                        source = $"{serverPrefix}/hls/{channelCode}/index.m3u8";
                                        break;
                                    case "hunzz":
                                        source = $"{serverPrefix}/live/live12{channelCode}/800K/tzwj_video.m3u8";
                                        break;
                                    case "i":
                                        source = $"{serverPrefix}/{channelCode}/index.m3u8";
                                        break;
                                    case "lantian":
                                        source = $"{serverPrefix}/channels/lantian/channel{channelCode}/360p.m3u8";
                                        break;
                                    case "lec":
                                        source = $"{serverPrefix}/live/hls/201{channelCode}/desc.m3u8";
                                        continue; // 这个连接没问题，能够获取ts地址，但是貌似是外链到乐视云上去了。。
                                    case "lived":
                                        source = $"{serverPrefix}/live/{channelCode}.m3u8";
                                        continue; // 有些能播有些不能播，不是很稳定。清华电视台在这一类，但是没啥节目。。而且估计校外的也是看不了的
                                    case "livei":
                                        source = $"{serverPrefix}/live/{channelCode}/index.m3u8";
                                        break;
                                    case "liven":
                                        source = $"{serverPrefix}/live/{channelCode}/.m3u8";
                                        continue; // 几乎连不上
                                    case "livep":
                                        source = $"{serverPrefix}/live/{channelCode}/playlist.m3u8";
                                        if (serverCode == string.Empty) break;
                                        if (channelCode == "wjpd") break;
                                        if (channelCode == "gjpd") break;
                                        // XXX: 继续测试看看哪些频道可用。。。
                                        continue; // 大部分都无法播放， 能播放的也是巨小的台
                                    case "livexd":
                                        source = $"{serverPrefix}/live/{channelCode}/{channelCode}.m3u8";
                                        break;
                                    case "n":
                                        source = $"{serverPrefix}/{channelCode}.m3u8";
                                        break;
                                    case "nm05":
                                        source = $"{serverPrefix}/channels/btgd/{channelCode}/m3u8:500k/live.m3u8";
                                        break;
                                    case "nttv":
                                        source = $"{serverPrefix}/channels/nttv/{channelCode}/m3u8:SD/live.m3u8";
                                        break;
                                    case "p":
                                        source = $"{serverPrefix}/{channelCode}/playlist.m3u8";
                                        break;
                                    case "sihtv":
                                        source = $"{serverPrefix}/channels/tvie/{channelCode}/m3u8:500k/live.m3u8";
                                        break;
                                    case "xxxs":
                                        source = $"{serverPrefix}/{channelCode}/sd/live.m3u8";
                                        break;
                                    default:
                                        System.Diagnostics.Debug.WriteLine($"不识别的安徽农大视频源: {link}");
                                        continue;
                                }
                            }
                        }
                        // 避免电视源重复
                        if (!sourceSet.Add(source))
                            continue;

                        Channel channel = Channel.GetChannel(chSpell, chName);
                        result.Add(new ProgramSource()
                        {
                            IsThumbAvaliable = false,
                            MediaSource = new Uri(source),
                            MediaSourceTag = channelCode,
                            IsMediaAvaliable = true,
                            SourceStation = this,
                            ProgramInfo = new Program()
                            {
                                Name = chName,
                                Channel = channel
                            }
                        });
                        System.Diagnostics.Debug.WriteLine($"[AHAU|{group}]{chName:10} : {chSpell:10}");
                    }
                    else
                        group = cNode.InnerText;
                }
                return result;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message, "Error");
                System.Diagnostics.Debugger.Break();
                return new List<ProgramSource>();
            }
        }
        public Task<IEnumerable<Program>> GetSchedule(Channel channel, bool force = false)
        {
            throw new NotImplementedException("Schedule Not Avaliable");
        }
    }
}
