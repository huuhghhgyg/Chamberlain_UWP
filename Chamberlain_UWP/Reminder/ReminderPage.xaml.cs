using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
             * 前提假设：每次只变化一项
             * true: 已完成 +1
             * false：未完成/已过期 +1
             */

            if (state)
            {
                // 未完成/已过期+1
                RefreshUnfinishedReminderList();
            }
            else
            {
                // 已完成+1
                RefreshFinishedReminderList();
            }
        }

        private void RefreshUnfinishedReminderList()
        {
            // 未完成/已过期+1
            List<ReminderItem> finishedItems = new List<ReminderItem>();
            ReminderManager.GetList(finishedItems, TaskState.Finished);

            ReminderItem itemMoved = null; //初始化变化项
            foreach (ReminderItem item in finishedItems)
            {
                if (!ReminderListFinished.Contains(item) && NotFiltered(item)) // 找旧列表中不包含的项
                {
                    itemMoved = item;
                    break;
                }
            }

            Insert2ReminderList(ReminderListFinished, itemMoved);
            ReminderListOnwork.Remove(itemMoved);
        }

        private void RefreshFinishedReminderList()
        {
            // 已完成+1
            List<ReminderItem> unfinishedItems = new List<ReminderItem>();
            ReminderManager.GetList(unfinishedItems, TaskState.Onwork);
            ReminderManager.GetList(unfinishedItems, TaskState.OutOfDate);

            ReminderItem itemMoved = null; //初始化变化项
            foreach (ReminderItem item in unfinishedItems)
            {
                if (!ReminderListOnwork.Contains(item) && NotFiltered(item)) // 找旧列表中不包含的项
                {
                    itemMoved = item;
                    break;
                }
            }

            Insert2ReminderList(ReminderListOnwork, itemMoved); //插入到目标列表中
            ReminderListFinished.Remove(itemMoved); //从源列表中删除

            // 由于IsCheck为双向绑定，因此不需要在这里更新ReminderManager里面的列表
        }

        /// <summary>
        /// 计算排序分数
        /// </summary>
        /// <param name="item">需要计算评分的项</param>
        /// <returns>排序分数</returns>
        private int InsertRankScore(ReminderItem item)
        {
            // 排序方式
            // 优先级：高 + 2，中 + 1，正常 + 0
            // 是否过期：过期 + 3，未到期 + 0

            int score = 0;

            // 优先级
            if (item.Priority == Priority.High) score += 2;
            else if (item.Priority == Priority.Middle) score += 1;

            // 任务完成状态
            if (item.TaskState == TaskState.OutOfDate) score += 3;

            return score;
        }

        /// <summary>
        /// 将ReminderItem插入到对应的ReminderList中
        /// </summary>
        /// <param name="reminderList">要插入的列表</param>
        /// <param name="insertItem">要插入的项</param>
        private void Insert2ReminderList(IList<ReminderItem> reminderList, ReminderItem insertItem)
        {
            // 排序方式
            // 优先级：高+2，中+1，正常+0
            // 是否过期：过期+3，未到期+0

            // 计算本项得分
            int insertItemScore = InsertRankScore(insertItem);

            // 计算前面的项数
            int itemsBefore = reminderList
                .Where(item => InsertRankScore(item) > insertItemScore)
                .ToList()
                .Count();
            reminderList.Insert(itemsBefore, insertItem);
        }

        private void ManageRemindItemButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ReminderItemsManagePage));
        }

        delegate void FilterReminderItems(AutoSuggestBox sender, IList<ReminderItem> unfinishedItems, IList<ReminderItem> finishedItems);

        private void OnworkItemSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            //获取列表
            List<ReminderItem> unfinishedItems = new(); //未完成项
            ReminderManager.GetList(unfinishedItems, TaskState.Onwork);
            ReminderManager.GetList(unfinishedItems, TaskState.OutOfDate);

            List<ReminderItem> finishedItems = new(); //已完成项
            ReminderManager.GetList(finishedItems, TaskState.Finished);

            FilterReminderItems filter; //声明委托
            List<string> suggestList = new List<string>(); //AutoSuggest项目列表

            if (ItemsSortComboBox.SelectedIndex == 1)
            {
                //按照名称排序
                List<ReminderItem> list = new();
                ReminderManager.GetList(list);

                sender.ItemsSource = list
                    .Where(item => item.Title.Contains(sender.Text))
                    .Select(item => item.Title)
                    .ToList();

                filter = new FilterReminderItems(FilterReminderItemsByName);
            }
            else
            {
                //按照标签排序
                ReminderManager.GetTagList(suggestList);
                sender.ItemsSource = suggestList.Where(item => item.Contains(sender.Text)).ToList();

                filter = new FilterReminderItems(FilterReminderItemsByTag);
            }

            filter(sender, unfinishedItems, finishedItems); // 使用委托
        }

        private void FilterReminderItemsByName(AutoSuggestBox sender, IList<ReminderItem> unfinishedItems, IList<ReminderItem> finishedItems)
        {
            //根据名称分别筛选
            unfinishedItems = unfinishedItems.Where(item => item.Title.Contains(sender.Text)).ToList();
            finishedItems = finishedItems.Where(item => item.Title.Contains(sender.Text)).ToList();

            //应用筛选
            ApplyFilteredResults(ReminderListOnwork, unfinishedItems);
            ApplyFilteredResults(ReminderListFinished, finishedItems);
        }

        private void FilterReminderItemsByTag(AutoSuggestBox sender, IList<ReminderItem> unfinishedItems, IList<ReminderItem> finishedItems)
        {
            //根据tag分别筛选
            unfinishedItems = unfinishedItems.Where(item => item.TagsString.Contains(sender.Text)).ToList();
            finishedItems = finishedItems.Where(item => item.TagsString.Contains(sender.Text)).ToList();

            //应用筛选
            ApplyFilteredResults(ReminderListOnwork, unfinishedItems);
            ApplyFilteredResults(ReminderListFinished, finishedItems);
        }

        /// <summary>
        /// 用于判断输入的ReminderItem是否按照条件被过滤
        /// </summary>
        /// <param name="item">要判断的ReminderItem</param>
        /// <returns>是否被过滤</returns>
        private bool NotFiltered(ReminderItem item)
        {
            if (ItemsSortComboBox.SelectedIndex == 0)
            {
                //按照标签
                return item.TagsString.Contains(OnworkItemSuggestBox.Text); //包含tag说明没有被过滤
            }
            else
            {
                //按照名字
                return item.Title.Contains(OnworkItemSuggestBox.Text); //包含说明没有被过滤
            }
        }

        /// <summary>
        /// 根据筛选结果在列表中显示（保留动画）
        /// </summary>
        /// <param name="filteredItems"></param>
        private void ApplyFilteredResults(ObservableCollection<ReminderItem> targetList, IList<ReminderItem> filteredItems)
        {
            //遍历列表元素，删除不包含在目标列表中的元素
            for (int i = targetList.Count - 1; i >= 0; i--)
            {
                if (!filteredItems.Contains(targetList[i])) targetList.RemoveAt(i);
            }

            //遍历目标元素，添加不包含在列表中的元素
            foreach (ReminderItem item in filteredItems)
            {
                if (!targetList.Contains(item)) targetList.Add(item);
            }
        }

        private void ItemsSortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnworkItemSuggestBox.Text = "";
        }

        private void GetListOnwork() // 用于筛选Onwork状态的item
        {
            //ReminderListOnwork.Clear();
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
            IsPageAlive = false;
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
