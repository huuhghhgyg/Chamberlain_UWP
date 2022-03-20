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
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

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
            ReminderJsonTextBox.Text = ReminderManager.GenerateJson();
        }

        private async void ImportReminderButton_Click(object sender, RoutedEventArgs e)
        {
            ImportReminderInfoBar.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(ImportReminderTextBox.Text))
            {
                ImportReminderInfoBar.Title = "未检测到内容";
                ImportReminderInfoBar.Message = "请填写有意义的文本内容";
                ImportReminderInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning;
            }
            else
            {
                string msg = ReminderManager.ImportByJsonAsync(ImportReminderTextBox.Text);

                if (string.IsNullOrEmpty(msg))
                {
                    ImportReminderInfoBar.Title = "导入成功";
                    ImportReminderInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
                    await ReminderManager.Data.Save();
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

        // 拖拽到文本框
        private void ImportReminderTextBox_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy; // 声明拖拽支持文件复制操作
            e.DragUIOverride.Caption = "将Json文件拖到此处"; // 自定义拖拽提示
        }

        private async void ImportReminderTextBox_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems)) // 判断是否是文件
            {
                var items = await e.DataView.GetStorageItemsAsync();

                if (items.Any()) // 有条目
                {
                    var storageFile = items[0] as StorageFile; // 可能拖拽了多个文件，只获取多个文件中的第一个
                    var contentType = storageFile.ContentType; // 获取拖拽文件的类型

                    StorageFolder folder = ApplicationData.Current.TemporaryFolder;// 程序临时目录
                    // folder.Path为本地目录路径
                    if (contentType== "application/json")
                    {
                        StorageFile jsonFile = await storageFile.CopyAsync(folder, "ReminderImport.json",NameCollisionOption.ReplaceExisting); // 得到文件的引用
                        ImportReminderTextBox.Text = await File.ReadAllTextAsync(jsonFile.Path);
                        await jsonFile.DeleteAsync();
                    }
                }
            }
        }

        private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop; // 起始位置
            picker.FileTypeFilter.Add(".json");

            StorageFile file = await picker.PickSingleFileAsync();
            // Application now has read/write access to the picked file

            if (file != null) // 文件handle不为空
            {
                string msg = await ReminderManager.Data.LoadFile(file);
                if (string.IsNullOrEmpty(msg))
                {
                    OpenFileTextBlock.Text = "✅导入成功";
                    await ReminderManager.Data.Save();
                }
                else OpenFileTextBlock.Text = msg; // 导入失败
            }
        }

        private async void ReminderSave_Click(object sender, RoutedEventArgs e)
        {
            ReminderSaveTextBox.Text = await ReminderManager.Data.Save();
        }

        private void DeleteReminderDataButton_Click(object sender, RoutedEventArgs e)
        {
            ReminderManager.Data.Clear();
            DeleteReminderDataButton.Content = "已删除";
        }

        private void DeleteFlyoutButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteReminderDataButton.Content = "确认删除";
        }
    }
}
