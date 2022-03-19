using System;
using System.Collections.Generic;
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
using System.Text.Json;
using Chamberlain_UWP.Reminder;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Chamberlain_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void DebugOptionExpander_Expanding(Microsoft.UI.Xaml.Controls.Expander sender, Microsoft.UI.Xaml.Controls.ExpanderExpandingEventArgs args)
        {
            // 调试选项折页展开
            List<ReminderItem> reminderList = new List<ReminderItem>();
            ReminderManager.GetList(reminderList);
            //reminderList.ForEach(reminder_item => JsonTextBox.Text += JsonSerializer.Serialize<ReminderItem>(reminder_item)+'\n');
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
                IncludeFields = true,
            };
            ReminderJsonTextBox.Text = JsonSerializer.Serialize(reminderList,options);
        }

        private void ImportReminderButton_Click(object sender, RoutedEventArgs e)
        {
            ImportReminderInfoBar.Visibility = Visibility.Visible;
            string msg = ReminderManager.ImportByJson(ImportReminderTextBox.Text);

            if (string.IsNullOrEmpty(msg))
            {
                ImportReminderInfoBar.Title = "导入成功";
                ImportReminderInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
            }
            else
            {
                ImportReminderInfoBar.Title = "导入失败";
                ImportReminderInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
            }
            ImportReminderInfoBar.Message = msg;

            ImportReminderInfoBar.IsOpen = true;
        }
    }
}
