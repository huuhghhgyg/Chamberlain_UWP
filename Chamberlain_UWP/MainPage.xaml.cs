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

        private async void InitializeData(Action callback)
        {
            await ReminderManager.Data.Load();

            if (callback != null)
                callback();
        }

        private void initializeNavigate()
        {
            contentFrame.Navigate(typeof(TaskPage));
            navControl.SelectedItem = navControl.MenuItems[0];
        }

        public MainPage()
        {
            this.InitializeComponent();

            InitializeData(initializeNavigate); // 使用回调（Action），先data后navigate
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
                        Navigate2Reminder();
                        break;
                }
            }
        }

        private void navControl_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            // navControl的IsBackEnabled与contentFrame.CanGoBack绑定，无需判断。
            contentFrame.GoBack();
        }

        private async void Navigate2Reminder()
        {
            if (ReminderManager.Data.IsDataEmpty) // 判断数据是否为空
            {
                // 为空，跳转到OOBE
                contentFrame.Navigate(typeof(Reminder.OOBE.ReminderOOBEWelcome));
            }
            else
            {
                await ReminderManager.Data.Load();
                contentFrame.Navigate(typeof(ReminderPage));
            }
        }
    }
}