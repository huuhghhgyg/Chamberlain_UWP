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
using Chamberlain_UWP.Settings;
using Windows.Storage.AccessCache;

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

            Settings_LoadText();
        }

        private async void Settings_LoadText() //读取提示文字
        {
            //Reminder文件夹权限
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("ReminderFolderToken"))
            {
                StorageFolder future_folder;
                try
                {
                    future_folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("ReminderFolderToken");
                    SelectedFolderPathText.Text = string.Format($"已启用，具有访问权限：{future_folder.Path}");
                }
                catch (FileNotFoundException)
                {
                    StorageApplicationPermissions.FutureAccessList.Remove("ReminderFolderToken"); //指定文件夹不存在，清除指定项
                    SelectedFolderPathText.Text = "指定文件夹不存在，已被清除";
                }
            }
            else
            {
                SelectedFolderPathText.Text = "未指定任何文件夹";
            }
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
            ImportReminderInfoBar.IsOpen = true;

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
                    if (contentType == "application/json")
                    {
                        StorageFile jsonFile = await storageFile.CopyAsync(folder, "ReminderImport.json", NameCollisionOption.ReplaceExisting); // 得到文件的引用
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

        private async void DeleteReminderDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("ReminderFolderToken")) //检查是否使用外部数据
            {
                // 使用外部数据，询问是否删除
                ContentDialog deleteDataDialog = new ContentDialog
                {
                    Title = "您正在使用指定文件夹中的数据",
                    Content = "是否一并删除？",
                    PrimaryButtonText = "保留",
                    SecondaryButtonText = "删除",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                };
                ContentDialogResult result = await deleteDataDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    object obj = null; RoutedEventArgs args = null;
                    DeleteSelectedFolderButton_Click(obj, args); // 相当于先取消使用外部数据文件

                    ReminderManager.Data.Clear(); // 取消完再删除
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    ReminderManager.Data.Clear(); // 删除数据
                    await ReminderManager.Data.Save(); // 保存空文件
                }
            }
            else
            {
                // 不使用外部数据，直接删除
                ReminderManager.Data.Clear();
                DeleteReminderDataButton.Content = "已删除";
                DeleteFlyoutButton.Flyout.Hide();
            }
        }

        private void DeleteFlyoutButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteReminderDataButton.Content = "确认删除";
        }

        /// <summary>
        /// 数据绑定区
        /// </summary>
        // 应用设置
        private int UpdateTriggerInterval
        {
            get { return SettingsConfig.UpdateTriggerInterval; }
            set { SettingsConfig.UpdateTriggerInterval = value; }
        }
        private bool IsNotificationEnabled
        {
            get { return SettingsConfig.IsNotificationEnabled; }
            set { SettingsConfig.IsNotificationEnabled = value; }
        }
        private bool IsNotificationBlockingVisible
        {
            get { return SettingsConfig.IsNotificationBlockingVisible; }
            set { SettingsConfig.IsNotificationBlockingVisible = value; }
        }
        //数据漫游
        private bool IsSettingsRoamingEnabled
        {
            get { return SettingsConfig.IsSettingsRoamingEnabled; }
            set { SettingsConfig.IsSettingsRoamingEnabled = value; }
        }

        private void SettingSyncSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch control = (ToggleSwitch)sender;
            if (control.IsOn == true) SettingsConfig.SaveAllRoaming();
        }

        private async void SetReminderFolderButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取文件对象
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                SelectedFolderPathText.Text = "选取的文件夹: " + folder.Path;

                // 检测文件夹内是否有数据文件
                StorageFile file = await folder.TryGetItemAsync(ReminderManager.DataFilename) as StorageFile;
                if (file != null)
                {
                    //文件存在
                    ContentDialog importDataDialog = new ContentDialog
                    {
                        Title = "检测到选定的文件夹内存在数据文件",
                        Content = "从这个文件内导入，还是覆盖这个文件？",
                        PrimaryButtonText = "导入",
                        SecondaryButtonText = "覆盖",
                        CloseButtonText = "取消",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    ContentDialogResult result = await importDataDialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        // 应用程序现在有了这个文件夹的所有权限，包括子文件夹
                        StorageApplicationPermissions.FutureAccessList.AddOrReplace("ReminderFolderToken", folder);

                        await ReminderManager.Data.Load(); //从文件导入
                    }
                    else if (result == ContentDialogResult.Secondary)
                    {
                        // 应用程序现在有了这个文件夹的所有权限，包括子文件夹
                        StorageApplicationPermissions.FutureAccessList.AddOrReplace("ReminderFolderToken", folder);

                        await ReminderManager.Data.Save(); //覆盖文件
                    }
                    else //操作取消
                    {
                        if (StorageApplicationPermissions.FutureAccessList.ContainsItem("ReminderFolderToken")) //本来有路径
                        {
                            folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("ReminderFolderToken");
                            SelectedFolderPathText.Text = String.Format("取消导入，" +
                                $"路径：{folder.Path}");
                        }
                        else SelectedFolderPathText.Text = "操作取消";
                    }
                }
                else //选定的文件夹中没有文件，直接保存
                {
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("ReminderFolderToken", folder);
                    await ReminderManager.Data.Save(); 
                }
            }
            else SelectedFolderPathText.Text = "操作取消";
        }

        private void DeleteSelectedFolderButton_Click(object sender, RoutedEventArgs e)
        {
            StorageApplicationPermissions.FutureAccessList.Remove("ReminderFolderToken");
            DeleteSelectedFolderButton.IsEnabled = false;
            SelectedFolderPathText.Text = "未指定任何文件夹";
        }
    }
}