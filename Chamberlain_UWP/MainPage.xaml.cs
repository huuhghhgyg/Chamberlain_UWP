using Chamberlain_UWP.Backup;
using Chamberlain_UWP.Reminder;
using Chamberlain_UWP.Settings;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
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
        public MainPage()
        {
            this.InitializeComponent();

            SetTitleBar();

            SettingsConfig.InitialLoad();

            InitializeData(initializeNavigate); // 使用回调（Action），先data后navigate
        }

        private void SetTitleBar()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            // 隐藏默认TitleBar。Hide default title bar.
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);

            // 设置为可拖动区域 Set XAML element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);

            // 分辨率变化的处理
            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            // 当窗口激活状态改变时，注册一个handler
            //Register a handler for when the window changes focus
            Window.Current.Activated += Current_Activated;
        }
        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // 更新TitleBar的大小，适应窗口大小
            AppTitleBar.Height = coreTitleBar.Height;

            // 保证自定义TitleBar不会覆盖标题控件
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }
        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }
        private void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e) // 根据是否处于活动状态更新TitleBar
        {
            SolidColorBrush defaultForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            SolidColorBrush inactiveForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorDisabledBrush"];

            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated) AppTitle.Foreground = inactiveForegroundBrush;
            else AppTitle.Foreground = defaultForegroundBrush;
        }

        private async void InitializeData(Action callback)
        {
            ToastNotificationManagerCompat.History.Clear(); //打开程序时清除所有通知

            await ReminderManager.Data.Load();

            if (callback != null)
                callback();
        }

        private void initializeNavigate()
        {
            contentFrame.Navigate(typeof(TaskPage));
            navControl.SelectedItem = navControl.MenuItems[0];
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

        // 根据NavigationView的显示模式更新TitleBar布局（宽/窄）
        // Update the TitleBar content layout depending on NavigationView DisplayMode
        private void NavigationViewControl_DisplayModeChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewDisplayModeChangedEventArgs args)
        {
            const int topIndent = 16;
            const int expandedIndent = 48;
            int minimalIndent = 104;

            // If the back button is not visible, reduce the TitleBar content indent.
            if (navControl.IsBackButtonVisible.Equals(Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed))
            {
                minimalIndent = 48;
            }

            Thickness currMargin = AppTitleBar.Margin;

            // Set the TitleBar margin dependent on NavigationView display mode
            if (sender.PaneDisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top)
            {
                AppTitleBar.Margin = new Thickness(topIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
            else if (sender.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal)
            {
                AppTitleBar.Margin = new Thickness(minimalIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
            else
            {
                AppTitleBar.Margin = new Thickness(expandedIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
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
                    case "备份":
                        contentFrame.Navigate(typeof(BackupPage));
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