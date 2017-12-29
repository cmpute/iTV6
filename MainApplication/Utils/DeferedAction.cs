using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Utils
{
    /// <summary>
    /// 用来存储延迟的并只执行一次的操作
    /// </summary>
    public class DeferedAction
    {
        public DeferedAction(Action action = null)
        {
            Action = action;
        }

        public Action Action { get; set; }

        public void Invoke()
        {
            Action?.Invoke();
            Action = null;
        }
    }

    public class DeferedAction<T>
    {
        public DeferedAction(Action<T> action = null)
        {
            Action = action;
        }

        public Action<T> Action { get; set; }

        public void Invoke(T param)
        {
            Action?.Invoke(param);
            Action = null;
        }
    }
}
