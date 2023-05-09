using Chamberlain_UWP.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Chamberlain_UWP.Reminder
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ReminderItemsManagePage : Page, INotifyPropertyChanged
    {
        ObservableCollection<string> TagList = new ObservableCollection<string>(); // 标签列表
        ObservableCollection<ReminderItem> ReminderList = new ObservableCollection<ReminderItem>(); // 所有reminder的列表

        bool IsPageAlive = true; // 确认页面是否被Unload
        bool IsProgressUpdaterWorking = false; // 进程锁，防止多次创建进程
        private int TimepickerInterval
        {
            get { return SettingsConfig.TimepickerInterval; }
            set { SettingsConfig.TimepickerInterval = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ReminderItemsManagePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            IsPageAlive = true;

            ReminderManager.SortListByDefault(); // 对List先进行排序

            TagList.Clear();
            ReminderList.Clear();

            ReminderManager.GetTagCollection(TagList);
            ReminderManager.GetList(ReminderList);

            UI_Initialize(); //对UI进行初始化设定

            new Thread(RefreshData).Start(); // 更新进度
        }

        private void UI_Initialize()
        {
            AddItemDatePicker.Date = DateTime.Today; // 将添加项的时间设为今天，方便添加
            TimepickerInterval = SettingsConfig.TimepickerInterval;
            NotifyPropertyChanged("TimepickerInterval");
        }

        private async void RefreshData() //修改页面的线程睡眠时间稍短，适应任务变化
        {
            IsProgressUpdaterWorking = true; // 上锁
            while (IsPageAlive)
            {
                if (ReminderManager.ItemCountRunning > 0) //检测是否存在需要更新进度的条目
                {
                    int thread_sleep_span = 1200; //线程最长休眠时间
                    if (ReminderManager.UpdateTimeSpan <= thread_sleep_span) // 如果小于最长休眠时间，按照小的来
                        thread_sleep_span = ReminderManager.UpdateTimeSpan;

                    Thread.Sleep(thread_sleep_span); // 根据列表中项的最小时间间隔来计算
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

        private void TagListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object[] tags = TagListBox.SelectedItems.ToArray();
            if (tags.Length > 0)
                SelectedTagsTextBlock.Text = string.Join(',', tags);
            else
                SelectedTagsTextBlock.Text = ResourceLoader.GetString("（未选择）");
        }

        private void AddTagButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(AddTagTextBox.Text)) TagList.Add(AddTagTextBox.Text);
            AddTagTextBox.Text = "";
            UpdateItemsProgress();
        }

        private void ItemSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            List<string> itemsName = new List<string>();
            ReminderList.ToList().ForEach(item => itemsName.Add(item.Title));
            sender.ItemsSource = itemsName.Where(item => item.Contains(sender.Text)).ToList();
        }

        private void ItemSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            int i = ReminderManager.FindItemWithTitle(args.SelectedItem.ToString());
            RemindItemListView.SelectedIndex = i; // 选中item
        }

        private void RemindItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 更新UI反馈
            revise_num = 0;
            ReviseItemInfoBadge.Value = -1;
            ReviseItemInfoBadge.Visibility = Visibility.Collapsed;

            int index = RemindItemListView.SelectedIndex; // 找到RemindItemListView中选择的元素下标
            if (index == -1)
            {
                RemindItemListView.SelectedItem = null;
                ClearReviseControl();
                return;
            }

            // 更新元素属性
            ItemReviseTitleText.Text = ReminderList[index].Title;
            ItemReviseDescText.Text = ReminderList[index].Description;
            ItemReviseDatePicker.Date = ReminderList[index].Deadline;

            int h = ReminderList[index].Deadline.Hour;
            int m = ReminderList[index].Deadline.Minute;
            int s = ReminderList[index].Deadline.Second;
            ItemReviseTimePicker.SelectedTime = new TimeSpan(h, m, s);

            ItemRevisePriorityComboBox.SelectedIndex = (int)ReminderList[index].Priority;
            CreatedTimeTextBlock.Text = ReminderList[index].CreatedTime.ToString("f");
        }

        int revise_num = 0;

        private void ModifyItemButton_Click(object sender, RoutedEventArgs e)
        {
            int index = RemindItemListView.SelectedIndex; //找到RemindItemListView中选择的元素下标
            if (index == -1) //无选中元素
            {
                RemindItemListView.SelectedItem = null;
                ClearReviseControl();
                CallTeachingTip(ResourceLoader.GetString("没有选中项"), ResourceLoader.GetString("请在这个列表中选择一项进行修改"),RemindItemListView);
                return;
            }

            // 检测表单内容合法性
            if(string.IsNullOrEmpty(ItemReviseTitleText.Text))
            {
                CallTeachingTip(ResourceLoader.GetString("标题不能为空"), ResourceLoader.GetString("否则你想怎么称呼？🤔"), ItemReviseTitleText);
                return;
            }

            if(string.IsNullOrEmpty(ItemReviseDescText.Text))
            {
                ItemReviseDescText.Text = ResourceLoader.GetString("(无描述)");
            }

            DateTimeOffset dto = (DateTimeOffset)ItemReviseDatePicker.Date; //转换为DateTimeOffset类型
            DateTime ddlDate = dto.LocalDateTime.Date; //获得本地时间
            ddlDate += ItemReviseTimePicker.Time; //计算预设的时间

            if(ddlDate - DateTime.Now <= TimeSpan.Zero)
            {
                CallTeachingTip(ResourceLoader.GetString("目标时间已经过了"), ResourceLoader.GetString("穿越不了捏"), ItemReviseTimePicker);
                return;
            }

            // 内容装填
            ReminderList[index].Title = ItemReviseTitleText.Text; //更新Title字段
            ReminderList[index].Description = ItemReviseDescText.Text; //更新Description字段
            ReminderList[index].SetDeadline(ddlDate); //按照类中的方法，更新Deadline字段
            ReminderList[index].Priority = (Priority)ItemRevisePriorityComboBox.SelectedIndex; //这项似乎没法不选，不做检测
            ReviseItemInfoBadge.Visibility = Visibility.Visible;

            //UI反馈（Badge）
            revise_num++;
            if (revise_num > 1) ReviseItemInfoBadge.Value = revise_num;

            ReminderManager.UpdateList(ReminderList); //更新列表

            // 考虑重启后台线程
            if (!IsProgressUpdaterWorking) // 判断进程是否不在工作（活动的条目=0）
                new Thread(RefreshData).Start(); // 重启更新进度的进程
        }

        private void ClearReviseControl() //清空更改控件中的内容
        {
            ItemReviseTitleText.Text = "";
            ItemReviseDescText.Text = "";
            CreatedTimeTextBlock.Text = ResourceLoader.GetString("（空）");
            ItemReviseDatePicker.Date = null; //清空选中的日期
            ItemReviseTimePicker.SelectedTime = null; //清空选中的时间
            ItemRevisePriorityComboBox.SelectedIndex = 0; //选中“默认”
            ReviseItemInfoBadge.Visibility = Visibility.Collapsed; //隐藏修改反馈图标
            revise_num = 0; //清除反馈图标的计数
        }

        private void UpdateItemsProgress()
        {
            foreach (ReminderItem item in ReminderList)
            {
                item.RefreshProgress();
            }
        }

        void CallTeachingTip(string title, string desc, FrameworkElement target_control)
        {
            AddInstructTip.Title = title;
            AddInstructTip.Subtitle = desc;
            AddInstructTip.Target = target_control;
            AddInstructTip.IsOpen = true;
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e) //添加提醒项按钮
        {
            string title, desc;

            //标题
            if (!string.IsNullOrEmpty(AddTitleTextBox.Text)) 
                title = AddTitleTextBox.Text;
            else
            {
                //CallTeachingTip("标题不能为空", "不然就找不到这个项目了😥", AddTitleTextBox);
                CallTeachingTip(ResourceLoader.GetString("标题不能为空"), ResourceLoader.GetString("不然就找不到这个项目了"), AddTitleTextBox);
                return;
            }

            //描述
            if (!string.IsNullOrEmpty(AddDescTextBox.Text))
                desc = AddDescTextBox.Text;
            else
                desc = ResourceLoader.GetString("(无描述)");

            //日期和时间
            DateTime ddlDate;
            if (AddItemDatePicker.Date==null)
            {
                CallTeachingTip(ResourceLoader.GetString("日期不能为空"), ResourceLoader.GetString("ddl日期必须要有"), AddItemDatePicker);
                return;
            }
            else if (AddItemTimePicker.SelectedTime == null)
            {
                CallTeachingTip(ResourceLoader.GetString("时间不能为空"), ResourceLoader.GetString("还是选一个ddl时间吧"), AddItemTimePicker);
                return;
            }
            else
            {
                // 日期和时间都有
                DateTimeOffset dto = (DateTimeOffset)AddItemDatePicker.Date; //转换为DateTimeOffset类型
                ddlDate = dto.LocalDateTime.Date; //获得本地时间
                ddlDate += AddItemTimePicker.Time; //计算预设的时间
            }

            if (ddlDate - DateTime.Now <= TimeSpan.Zero)
            {
                CallTeachingTip(ResourceLoader.GetString("目标时间已经过了"), ResourceLoader.GetString("穿越不了捏"), AddItemTimePicker);
                return;
            }

            List<string> tags = new List<string>();
            if (TagListBox.SelectedItems.ToList().Count > 0)
            {
                // 符合
                TagListBox.SelectedItems.ToList().ForEach(p => tags.Add(p.ToString()));
            }
            else
            {
                CallTeachingTip(ResourceLoader.GetString("标签不能为空"), ResourceLoader.GetString("请在列表中选择一个或多个标签，也可以在下方新建标签🏷"), TagListBox);
                return;
            }

            Priority priority = new Priority();
            priority = (Priority)AddItemPriorityComboBox.SelectedIndex; // 因为无法为空，所以不检查

            ReminderItem item = new ReminderItem(title, desc, tags, ddlDate, TaskState.Onwork,priority); // 新建ReminderItem
            ReminderList.Insert(0, item);

            ReminderManager.UpdateList(ReminderList); //更新存放在类中的列表

            //清除表单数据
            AddTitleTextBox.Text = "";
            AddDescTextBox.Text = "";
            AddItemTimePicker.SelectedTime = null;

            //考虑重启后台线程
            if (!IsProgressUpdaterWorking) // 判断进程是否不在工作（活动的条目=0）
                new Thread(RefreshData).Start(); // 重启更新进度的进程
        }

        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            int i = RemindItemListView.SelectedIndex;
            if(i != -1)
            {
                ReminderList.RemoveAt(i);
                ReminderManager.UpdateList(ReminderList);
                if (RemindItemListView.Items.Count >= i + 1) RemindItemListView.SelectedIndex = i;
            }
        }

        private void SortListButton_Click(object sender, RoutedEventArgs e) // 暴力刷新
        {
            ReminderManager.SortListByDefault();
            ReminderList.Clear();
            ReminderManager.GetList(ReminderList);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            IsPageAlive = false;
        }

        private async void ReminderItemRevise_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            await ReminderManager.Data.Save(); // 保存数据

            if (!IsProgressUpdaterWorking) // 判断进程是否不在工作（活动的条目=0）
                new Thread(RefreshData).Start(); // 重启更新进度的进程
        }
    }
}
