﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Chamberlain_UWP.Reminder.OOBE
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ReminderOOBEImport : Page
    {
        public ReminderOOBEImport()
        {
            this.InitializeComponent();
        }

        private void TitleTextBox_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy; // 声明拖拽支持文件复制操作
            e.DragUIOverride.Caption = "将Json文件拖到此处"; // 自定义拖拽提示
        }

        private async void TitleTextBox_Drop(object sender, DragEventArgs e)
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
                        string content = await File.ReadAllTextAsync(jsonFile.Path); // 获取文件中的内容
                        string msg = ReminderManager.ImportByJsonAsync(content);

                        // 导入判断
                        if (!string.IsNullOrEmpty(msg)) ImportFailed(msg);// 导入出错
                        else ImportSuccess(); // 导入成功

                        await jsonFile.DeleteAsync(); // 删除缓存文件
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

                // 导入判断
                if (!string.IsNullOrEmpty(msg)) ImportFailed(msg);// 导入出错
                else ImportSuccess(); // 导入成功
            }
        }

        private async void ImportSuccess()
        {
            // 导入成功
            this.Frame.Navigate(typeof(ReminderPage));
            Frame.BackStack.Clear(); // 禁止返回
            await ReminderManager.Data.Save();
        }

        private async void ImportFailed(string msg)
        {
            // 导入出错
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "导入的文件似乎有些问题🤔",
                Content = msg,
                PrimaryButtonText = "确定",
            };
            await deleteFileDialog.ShowAsync();
        }
    }
}
