using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Chamberlain_UWP
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this. PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool Set<T>(ref T target, T value, [CallerMemberName] string propertyName = null)
        {
            //如果value无变化
            if(EqualityComparer<T>.Default.Equals(target, value)) return false;

            //目标值与value不同
            target = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
