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

namespace Chamberlain_UWP.Reminder
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ReminderPage : Page
    {
        ObservableCollection<ReminderItem> ReminderListOnwork = new ObservableCollection<ReminderItem>(); // 正在处理
        ObservableCollection<ReminderItem> ReminderListFinished = new ObservableCollection<ReminderItem>(); // 已完成

        bool IsPageAlive = true; // 确认页面是否被Unload
        bool IsProgressUpdaterWorking = false; // 进程锁，防止多次创建进程

        public ReminderPage()
        {
            this.InitializeComponent();
        }

        private async void RefreshData()
        {
            IsProgressUpdaterWorking = true; // 上锁
            while (IsPageAlive)
            {
                if (ReminderManager.ItemCountRunning > 0) //检测是否存在需要更新进度的条目
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
            IsProgressUpdaterWorking = false; // 解锁
        }

        private void RefreshReminderList(bool state)
        {
            /* 点击之后状态的collection+1。执行前的ObservableList没同步，存在差异。
             * true: 已完成 +1
             * false：未完成/已过期 +1
             */
            ReminderItem itemMoved = null;
            if (state)
            {
                // 未完成/已过期+1
                List<ReminderItem> finished = new List<ReminderItem>();
                ReminderManager.GetList(finished, TaskState.Finished);
                finished.ForEach(item =>
                {
                    if (!ReminderListFinished.Contains(item)) // 找旧列表中不包含的项
                    {
                        itemMoved = item;
                    }
                });
                if (itemMoved != null)
                {
                    ReminderListFinished.Insert(0,itemMoved);
                    ReminderListOnwork.Remove(itemMoved);
                }
            }
            else
            {
                // 已完成+1
                List<ReminderItem> onwork = new List<ReminderItem>();
                ReminderManager.GetList(onwork,TaskState.Onwork);
                ReminderManager.GetList(onwork,TaskState.OutOfDate);
                onwork.ForEach(item =>
                {
                    if (!ReminderListOnwork.Contains(item)) // 找旧列表中不包含的项
                    {
                        itemMoved = item;
                    }
                });
                if (itemMoved != null)
                {
                    // 选择插入。顺序：过期事项、高优先级、中优先级、普通
                    if (itemMoved.TaskState == TaskState.OutOfDate) // 是否过期项
                    {
                        ReminderListOnwork.Insert(0, itemMoved);
                    }
                    else if (itemMoved.Priority > Priority.Default) // 是否特别优先级项
                    {
                        if(itemMoved.Priority == Priority.High) // 是否高优先级项
                        {
                            int items_outofdate = ReminderListOnwork
                                .Where(item => item.TaskState == TaskState.OutOfDate)
                                .ToList()
                                .Count();
                            ReminderListOnwork.Insert(items_outofdate, itemMoved);
                        }
                        else
                        {
                            // 中优先级项
                            int items_before = ReminderListOnwork
                                .Where(item => item.Priority == Priority.High || item.TaskState == TaskState.OutOfDate)
                                .ToList()
                                .Count();
                            ReminderListOnwork.Insert(items_before, itemMoved);
                        }
                    }
                    else
                    {
                        int items_before = ReminderListOnwork
                            .Where(item => item.TaskState == TaskState.OutOfDate || item.Priority > Priority.Default)
                            .ToList()
                            .Count();
                        ReminderListOnwork.Insert(items_before,itemMoved);
                    }

                    ReminderListFinished.Remove(itemMoved);
                    
                    // 由于IsCheck为双向绑定，因此不需要在这里更新ReminderManager里面的列表
                }
            }
        }

        private void ManageRemindItemButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ReminderItemsManagePage));
        }

        delegate void FilterReminderItems(AutoSuggestBox sender);

        private void OnworkItemSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            GetListOnwork();

            FilterReminderItems filter; //声明委托
            List<string> suggestList = new List<string>(); //AutoSuggest项目列表

            if (ItemsSortComboBox.SelectedIndex == 1)
            {
                //ReminderListOnwork.ToList().ForEach(item => suggestList.Add(item.Title));
                //sender.ItemsSource = suggestList.Where(item => item.Contains(sender.Text)).ToList();
                sender.ItemsSource = ReminderListOnwork
                                        .Where(item => item.Title.Contains(sender.Text))
                                        .Select(item => item.Title)
                                        .ToList();

                filter = new FilterReminderItems(FilterReminderItemsByName);
            }
            else
            {
                ReminderManager.GetTagList(suggestList);
                sender.ItemsSource = suggestList.Where(item => item.Contains(sender.Text)).ToList();

                filter = new FilterReminderItems(FilterReminderItemsByTag);
            }
            filter(sender); // 使用委托
        }

        private void FilterReminderItemsByName(AutoSuggestBox sender)
        {
            List<ReminderItem> reminderItems = new List<ReminderItem>(ReminderListOnwork);

            reminderItems = reminderItems.Where(item => item.Title.Contains(sender.Text)).ToList();

            //根据名称搜索
            ReminderListOnwork.Clear();
            reminderItems.ForEach(item => ReminderListOnwork.Add(item));
        }

        private void FilterReminderItemsByTag(AutoSuggestBox sender)
        {
            List<ReminderItem> reminderItems = new List<ReminderItem>(ReminderListOnwork);

            //根据tag搜索
            reminderItems = reminderItems.Where(item => item.TagsString.Contains(sender.Text)).ToList();

            ReminderListOnwork.Clear();
            reminderItems.ForEach(item => ReminderListOnwork.Add(item));
        }

        private void ItemsSortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnworkItemSuggestBox.Text = "";
        }

        private void GetListOnwork() // 用于筛选Onwork状态的item
        {
            ReminderListOnwork.Clear();
            ReminderManager.GetList(ReminderListOnwork, TaskState.Onwork); // 获取未完成提醒，放入正在处理
            ReminderManager.GetList(ReminderListOnwork, TaskState.OutOfDate); // 获取过期提醒，放入正在处理
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            IsPageAlive = false; //结束线程
        }

        private async void ReminderCardListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            bool state = e.IsChecked;
            // 点击之后的状态
            RefreshReminderList(state);
            await ReminderManager.Data.Save(); // 保存数据

            if (!IsProgressUpdaterWorking) // 判断进程是否不在工作（活动的条目=0）
                new Thread(RefreshData).Start(); // 重启更新进度的进程
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            IsPageAlive = true;
            ReminderManager.SortListByDefault(); // 对List先进行排序

            ReminderListOnwork.Clear();
            ReminderListFinished.Clear();

            ReminderManager.GetList(ReminderListOnwork, TaskState.OutOfDate); // 1. 获取过期提醒，放入正在处理（减少排序工作量，代码按照这个顺序）
            ReminderManager.GetList(ReminderListOnwork, TaskState.Onwork); // 2. 获取未完成提醒，放入正在处理
            ReminderManager.GetList(ReminderListFinished, TaskState.Finished); // 获取已完成提醒，放入已完成

            new Thread(RefreshData).Start(); // 更新进度
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            IsPageAlive=false;
        }
    }

    class ListCount2VisibilityConverter : IValueConverter
    {
        // 输入ListCount，返回是否可见
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (int)value > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException(); // 单向绑定，不做规定
        }
    }
}
