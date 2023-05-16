using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Notifications;
using static BackgroundUpdater.Notify;

namespace BackgroundUpdater
{
    public sealed class DateUpdater : IBackgroundTask
    {
        static ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            LoadSettings();
            LoadReminder(); //检查Reminder的信息
            RemindCheck(); //检查是否到时间执行每日提醒
        }

        private void LoadReminder()
        {
            LoadReminderItems();
            ApplyReminderStatistics();
        }

        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings; //本地存储的设置
        private static ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings; //漫游存储的设置

        bool roaming_settings;
        bool isNotificationEnabled;

        private void ApplyReminderStatistics()
        {
            object cache;
            cache = roaming_settings ? roamingSettings.Values["IsNotificationEnabled"] : localSettings.Values["IsNotificationEnabled"];
            isNotificationEnabled = (cache == null) ? true : (bool)cache; //是否开启通知，默认开启

            if (!isNotificationEnabled) //没有开启通知，更新磁贴后直接返回
            {
                //更新磁贴
                ReminderManager.UpdateTile();
                return;
            }

            int blocking_count = 0; //阻塞次数
            int blocking_timespan = 10; //阻塞时间（毫秒）

            while (!ReminderManager.Data.Loaded) //线程阻塞
            {
                Thread.Sleep(blocking_timespan);
                blocking_count++;
            }

            int remain_hour = 1; //规定时间：1个小时
            List<ReminderItem> list = ReminderManager.Statistics.AdjournmentItemList(remain_hour); //规定时间内到期的列表
            int outdated = ReminderManager.Statistics.Outdated;
            if (list.Count > 0) //存在规定时间以内到期的项
            {
                int count = list.Count; //规定时间内到期的项数

                string title, desc = "";

                // 发送通知
                // 小于3项直接跳过，大于3项堆叠通知名
                if (list.Count <= 2)
                {
                    // 小于等于2项，每项的标题
                    title = string.Format(resourceLoader.GetString("NotifyDeadlineWithinHours"), remain_hour); //deadline不足{remain_hour}小时
                }
                else
                {
                    // 三项或以上的标题
                    title = string.Format(resourceLoader.GetString("NotifyMultipleDeadlineWithinHours"), count, remain_hour); //{count}项deadline不足{remain_hour}小时

                    if (list.Count == 3)
                    {
                        // 临期项在3项以内
                        list.ForEach(item =>
                        {
                            desc += string.Format($"\"{item.Title}\", "); //添加临期项
                        });
                        desc = desc.TrimEnd().TrimEnd(','); //去掉末尾的逗号组合（", "）
                        desc += string.Format(resourceLoader.GetString("NotifyDueInHours"), remain_hour); //将在{remain_hour}个小时内到期。
                    }
                    else
                    {
                        //临期项大于3项
                        for (int i = 0; i < 3; i++)
                            desc += string.Format($"\"{list[i].Title}\", "); //添加临期项
                        desc = desc.TrimEnd().TrimEnd(','); //去掉末尾的顿号
                        desc += string.Format(resourceLoader.GetString("NotifyMultipleDueInHours"), count, remain_hour); //等，共{count}项将在{remain_hours}个小时内到期。
                    }
                }

                // 已过期项提示
                if (outdated > 0) //存在过期项
                {
                    if (!string.IsNullOrEmpty(desc)) desc += resourceLoader.GetString("NotifyDescInAddition");
                    desc += string.Format(resourceLoader.GetString("NotifyOutdatedItems"), outdated); //此外，{outdated}项已过期，请尽快处理。
                }

                // 发送通知
                cache = roaming_settings ? roamingSettings.Values["IsNotificationBlockingVisible"] : localSettings.Values["IsNotificationBlockingVisible"];
                bool IsNotificationBlockingVisible = (cache == null) ? false : (bool)cache; //是否显示通知阻塞信息，默认关闭

                //阻塞信息处理
                if (IsNotificationBlockingVisible)
                    desc = string.Format(resourceLoader.GetString("NotifyBlockingInfo"), desc, blocking_count, blocking_count * blocking_timespan);

                //分情况发送通知
                //todo: 此部分可以改进，可以发送多个可确认toast
                if (count < 3)
                {
                    list.ForEach(item =>
                    {
                        string item_desc = string.Format(resourceLoader.GetString("NotifyReminderItemMeetDeadline"), item.Title);
                        NotificationManager.SendNotification_ReminderCheck(title, item_desc, item); //仅在临期项只有一项时使用，否则将勾选所有符合的项
                    });
                }
                else NotificationManager.SendNotification(title, desc);
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
                ReminderManager.Data.Loaded = true;
            }
            else throw new Exception(err);
        }

        private void LoadSettings()
        {
            object cache;
            cache = localSettings.Values["IsSettingsRoamingEnabled"];
            roaming_settings = (cache == null) ? false : (bool)cache; //设置是否漫游，默认不漫游
        }

        private void RemindCheck()
        {
            //如果没有开启通知，直接返回
            if (!isNotificationEnabled) return;

            object cache;
            cache = roaming_settings ? roamingSettings.Values["IsRemindOnTimeEnabled"] : localSettings.Values["IsRemindOnTimeEnabled"];
            bool IsRemindOnTimeEnabled = (cache == null) ? true : (bool)cache; //是否开启每日定时通知，默认开启

            //如果没有开启定时通知，直接返回
            if (!IsRemindOnTimeEnabled) return;

            cache = roaming_settings ? roamingSettings.Values["RemindTime"] : localSettings.Values["RemindTime"];
            TimeSpan RemindTime = (cache == null) ? new TimeSpan(17, 0, 0) : (TimeSpan)cache; //每日定时通知时间，默认17：00

            cache = roaming_settings ? roamingSettings.Values["UpdateTriggerInterval"] : localSettings.Values["UpdateTriggerInterval"];
            int UpdateTriggerInterval = (cache == null) ? 15 : (int)cache; ; //通知更新间隔，最小、默认=15

            DateTime RemindTimeToday = DateTime.Today + RemindTime; //获取今天提醒的具体时间
            TimeSpan delta = RemindTimeToday - DateTime.Now; //时间差
            if (delta >= TimeSpan.Zero && delta <= new TimeSpan(0, UpdateTriggerInterval, 0)) //相差时间小于唤醒时间间隔（在唤醒时间前1个“通知更新时间间隔”内就会提醒）
            {
                //提醒
                string title = resourceLoader.GetString("ReminderNotifyTitle"), desc = ""; //title:事项提醒
                List<ReminderItem> list = ReminderManager.Statistics.AdjournmentItemList(24); //获取24小时内的到期项
                if (list.Count > 0) //剩余24小时内到期的项大于0
                {
                    title = resourceLoader.GetString("ReminderNotifyDescRemaining"); //title:剩余事项提醒
                    if (list.Count > 5)
                    {
                        desc += string.Format(resourceLoader.GetString("ReminderNotifyDescMultipleDueInADay"), list.Count) + '\n'; //有{list.Count}项将在1天之内到期。
                    }
                    else if (list.Count > 1)
                    {
                        List<string> titles = new List<string>();
                        list.ForEach(item => titles.Add($"\"{item.Title}\""));
                        desc += $"{string.Join(",", titles)}" + string.Format(resourceLoader.GetString("ReminderNotifyDescItemsDueInADay"), list.Count) + '\n'; //共{list.Count}项将在1天之内到期。
                    }
                    else
                    {
                        desc += $"\"{list[0].Title}\"" + resourceLoader.GetString("ReminderNotifyDescItemDueInADay") + '\n';
                    }
                }

                if (ReminderManager.Statistics.Outdated > 0)
                {
                    if (!string.IsNullOrEmpty(desc)) desc += resourceLoader.GetString("NotifyDescInAddition");
                    desc += string.Format(resourceLoader.GetString("NotifyOutdatedItems"), ReminderManager.Statistics.Outdated); //过期项提醒：此外，{ReminderManager.Statistics.Outdated}项已过期，请尽快处理。
                }

                //描述非空时发送每日事项体现
                if (!string.IsNullOrEmpty(desc))
                    NotificationManager.SendNotification(title, desc);
            }

        }
    }
}
