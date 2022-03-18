using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class ReminderItemsManagePage : Page
    {
        ObservableCollection<string> TagList = new ObservableCollection<string>(); // 标签列表
        ObservableCollection<ReminderItem> ReminderList = new ObservableCollection<ReminderItem>(); // 所有reminder的列表

        public ReminderItemsManagePage()
        {
            this.InitializeComponent();

            ReminderManager.GetTagCollection(TagList);
            ReminderManager.GetList(ReminderList);

            ReminderManager.SortCollectionByTaskState(ReminderList);
            ReminderManager.UpdateList(ReminderList);
        }

        private void TagListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object[] tags = TagListBox.SelectedItems.ToArray();
            SelectedTagsTextBlock.Text = string.Join(',', tags);
        }

        private void AddTagButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(AddTagTextBox.Text)) TagList.Add(AddTagTextBox.Text);
            AddTagTextBox.Text = "";
            UpdateItemsProgress();
            ReminderManager.SortCollectionByTaskState(ReminderList);
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
        }

        private void ModifyItemButton_Click(object sender, RoutedEventArgs e)
        {
            int index = RemindItemListView.SelectedIndex; //找到RemindItemListView中选择的元素下标
            if (index == -1) //无选中元素
            {
                RemindItemListView.SelectedItem = null;
                ClearReviseControl();
                callTeachingTip("没有选中项", "请在这个列表中选择一项进行修改🛠",RemindItemListView);
                return;
            }

            // 检测表单内容合法性
            if(string.IsNullOrEmpty(ItemReviseTitleText.Text))
            {
                callTeachingTip("标题不能为空", "否则你想怎么称呼？🤔", ItemReviseTitleText);
                return;
            }

            if(string.IsNullOrEmpty(ItemReviseDescText.Text))
            {
                callTeachingTip("描述不能为空", "创建的时候不能为空，修改的时候也不能为空捏🤗",ItemReviseDescText);
                return;
            }

            DateTimeOffset dto = (DateTimeOffset)ItemReviseDatePicker.Date; //转换为DateTimeOffset类型
            DateTime ddlDate = dto.LocalDateTime.Date; //获得本地时间
            ddlDate += ItemReviseTimePicker.Time; //计算预设的时间

            if(ddlDate - DateTime.Now <= TimeSpan.Zero)
            {
                callTeachingTip("目标时间已经过了", "穿越不了捏😵", ItemReviseTimePicker);
                return;
            }

            // 内容装填
            ReminderList[index].Title = ItemReviseTitleText.Text; //更新Title字段
            ReminderList[index].Description = ItemReviseDescText.Text; //更新Description字段
            ReminderList[index].SetDeadline(ddlDate); //按照类中的方法，更新Deadline字段

            ReminderManager.UpdateList(ReminderList); //更新列表
        }

        private void ClearReviseControl()
        {
            ItemReviseTitleText.Text = "";
            ItemReviseDescText.Text = "";
            ItemReviseDatePicker.Date = null;
            ItemReviseTimePicker.SelectedTime = null;
        }

        private void UpdateItemsProgress()
        {
            foreach (ReminderItem item in ReminderList)
            {
                item.RefreshProgress();
            }
        }

        void callTeachingTip(string title, string desc, FrameworkElement target_control)
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
                callTeachingTip("标题不能为空", "不然就找不到这个项目了😥", AddTitleTextBox);
                return;
            }

            //描述
            if (!string.IsNullOrEmpty(AddDescTextBox.Text))
                desc = AddDescTextBox.Text;
            else
            {
                callTeachingTip("描述不能为空", "可以详细描述子事件。如果没有可以直接截取标题的一部分作为描述😂", AddDescTextBox);
                return;
            }

            //日期和时间
            DateTime ddlDate;
            if (AddItemDatePicker.Date==null)
            {
                callTeachingTip("日期不能为空", "ddl日期必须要有📅", AddItemDatePicker);
                return;
            }
            else if (AddItemTimePicker.SelectedTime == null)
            {
                callTeachingTip("时间不能为空", "还是选一个ddl时间吧⏰", AddItemTimePicker);
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
                callTeachingTip("目标时间已经过了", "穿越不了捏😵", AddItemTimePicker);
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
                callTeachingTip("标签不能为空", "请在列表中选择一个或多个标签，也可以在下方新建标签🏷", TagListBox);
                return;
            }

            ReminderItem item = new ReminderItem(title, desc, tags, ddlDate, 0); // 新建ReminderItem
            ReminderList.Insert(0, item);

            ReminderManager.UpdateList(ReminderList); //更新存放在类中的列表

            //清除表单数据
            AddTitleTextBox.Text = "";
            AddDescTextBox.Text = "";
            AddItemTimePicker.SelectedTime = null;
        }

        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            int i = RemindItemListView.SelectedIndex;
            ReminderList.RemoveAt(i);
            ReminderManager.UpdateList(ReminderList);
        }

    }
}
