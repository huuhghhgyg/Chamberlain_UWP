using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace BackgroundUpdater
{
    public sealed class DateUpdater : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            CheckReminderItems(); //检查Reminder的信息
            ApplyReminderStatistics();
        }

        private void ApplyReminderStatistics()
        {
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
                SendNotification(title, desc);
            }

            //更新磁贴
            ReminderManager.UpdateTile();
        }

        public async void CheckReminderItems() // 检查ReminderItems
        {
            string err;
            err = await ReminderManager.Data.Load(); //读取数据
            if (string.IsNullOrEmpty(err))
            {
                // 读取成功
                if (ReminderManager.Statistics.ItemCount==0)
                    SendNotification("(Debug) Strange thing", "列表中没有项"); //todo: 仅供debug使用的代码，调式好后删除
                else
                {
                    //列表中项数不为0
                    ReminderManager.RefreshList(); //更新数据
                    ReminderManager.SortListByDefault(); //对数据排序
                }
            }
            else throw new Exception(err);
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
