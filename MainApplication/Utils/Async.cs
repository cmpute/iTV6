using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace iTV6.Utils
{
    public static class Async
    {
        /// <summary>
        /// 使async方法同步运行
        /// </summary>
        /// <param name="asyncMethod">要同步运行的async方法</param>
        public static void InvokeAndWait(Func<Task> asyncMethod)
        {
            Task.Run(() => asyncMethod())
                .ContinueWith(task => task.Wait())
                .Wait();
        }
        /// <summary>
        /// 使返回<see cref="IAsyncAction"/>的无参方法同步运行
        /// </summary>
        /// <param name="asyncMethod">要同步运行的方法</param>
        public static void InvokeAndWait(Func<IAsyncAction> asyncMethod) => InvokeAndWait(async () => await asyncMethod());

        /// <summary>
        /// 使async方法同步运行
        /// </summary>
        /// <param name="asyncMethod">要同步运行的async方法</param>
        public static T InvokeAndWait<T>(Func<Task<T>> asyncMethod)
        {
            Task<T> t = Task.Run(() => asyncMethod())
                .ContinueWith(task =>
                {
                    task.Wait();
                    return task.Result;
                });
            t.Wait();
            return t.Result;
        }
        /// <summary>
        /// 使返回<see cref="IAsyncOperation{TResult}"/>的无参方法同步运行
        /// </summary>
        /// <param name="asyncMethod">要同步运行的方法</param>
        public static T InvokeAndWait<T>(Func<IAsyncOperation<T>> asyncMethod) => InvokeAndWait(async () => await asyncMethod());
    }
}
