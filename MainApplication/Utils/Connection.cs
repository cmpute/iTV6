using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace iTV6.Utils
{
    /// <summary>
    /// 连接性测试工具
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// 测试指定域名是否能够连接
        /// </summary>
        /// <param name="hostname">域名</param>
        /// <returns>能正常连接则返回true</returns>
        public static async Task<bool> TestConnectivity(string hostname)
        {
            try
            {
                HostName host = new HostName(hostname);
                var eps = await DatagramSocket.GetEndpointPairsAsync(host, "80");
                return eps.Count >= 1;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("在测试连接性中发生异常");
                System.Diagnostics.Debug.WriteLine(e.Message);
                return false;
            }
        }

        private static bool? IPv6Connected = null;
        /// <summary>
        /// 测试是否有IPv6的连接
        /// </summary>
        /// <param name="force">是否强制刷新</param>
        /// <returns>能正常连接则返回true</returns>
        public static async Task<bool> TestIPv6Connectivity(bool force = false)
        {
            if(IPv6Connected == null || force)
                IPv6Connected = await TestConnectivity("ipv6.google.com");
            return IPv6Connected.Value;
        }
    }
}
