using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;

namespace BackgroundUpdater
{
    public sealed class DateUpdater : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            LoadSettings();
            LoadReminder(); //检查Reminder的信息
        }

        private void LoadReminder()
        {
            LoadReminderItems();
            ApplyReminderStatistics();
        }

        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings; //本地存储的设置
        private static ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings; //漫游存储的设置

        bool roaming_settings;

        private void ApplyReminderStatistics()
        {
            object cache;
            cache = roaming_settings ? roamingSettings.Values["IsNotificationEnabled"] : localSettings.Values["IsNotificationEnabled"];
            bool isNotificationEnabled = (cache == null) ? true : (bool)cache; //是否开启通知，默认开启

            if (isNotificationEnabled)
            {
                int round_count=0;
                while (!ReminderManager.ReminderItemListLoaded) //线程阻塞
                {
                    Thread.Sleep(100);
                    round_count++;
                }

                int remian_hour = 1; //规定时间：1个小时
                List<ReminderItem> list = ReminderManager.Statistics.AdjournmentItemList(remian_hour); //规定时间内到期的列表
                if (list.Count > 0) //存在规定时间以内到期的项
                {
                    int count = list.Count; //规定时间内到期的项数

                    string title = string.Format($"{count}项deadline不足{remian_hour}小时");
                    string desc = "";

                    // 发送通知
                    // todo: 通过通知确认完成

                    if (list.Count <= 3)
                    {
                        // 临期项在3项以内
                        list.ForEach(item =>
                        {
                            desc += string.Format($"\"{item.Title}\"、"); //添加临期项
                            });
                        desc = desc.TrimEnd('、'); //去掉末尾的顿号
                        desc += string.Format($"将在{remian_hour}个小时内到期。");
                    }
                    else
                    {
                        //临期项大于3项
                        for (int i = 0; i < 3; i++)
                            desc += string.Format($"\"{list[i].Title}\"、"); //添加临期项
                        desc = desc.TrimEnd('、'); //去掉末尾的顿号
                        desc += string.Format($"等，共{count}项将在{remian_hour}个小时内到期。");
                    }

                    // 已过期项提示
                    int outdated = ReminderManager.Statistics.Outdated;
                    if (outdated > 0) //存在过期项
                        desc += String.Format($"此外，{outdated}项已过期，请注意。");

                    // 发送通知
                    cache = roaming_settings ? roamingSettings.Values["IsNotificationBlockingVisible"] : localSettings.Values["IsNotificationBlockingVisible"];
                    bool IsNotificationBlockingVisible = (cache == null) ? false : (bool)cache; //是否显示通知阻塞信息，默认关闭
                    //分情况发送通知
                    if(IsNotificationBlockingVisible) SendNotification(title, string.Format($"{desc}(空转次数{round_count})"));
                    else SendNotification(title, desc);
                }
            }

            //更新磁贴
            ReminderManager.UpdateTile();
        }

        public async void LoadReminderItems() // 检查ReminderItems
        {
            string err;
            err = await ReminderManager.Data.Load(); //读取数据
            if (string.IsNullOrEmpty(err))
            {
                // 读取成功
                ReminderManager.RefreshList(); //更新数据
                ReminderManager.SortListByDefault(); //对数据排序
                ReminderManager.ReminderItemListLoaded = true;
            }
            else throw new Exception(err);
        }

        private void LoadSettings()
        {
            object cache;
            cache = localSettings.Values["IsSettingsRoamingEnabled"];
            roaming_settings = (cache == null) ? false : (bool)cache; //设置是否漫游，默认不漫游
        }

        private void SendNotification(string title, string content)
        {
            // 触发通知
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title
                            },
                            new AdaptiveText()
                            {
                                Text = content
                            }
                        }
                    }
                }
            };

            // Create the toast notification
            var toastNotif = new ToastNotification(toastContent.GetXml());

            // And send the notification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);

        }
    }
}
