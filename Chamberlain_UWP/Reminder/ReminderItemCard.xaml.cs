using System;
using System.Collections.Generic;
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
    public sealed partial class ReminderItemCard : UserControl
    {
        public ReminderItem Reminder { get => this.DataContext as ReminderItem; set => DataContext = value; }
        public ReminderItemCard()
        {
            this.InitializeComponent();

            this.DataContextChanged += (s, e) => Bindings.Update();
        }

        public event EventHandler<ItemCheckedEventArgs> ItemChecked;

        private void ItemCheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool state = ItemCheckBox.IsChecked == true;
            ItemCheckedEventArgs args = new ItemCheckedEventArgs() { IsChecked = state };
            ItemChecked?.Invoke(this, args);
        }
    }

    public class ItemCheckedEventArgs : RoutedEventArgs
    {
        public bool IsChecked { get; set; }
    }
}
