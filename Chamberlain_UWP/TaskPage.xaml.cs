using Chamberlain_UWP.Reminder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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

namespace Chamberlain_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TaskPage : Page
    {
        ObservableCollection<ReminderItem> ReminderListOnwork = new ObservableCollection<ReminderItem>(); // 正在处理

        bool IsPageAlive = true; // 确认页面是否被Unload

        public TaskPage()
        {
            this.InitializeComponent();

            ReminderManager.SortListByDefault();

            ReminderManager.GetList(ReminderListOnwork, TaskState.OutOfDate); // 获取过期提醒，放入正在处理
            ReminderManager.GetList(ReminderListOnwork, TaskState.Onwork); // 获取未完成提醒，放入正在处理

            if(ReminderListOnwork.Count > 0)
                NoTaskTextBlock.Visibility = Visibility.Collapsed; //有任务，隐藏无任务的提示

            new Thread(RefreshData).Start(); //更新进度

            ReminderManager.UpdateTile(); //更新磁贴
        }

        private async void RefreshData()
        {
            while (IsPageAlive)
            {
                if (ReminderManager.ItemCountOnwork > 0)
                {
                    Thread.Sleep(ReminderManager.UpdateTimeSpan); // 根据列表中项的最小时间间隔来计算
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        ReminderManager.UpdateListProgress();
                    });
                }
                else
                {
                    break; //没有onwork项直接结束线程
                }
            }
        }

        private async void ItemCheckBox_Click(object sender, RoutedEventArgs e)
        {
            // 未完成减少了
            List<ReminderItem> listOnwork = new List<ReminderItem>(ReminderListOnwork);
            List<ReminderItem> newlistOnwork = new List<ReminderItem>(); //少
            ReminderManager.GetList(newlistOnwork,TaskState.Finished);
            listOnwork.ForEach(item =>
            {
                if(newlistOnwork.Contains(item)) ReminderListOnwork.Remove(item); //删除不存在的项
            });

            if (ReminderListOnwork.Count == 0)
                NoTaskTextBlock.Visibility = Visibility.Visible; // 无任务时提示任务已完成

            await ReminderManager.Data.Save(); // 保存数据
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            IsPageAlive = false;
        }
    }
}
