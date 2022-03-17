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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Chamberlain_UWP.Reminder
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ReminderPage : Page
    {
        ObservableCollection<ReminderItem> ReminderListOnwork = new ObservableCollection<ReminderItem>(); // 正在处理
        ObservableCollection<ReminderItem> ReminderListFinished = new ObservableCollection<ReminderItem>(); // 已完成
        public ReminderPage()
        {
            this.InitializeComponent();
            ReminderManager.GetReminderList(ReminderListOnwork, 0); // 获取未完成提醒，放入正在处理
            ReminderManager.GetReminderList(ReminderListOnwork, 2); // 获取过期提醒，放入正在处理
            ReminderManager.GetReminderList(ReminderListFinished, 1); // 获取已完成提醒，放入已完成
        }

        private void RefreshReminderList(bool state)
        {
            /* 点击之后状态的collection+1。执行前的ObservableList没同步，存在差异。
             * true: 已完成 +1
             * false：未完成 +1
             */
            ReminderItem itemMoved = null;
            if (state)
            {
                //已完成+1
                List<ReminderItem> finished = new List<ReminderItem>();
                ReminderManager.GetReminderList(finished, 1);
                finished.ForEach(item =>
                {
                    if (!ReminderListFinished.Contains(item)) // 找旧列表中不包含的项
                    {
                        itemMoved = item;
                    }
                });
                if (itemMoved != null)
                {
                    ReminderListFinished.Add(itemMoved);
                    ReminderListOnwork.Remove(itemMoved);
                }
            }
            else
            {
                //未完成+1
                List<ReminderItem> onwork = new List<ReminderItem>();
                ReminderManager.GetReminderList(onwork,0);
                onwork.ForEach(item =>
                {
                    if (!ReminderListOnwork.Contains(item)) // 找旧列表中不包含的项
                    {
                        itemMoved = item;
                    }
                });
                if (itemMoved != null)
                {
                    ReminderListOnwork.Add(itemMoved);
                    ReminderListFinished.Remove(itemMoved);
                }
            }
        }

        private void ManageRemindItemButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ReminderItemsManagePage));
        }

        private void ItemCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var control = (CheckBox)sender;
            bool sender_state = (bool)control.IsChecked; // 点击之后的状态
            RefreshReminderList(sender_state);
        }

        private void OnworkItemSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            List<string> itemsName = new List<string>();
            ReminderListOnwork.ToList().ForEach(item => itemsName.Add(item.Title));
            sender.ItemsSource = itemsName.Where(item => item.Contains(sender.Text)).ToList();
        }
    }
}
