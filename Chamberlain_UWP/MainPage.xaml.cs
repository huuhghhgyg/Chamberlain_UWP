using Chamberlain_UWP.Reminder;
using Chamberlain_UWP.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
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

            SettingsConfig.InitialLoad();

            InitializeData(initializeNavigate); // 使用回调（Action），先data后navigate
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.RegisterBackgroundTask();
        }

        private async void RegisterBackgroundTask() //注册后台任务
        {
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (backgroundAccessStatus == BackgroundAccessStatus.AllowedSubjectToSystemPolicy ||
                backgroundAccessStatus == BackgroundAccessStatus.AlwaysAllowed)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == taskName)
                    {
                        task.Value.Unregister(true);
                    }
                }

                BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
                taskBuilder.Name = taskName;
                taskBuilder.TaskEntryPoint = taskEntryPoint;
                uint interval = (uint)SettingsConfig.UpdateTriggerInterval; //导入设置变量中的间隔
                taskBuilder.SetTrigger(new TimeTrigger(interval, false));
                var registration = taskBuilder.Register();
            }
        }

        private const string taskName = "ChamberlainBackgroundUpdater";
        private const string taskEntryPoint = "BackgroundUpdater.DateUpdater";

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

            //清除返回
            contentFrame.BackStack.Clear();
            contentFrame.ForwardStack.Clear();
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
                await ReminderManager.Data.Load(); // 跳转页面前加载数据，免得后面使用线程加载（todo）
                contentFrame.Navigate(typeof(ReminderPage));

                //清除返回（由于await跳过了清除，所以在这里再次清除）
                contentFrame.BackStack.Clear();
                contentFrame.ForwardStack.Clear();
            }
        }
    }
}