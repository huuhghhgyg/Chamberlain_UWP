using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Chamberlain_UWP
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            OnLaunchedOrActivated(e);
            //设置窗口最小大小
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(420, 640));
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            OnLaunchedOrActivated(e);
        }

        private void OnLaunchedOrActivated(IActivatedEventArgs e)
        {
            RegisterBackgroundTaskAsync();

            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            //正常启动
            if (e is LaunchActivatedEventArgs)
            {
                LaunchActivatedEventArgs args = (LaunchActivatedEventArgs)e;
                if (args.PrelaunchActivated == false)
                {
                    if (rootFrame.Content == null)
                    {
                        // 当导航堆栈尚未还原时，导航到第一页，
                        // 并通过将所需信息作为导航参数传入来配置
                        // 参数
                        rootFrame.Navigate(typeof(MainPage), args.Arguments);
                    }
                    // 确保当前窗口处于活动状态
                    Window.Current.Activate();
                }
            }
            // 处理通知激活
            else if (e is ToastNotificationActivatedEventArgs toastActivationArgs)
            {
                if (toastActivationArgs.Argument.Length == 0) //没有参数，直接启动
                {
                    if (rootFrame.Content == null)
                        rootFrame.Navigate(typeof(MainPage));
                }
                else
                {
                    // 从通知获取参数
                    ToastArguments args = ToastArguments.Parse(toastActivationArgs.Argument);

                    // 从通知中获取所有用户输入 (text boxes, menu selections)
                    ValueSet userInput = toastActivationArgs.UserInput;

                    // TODO: 显示相应的内容
                }
            }

            // 保证当前窗口激活
            Window.Current.Activate();
        }

        private async void RegisterBackgroundTaskAsync()
        {
            const string taskName = "ReminderToastBackgroundTask";

            // If background task is already registered, do nothing
            if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(taskName)))
                return;

            // Otherwise request access
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

            // Create the background task
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder()
            {
                Name = taskName
            };

            // Assign the toast action trigger
            builder.SetTrigger(new ToastNotificationActionTrigger());

            // And register the task
            BackgroundTaskRegistration registration = builder.Register();
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            var deferral = args.TaskInstance.GetDeferral();

            switch (args.TaskInstance.Task.Name) // 后台任务类型
            {
                case "ReminderToastBackgroundTask":
                    var details = args.TaskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
                    if (details != null)
                    {
                        ToastArguments arguments = ToastArguments.Parse(details.Argument);
                        var userInput = details.UserInput;

                        // Perform tasks
                        string arg = arguments.Get("action");
                        if (arg == "Check")
                        {
                            int blocking_count = 0;
                            int blocking_timespan = 10;
                            await Reminder.ReminderManager.Data.Load();
                            while (!Reminder.ReminderManager.Data.Loaded)
                            {
                                Thread.Sleep(blocking_timespan);
                                blocking_count++;
                            }

                            Settings.SettingsConfig.InitialLoad(); //读取设置
                            bool isNotificationBlockingVisible = Settings.SettingsConfig.IsNotificationBlockingVisible; //读取是否显示阻塞信息

                            string created_time = arguments.Get("created_time");
                            string item_title = arguments.Get("item_title");
                            Reminder.ReminderManager.CheckItemByCreatedTimeString(created_time);
                            //通知描述
                            string desc = string.Format($"\"{item_title}\"已标记完成");
                            if (isNotificationBlockingVisible) desc = string.Format($"{desc}(空转次数{blocking_count}，共{blocking_count * blocking_timespan}ms)");

                            Notify.NotificationManager.SendNotification("完成", desc);
                        }
                    }
                    break;
            }

            deferral.Complete();
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }
    }
}
