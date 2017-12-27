using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using iTV6.Models;
using Windows.Foundation.Collections;
using System.Collections.ObjectModel;

namespace iTV6.Services
{
    /// <summary>
    /// 提供收藏相关功能的服务
    /// </summary>
    public class CollectionService
    {
        const string containerKey = "Collections";
        const string channelCollectionKey = "Channels";
        const string programCollectionKey = "Programs";
        const char splitChar = '\n';

        private ApplicationDataContainer _container =
            ApplicationData.Current.RoamingSettings.CreateContainer(containerKey, ApplicationDataCreateDisposition.Always);
        
        /// <summary>
        /// 被收藏的频道列表
        /// </summary>
        public ObservableCollection<Channel> ChannelList { get; }
        public event NotifyCollectionChangedEventHandler ChannelListChanged;
        /// <summary>
        /// 被收藏的节目列表
        /// </summary>
        public ObservableCollection<string> ProgramList { get; } // 由于不储存频道信息，Program对象是不完整的，只能用UniqueID代表
        public event NotifyCollectionChangedEventHandler ProgramListChanged;

        private CollectionService()
        {
#if DEBUG
            // 在调试状态下每次都清空收藏
            if (_container.Values.ContainsKey(channelCollectionKey))
                _container.Values[channelCollectionKey] = string.Empty;
            if (_container.Values.ContainsKey(programCollectionKey))
                _container.Values[programCollectionKey] = string.Empty;
#endif
            // 建立频道收藏列表
            if (!_container.Values.ContainsKey(channelCollectionKey))
            {
                _container.Values.Add(channelCollectionKey, string.Empty);
                ChannelList = new ObservableCollection<Channel>();
            }
            else
                ChannelList = new ObservableCollection<Channel>((_container.Values[channelCollectionKey] as string)
                    .Split(splitChar).Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => Channel.GetChannel(id)));
            ChannelList.CollectionChanged += SyncChannelList;
            ChannelList.CollectionChanged += (sender, e) => ChannelListChanged?.Invoke(sender, e);

            // 建立节目收藏列表
            if (!_container.Values.ContainsKey(programCollectionKey))
            {
                _container.Values.Add(programCollectionKey, string.Empty);
                ProgramList = new ObservableCollection<string>();
            }
            else
                ProgramList = new ObservableCollection<string>((_container.Values[programCollectionKey] as string)
                    .Split(splitChar).Where(name => !string.IsNullOrWhiteSpace(name)));
            ProgramList.CollectionChanged += SyncProgramList;
            ProgramList.CollectionChanged += (sender, e) => ProgramListChanged?.Invoke(sender, e);
        }
        
        /// <summary>
        /// 获取收藏服务实例，实例为单例
        /// </summary>
        public static CollectionService Instance { get; } = new CollectionService();

        private void SyncChannelList(object sender, NotifyCollectionChangedEventArgs e)
        {
            _container.Values[channelCollectionKey] = String.Join(splitChar.ToString(), ChannelList.Select(ch => ch.UniqueId));
        }
        public void AddChannel(Channel channel)
        {
            Debug.Assert(!CheckChannel(channel));
            ChannelList.Add(channel);
        }
        public void RemoveChannel(Channel channel)
        {
            Debug.Assert(CheckChannel(channel));
            ChannelList.Remove(channel);
        }
        public bool CheckChannel(Channel channel)
        {
            Debug.Assert(!channel.Name.Contains(splitChar), "频道名称应不含换行符");
            return ChannelList.Contains(channel);
        }

        private void SyncProgramList(object sender, NotifyCollectionChangedEventArgs e)
        {
            _container.Values[channelCollectionKey] = String.Join(splitChar.ToString(), ProgramList);
        }
        public void AddProgram(Models.Program program)
        {
            Debug.Assert(!CheckProgram(program));
            ProgramList.Add(program.UniqueId);
        }
        public void RemoveProgram(Models.Program program)
        {
            Debug.Assert(CheckProgram(program));
            ProgramList.Remove(program.UniqueId);
        }
        public bool CheckProgram(Models.Program program)
        {
            Debug.Assert(!program.UniqueId.Contains(splitChar), "节目ID应不含换行符");
            return ProgramList.Contains(program.UniqueId);
        }
    }
}
