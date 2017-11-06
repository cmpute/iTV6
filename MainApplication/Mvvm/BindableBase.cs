using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace iTV6.Mvvm
{
    // From Template10
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void Set(Action set, [CallerMemberName]string propertyName = null)
        {
            set.Invoke();
            RaisePropertyChanged(propertyName);
        }

        public virtual bool Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        public virtual void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
