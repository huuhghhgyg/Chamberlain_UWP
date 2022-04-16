using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Chamberlain_UWP.Reminder
{
    public sealed partial class ReminderCardListView : UserControl
    {
        public ReminderCardListView()
        {
            this.InitializeComponent();

            this.DataContextChanged += (s, e) => Bindings.Update();
        }

        public ObservableCollection<ReminderItem> ReminderList
        {
            get => DataContext as ObservableCollection<ReminderItem>;
            set => DataContext = value;
        }

        public event EventHandler<ItemCheckedEventArgs> ListViewItemChecked;
        private void ReminderItemCard_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ListViewItemChecked?.Invoke(this, e);
        }
    }
}
