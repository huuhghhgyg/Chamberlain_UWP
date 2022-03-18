using Chamberlain_UWP.Reminder;
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

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Chamberlain_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private void initializeData()
        {
            List<string> tg = new List<string>(){ "tag1", "tag2", "tag3" };
            List<string> tg2 = new List<string>(){ "tag3", "newtag" };
            ReminderItem item1 = new ReminderItem("item1", "desc1", tg, DateTime.Now.AddHours(1), TaskState.Onwork);
            ReminderItem item2 = new ReminderItem("item2", "desc2", tg, DateTime.Now.AddHours(1), TaskState.Onwork);
            ReminderItem item3 = new ReminderItem("item3", "desc3", tg, DateTime.Now.AddHours(1), TaskState.Onwork);
            ReminderItem item4 = new ReminderItem("item4", "desc4", tg2, DateTime.Now.AddHours(1), TaskState.Finished);
            ReminderItem item5 = new ReminderItem("item5", "desc5", tg, DateTime.Now.AddHours(1), TaskState.Onwork);
            ReminderItem item6 = new ReminderItem("item6", "desc6", tg, DateTime.Now.AddHours(1), TaskState.Finished);
            ReminderItem item7 = new ReminderItem("item7", "desc7", tg2, DateTime.Now.AddHours(1), TaskState.Onwork);
            ReminderManager.Add(item1);
            ReminderManager.Add(item2);
            ReminderManager.Add(item3);
            ReminderManager.Add(item4);
            ReminderManager.Add(item5);
            ReminderManager.Add(item6);
            ReminderManager.Add(item7);
        }
        public MainPage()
        {
            this.InitializeComponent();
            initializeData();
            contentFrame.Navigate(typeof(TaskPage));
        }

        private void navControl_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                //选中了Settings
                contentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                switch (args.InvokedItem) // 选择选中的其它选项
                {
                    case "任务":
                        contentFrame.Navigate(typeof(TaskPage));
                        break;
                    case "提醒":
                        contentFrame.Navigate(typeof(Reminder.ReminderPage));
                        break;
                }
            }
        }

        private void navControl_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            // navControl的IsBackEnabled与contentFrame.CanGoBack绑定，无需判断。
            contentFrame.GoBack();
        }
    }
}