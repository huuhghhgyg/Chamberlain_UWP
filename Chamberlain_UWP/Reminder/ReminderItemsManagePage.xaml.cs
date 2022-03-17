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
            ReminderManager.GetTags(TagList);
            ReminderManager.GetReminderList(ReminderList);
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
        }

        private void ItemSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            List<string> itemsName = new List<string>();
            ReminderList.ToList().ForEach(item => itemsName.Add(item.Title));
            sender.ItemsSource = itemsName.Where(item => item.Contains(sender.Text)).ToList();
        }

        private void RemindItemGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = RemindItemGridView.SelectedIndex; // 找到RemindItemGridView中选择的元素下标
            if (index == -1)
            {
                RemindItemGridView.SelectedItem = null;
                ClearReviseControl();
                return;
            }

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
            int index = RemindItemGridView.SelectedIndex; //找到RemindItemGridView中选择的元素下标
            if (index == -1)
            {
                RemindItemGridView.SelectedItem = null;
                ClearReviseControl();
                return;
            }

            ReminderList[index].Title = ItemReviseTitleText.Text; //更新Title字段
            ReminderList[index].Description = ItemReviseDescText.Text; //更新Description字段

            DateTimeOffset dto = (DateTimeOffset)ItemReviseDatePicker.Date; //转换为DateTimeOffset类型
            DateTime ddlDate = dto.LocalDateTime.Date; //获得本地时间
            ddlDate += ItemReviseTimePicker.Time; //计算预设的时间

            ReminderList[index].SetDeadline(ddlDate); //按照类中的方法，更新Deadline字段

            ReminderManager.UpdateReminderList(ReminderList); //更新列表

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

    }
}
