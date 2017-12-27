using iTV6.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace iTV6.Services
{
    public class TileService
    {
        private TileService() { }
        /// <summary>
        /// 获取磁贴服务实例，实例为单例
        /// </summary>
        public static TileService Instance { get; } = new TileService();

        /// <summary>
        /// 将频道固定至开始菜单
        /// </summary>
        /// <param name="channel">需要固定的频道</param>
        /// <remarks> TODO: 可以添加更多的参数，如是否为LiveTile，选择尺寸，背景为频道缩略图等等 </remarks>
        public async Task<bool> PinChannel(Channel channel)
        {
            Uri logo = new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png");
            SecondaryTile tile = new SecondaryTile("channel-" + channel.Name,
                channel.Name, channel.UniqueId, logo, TileSize.Square150x150);
            tile.VisualElements.ShowNameOnSquare150x150Logo = true;

            if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent(("Windows.Phone.UI.Input.HardwareButtons"))))
            {
                // 在PC端
                return await tile.RequestCreateAsync();
            }
            else
            {
                // 若在手机端的话，贴磁铁时应用会被挂起，因此需要额外处理
                return false;
            }
        }
    }
}
