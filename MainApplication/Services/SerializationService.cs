using iTV6.Models;
using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Services
{
    public class SerializationService
    {
        public SerializationContext Context { get; }

        private SerializationService()
        {
            Context = new SerializationContext();
            Context.Serializers.RegisterOverride(new Channel.ChannelSerializer(Context));
        }

        /// <summary>
        /// 获取序列化服务实例，实例为单例
        /// </summary>
        public static SerializationService Instance { get; } = new SerializationService();

        public MessagePackSerializer<T> Get<T>() => MessagePackSerializer.Get<T>(Context);
    }
}
