using iTV6.Models;
using MsgPack;
using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace iTV6.Services
{
    public class SerializationService
    {
        public SerializationContext Context { get; }

        private SerializationService()
        {
            Context = new SerializationContext();
            Context.Serializers.RegisterOverride(new Channel.ExistingChannelSerializer(Context));
            Context.EnumSerializationOptions.SerializationMethod = EnumSerializationMethod.ByUnderlyingValue;
        }

        /// <summary>
        /// 获取序列化服务实例，实例为单例
        /// </summary>
        public static SerializationService Instance { get; } = new SerializationService();

        public MessagePackSerializer<T> Get<T>() => MessagePackSerializer.Get<T>(Context);
    }

    /// <summary>
    /// 与缓存文件相对应的字典对象
    /// </summary>
    /// <remarks>字典内容如果太多的话可以考虑把字典做成文件映射型的，如利用LMDB进行文件映射</remarks>
    public class CachedDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public string CacheFile { get; set; }
        /// <summary>
        /// 创建新的缓存字典实例
        /// </summary>
        /// <param name="filename">与字典对应的缓存文件名</param>
        public CachedDictionary(string filename) : base()
        {
            CacheFile = filename;
        }

        /// <summary>
        /// 将字典缓存到文件
        /// </summary>
        /// <param name="expire">缓存到期日期</param>
        public async Task Save(DateTime expire)
        {
            var storage = await ApplicationData.Current.LocalFolder.CreateFileAsync(CacheFile, CreationCollisionOption.ReplaceExisting);
            var serializer = SerializationService.Instance.Get<IDictionary<TKey, TValue>>();
            using (var stream = await storage.OpenStreamForWriteAsync())
            {
                SerializationService.Instance.Get<DateTime>().Pack(stream, expire);
                serializer.Pack(stream, this);
            }
        }

        /// <summary>
        /// 从文件恢复字典内容
        /// </summary>
        public async Task Restore()
        {
            var storage = await ApplicationData.Current.LocalFolder.TryGetItemAsync(CacheFile) as StorageFile;
            if (storage != null)
            {
                IDictionary<TKey, TValue> store = null;
                DateTime expire = DateTime.MaxValue;
                using (var stream = await storage.OpenStreamForReadAsync())
                {
                    try
                    {
                        expire = SerializationService.Instance.Get<DateTime>().Unpack(stream);
                        if (expire > DateTime.Now)
                            store = SerializationService.Instance.Get<IDictionary<TKey, TValue>>().Unpack(stream);
                    }
                    catch(UnpackException ue)
                    {
                        // 在缓存格式发生变化时（如版本升级）可能会发生
                        System.Diagnostics.Debug.WriteLine("反序列化错误：" + ue.Message);
                        expire = DateTime.MinValue; // 让缓存文件失效
                    }
                }
                if (expire > DateTime.Now)
                {
                    foreach (var item in store)
                        Add(item.Key, item.Value);
                }
                else
                    await storage.DeleteAsync();
            }
        }

        /// <summary>
        /// 删除文件中的缓存
        /// </summary>
        public async Task ClearCache()
        {
            var storage = await ApplicationData.Current.LocalFolder.TryGetItemAsync(CacheFile) as StorageFile;
            if (storage != null)
                await storage.DeleteAsync();
        }
    }
}
