using Chamberlain_UWP.Backup;
using Chamberlain_UWP.Reminder;
using Chamberlain_UWP.Settings;
using Chamberlain_UWP.Settings.Update;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

            if (DataSettings.IsWindows10) SetAcrylic();

            SetTitleBar();

            InitializeData(initializeNavigate); // 使用回调（Action），先data后navigate

            Updater.CheckUpdate();
        }

        /// <summary>
        /// 将背景设置为Acrylic(仅Win10)
        /// </summary>
        void SetAcrylic()
        {
            //win10
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                KeyValuePair<object, object> styl = Resources.FirstOrDefault(x => x.Key.Equals("AcrylicBg"));
                bgGrid.Style = (Style)styl.Value;
            }
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

            CheckBackupFile();

            await ReminderManager.Data.Load();

            if (callback != null)
                callback();
        }

        private async void CheckBackupFile()
        {
            // 检查有无备份的文件
            StorageFolder local_folder = ApplicationData.Current.LocalFolder;

            StorageFile backupfile = await local_folder.TryGetItemAsync(ReminderManager.BackupFilename) as StorageFile;

            if (backupfile == null) return;//判断是否存在备份文件，不存在则直接返回

            //存在备份文件
            StorageFolder folder = await ReminderManager.Data.getFolder(); //获取使用目录
            StorageFile datafile = await folder.TryGetItemAsync(ReminderManager.DataFilename) as StorageFile;

            if (datafile != null) //判断是否存在数据文件
            {
                // 存在数据文件
                if (string.IsNullOrEmpty(await ReminderManager.Data.Load()))
                {
                    if (ReminderManager.ItemCount > 0) await backupfile.DeleteAsync(); //数据不为空，删除备份
                    else
                    {
                        // 数据为空
                        ContentDialog restoreDialog = new ContentDialog
                        {
                            Title = Strings.Resources.ReminderFoundBackupFileTitle, //"找到"提醒"的数据备份"
                            Content = Strings.Resources.ReminderFoundDataEmptyDesc, //找到数据文件，数据为空，但是找到数据备份。是否使用备份数据恢复？
                            PrimaryButtonText = Strings.Resources.Restore, //恢复
                            CloseButtonText = Strings.Resources.Cancel //取消
                        };
                        ContentDialogResult result = await restoreDialog.ShowAsync();

                        if (result == ContentDialogResult.Primary)
                        {
                            //恢复文件
                            await backupfile.CopyAndReplaceAsync(datafile); //替换数据文件
                        }
                        await backupfile.DeleteAsync(); //删除备份文件
                    }
                }
            }
            else
            {
                // 不存在数据文件
                ContentDialog restoreDialog = new ContentDialog
                {
                    Title = Strings.Resources.ReminderFoundBackupFileTitle,
                    Content = Strings.Resources.ReminderNotFoundDataDesc, //目前无数据文件，但是找到数据备份。是否使用备份数据恢复？
                    PrimaryButtonText = Strings.Resources.Restore, //恢复
                    CloseButtonText = Strings.Resources.Cancel //取消
                };
                ContentDialogResult result = await restoreDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    //恢复文件
                    await backupfile.CopyAsync(folder, ReminderManager.DataFilename); //替换数据文件
                }
                await backupfile.DeleteAsync(); //删除备份文件
            }
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

        private void navControl_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected == true)
            {
                //选中了Settings
                contentFrame.Navigate(typeof(SettingsPage));
            }
            else if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                switch (navItemTag) // 选择选中的其它选项
                {
                    case "task":
                        contentFrame.Navigate(typeof(TaskPage));
                        break;
                    case "reminder":
                        Navigate2Reminder();
                        break;
                    case "backup":
                        contentFrame.Navigate(typeof(BackupPage));
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
                await ReminderManager.Data.Load(); // 跳转页面前加载数据，免得后面使用线程加载（todo）
                contentFrame.Navigate(typeof(ReminderPage));

                //清除返回（由于await跳过了清除，所以在这里再次清除）
                contentFrame.BackStack.Clear();
                contentFrame.ForwardStack.Clear();
            }
        }
    }
}