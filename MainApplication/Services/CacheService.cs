using iTV6.Models;
using MsgPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace iTV6.Services
{
    /// <summary>
    /// 缓存的操作优先级比较高，需要优先初始化，因此设为静态类而非单例类
    /// </summary>
    public static class CacheService
    {
        /// <summary>
        /// 读取各种缓存
        /// </summary>
        public static async void RestoreCache()
        {
#if DEBUG && CLEAR_CACHE
            await ClearCache();
#else
            await Channel.RestoreChannels();
            await ScheduleService.Instance.RestoreCache();
#endif
        }

        /// <summary>
        /// 保存各种缓存
        /// </summary>
        public static async Task SaveCache()
        {
            await Channel.StoreChannels();
            await ScheduleService.Instance.SaveCache();
        }

        /// <summary>
        /// 清空各种缓存
        /// </summary>
        public static async Task ClearCache()
        {
            foreach(var file in _cacheFiles)
            {
                var storage = await ApplicationData.Current.LocalFolder.TryGetItemAsync(file) as StorageFile;
                if (storage != null) await storage.DeleteAsync();
            }
        }

        static List<string> _cacheFiles = new List<string>();

        /// <summary>
        /// 计算缓存大小
        /// </summary>
        public static async Task<ulong> ComputeCacheSize()
        {
            ulong totalSize = 0;
            foreach (var file in _cacheFiles)
            {
                var storage = await ApplicationData.Current.LocalFolder.TryGetItemAsync(file) as StorageFile;
                if (storage == null) continue;
                var prop = await storage.GetBasicPropertiesAsync();
                totalSize += prop.Size;
            }
            return totalSize;
        }

        /// <summary>
        /// 注册缓存文件，以便计算缓存大小和清空缓存
        /// </summary>
        /// <param name="filename"></param>
        public static void RegisterCacheFile(string filename)
            => _cacheFiles.Add(filename);
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
            CacheService.RegisterCacheFile(filename);
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
                    catch (UnpackException ue)
                    {
                        // 在缓存格式发生变化时（如版本升级）可能会发生
                        LoggingService.Debug("Service", "反序列化错误：" + ue.Message,
                            Windows.Foundation.Diagnostics.LoggingLevel.Error);
                        expire = DateTime.MinValue; // 让缓存文件失效
                    }
                    catch(Exception e)
                    {
                        // 其他异常
                        LoggingService.Debug("Service", $"反序列化过程中发生{e.GetType().Name}：{e.Message}",
                            Windows.Foundation.Diagnostics.LoggingLevel.Error);
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
            if (storage != null) await storage.DeleteAsync();
        }
    }
}
