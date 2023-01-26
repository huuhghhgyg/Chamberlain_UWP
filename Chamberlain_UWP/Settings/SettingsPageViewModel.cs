using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.IO;
using Windows.Storage.AccessCache;
using Windows.Storage;
using System.Windows.Input;
using Windows.ApplicationModel;
using Chamberlain_UWP.Reminder;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.DataTransfer;

namespace Chamberlain_UWP.Settings
{
    internal class SettingsPageViewModel : ViewModelBase
    {
        internal SettingsPageViewModel()
        {
            LoadExternalReminderFolder();
        }
        #region 属性和内部变量
        // 应用设置
        internal int UpdateTriggerInterval
        {
            get { return SettingsConfig.UpdateTriggerInterval; }
            set { SettingsConfig.UpdateTriggerInterval = value; }
        }
        internal bool IsNotificationEnabled
        {
            get { return SettingsConfig.IsNotificationEnabled; }
            set { SettingsConfig.IsNotificationEnabled = value; }
        }
        internal bool IsNotificationBlockingVisible
        {
            get { return SettingsConfig.IsNotificationBlockingVisible; }
            set { SettingsConfig.IsNotificationBlockingVisible = value; }
        }
        internal int TimepickerInterval
        {
            get { return SettingsConfig.TimepickerInterval; }
            set { SettingsConfig.TimepickerInterval = value; }
        }
        internal bool IsRemindOnTimeEnabled
        {
            get { return SettingsConfig.IsRemindOnTimeEnabled; }
            set { SettingsConfig.IsRemindOnTimeEnabled = value; }
        }
        internal TimeSpan RemindTime
        {
            get { return SettingsConfig.RemindTime; }
            set { SettingsConfig.RemindTime = value; }
        }
        internal string CheckUpdate
        {
            get { return SettingsConfig.CheckUpdate; }
            set
            {
                SettingsConfig.CheckUpdate = value;
                OnPropertyChanged(nameof(UpdateStatusString));
            }
        }
        internal bool IsCheckUpdateEnabled //CheckUpdate的附属类
        {
            get { return CheckUpdate == "auto"; }
            set { CheckUpdate = value ? "auto" : "false"; }
        }
        string _selectedFolderPath = "";
        internal string SelectedFolderPath
        {
            get => _selectedFolderPath;
            set
            {
                _selectedFolderPath = value;
                OnPropertyChanged(nameof(SelectedFolderPath));
            }
        }
        internal string AppFolderPath
        {
            get => DataSettings.AppFolder.Path;
        }
        internal string ProgramVersion
        {
            get
            {
                PackageVersion version = Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }
        internal string ProgramInstalledDate
        {
            get => Package.Current.InstalledDate.ToString("D");
        }
        /// <summary>
        /// 数据漫游开启状态
        /// </summary>
        internal bool IsSettingsRoamingEnabled
        {
            get { return SettingsConfig.IsSettingsRoamingEnabled; }
            set { SettingsConfig.IsSettingsRoamingEnabled = value; }
        }

        /// <summary>
        /// 更新状态文本
        /// </summary>
        internal string UpdateStatusString
        {
            get
            {
                string hintString = "检测更新状态：";
                switch (CheckUpdate)
                {
                    case "auto":
                        return $"{hintString}开启应用时自动检查更新";
                    case "false":
                        return $"{hintString}不检查更新";
                    default:
                        return $"{hintString}不提示版本{CheckUpdate}的更新";
                }
            }
        }
        bool _clearFolderPathButtonEnabled = true;
        /// <summary>
        /// 清除在指定位置存放Reminder数据
        /// </summary>
        internal bool ClearFolderPathButtonEnabled
        {
            get => _clearFolderPathButtonEnabled;
            set
            {
                _clearFolderPathButtonEnabled = value;
                OnPropertyChanged(nameof(ClearFolderPathButtonEnabled));
            }
        }
        internal string ReminderJsonText
        {
            get
            {
                List<ReminderItem> reminderList = new List<ReminderItem>();
                ReminderManager.GetList(reminderList);
                return ReminderManager.GenerateJson();
            }
        }

        InfoBar _importReminderInfoBar = new InfoBar();
        /// <summary>
        /// 导入ReminderJson的InfoBar
        /// </summary>
        internal InfoBar ImportReminderInfoBar
        {
            get => _importReminderInfoBar;
            set
            {
                _importReminderInfoBar = value;
                OnPropertyChanged(nameof(ImportReminderInfoBar));
            }
        }

        string _importReminderText = string.Empty;
        internal string ImportReminderText
        {
            get => _importReminderText;
            set
            {
                _importReminderText = value;
                OnPropertyChanged(nameof(ImportReminderText));
            }
        }

        string _importReminderJsonText = string.Empty;
        /// <summary>
        /// 打开Expander时读取的ReminderJson文本
        /// </summary>
        internal string ImportReminderJsonFileText
        {
            get => _importReminderJsonText;
            set
            {
                _importReminderJsonText = value;
                OnPropertyChanged(nameof(ImportReminderJsonFileText));
            }
        }

        string _reminderSavePath = string.Empty;
        /// <summary>
        /// 手动点击保存后得到的App目录内的ReminderJson文件路径
        /// </summary>
        internal string ReminderSavePath
        {
            get => _reminderSavePath;
            set
            {
                _reminderSavePath = value;
                OnPropertyChanged(nameof(ReminderSavePath));
            }
        }

        #endregion


        #region 函数
        internal void OnNavigatedTo()
        {
            ImportReminderJsonFileText = string.Empty; //清除导入状态
            DeleteReminderDataText = string.Empty; //清除删除状态
        }
        internal void ResetUpdateState() => CheckUpdate = "auto";
        /// <summary>
        /// 读取提示文字
        /// </summary>
        private async void LoadExternalReminderFolder()
        {
            //Reminder文件夹权限
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("ReminderFolderToken"))
            {
                StorageFolder future_folder;
                try
                {
                    future_folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("ReminderFolderToken");
                    SelectedFolderPath = string.Format($"已启用，具有访问权限：{future_folder.Path}");
                    ClearFolderPathButtonEnabled = true;
                }
                catch (FileNotFoundException)
                {
                    StorageApplicationPermissions.FutureAccessList.Remove("ReminderFolderToken"); //指定文件夹不存在，清除指定项
                    SelectedFolderPath = "指定文件夹不存在，已被清除";
                    ClearFolderPathButtonEnabled = false;
                }
            }
            else
            {
                SelectedFolderPath = "未指定任何文件夹";
            }
        }

        internal async void ClearBackupListData()
        {
            ContentDialog clearBackupDataDialog = new ContentDialog()
            {
                Title = "确认",
                Content = "是否要清除备份模块的数据？保存于备份文件夹中的数据将不会被清除",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Close
            };

            var result = await clearBackupDataDialog.ShowAsync();

            if (result == ContentDialogResult.Primary) //确认清除
            {
                await DataSettings.DeleteFile(DataSettings.AppFolder, DataSettings.BackupJsonName);
                await DataSettings.DeleteFile(DataSettings.AppFolder, DataSettings.SaveJsonName);
                await DataSettings.DeleteFile(DataSettings.AppFolder, DataSettings.BackupTaskJsonName);
            }
        }
        /// <summary>
        /// 停止将Reminder数据保存在指定文件夹
        /// </summary>
        internal void ClearFolderPath()
        {
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("ReminderFolderToken")) //判断是否包含项，包含才删除
            {
                StorageApplicationPermissions.FutureAccessList.Remove("ReminderFolderToken");
                ClearFolderPathButtonEnabled = false;
                SelectedFolderPath = "未指定任何文件夹";
            }
        }
        /// <summary>
        /// 调试选项折页展开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal void DebugOptionExpanding() => OnPropertyChanged(nameof(ReminderJsonText));

        internal async void ImportReminder()
        {
            ImportReminderInfoBar.Visibility = Visibility.Visible;
            ImportReminderInfoBar.IsOpen = true;
            OnPropertyChanged(nameof(ImportReminderInfoBar));

            if (string.IsNullOrEmpty(ImportReminderText))
            {
                ImportReminderInfoBar.Title = "未检测到内容";
                ImportReminderInfoBar.Message = "请填写有意义的文本内容";
                ImportReminderInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning;
            }
            else
            {
                string msg = ReminderManager.ImportByJsonAsync(ImportReminderText);

                if (string.IsNullOrEmpty(msg))
                {
                    ImportReminderInfoBar.Title = "导入成功";
                    ImportReminderInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
                    ImportReminderText = string.Empty;
                    await ReminderManager.Data.Save();
                }
                else
                {
                    ImportReminderInfoBar.Title = "导入失败";
                    ImportReminderInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
                }
                ImportReminderInfoBar.Message = msg;
            }
        }

        /// <summary>
        /// 从JSON文件中读取数据
        /// </summary>
        internal async void OpenReminderJsonFile()
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
                    ImportReminderJsonFileText = "✅导入成功";
                    await ReminderManager.Data.Save();
                }
                else ImportReminderJsonFileText = msg; // 导入失败
            }
        }

        internal async void ReminderSave()
        {
            ReminderSavePath = await ReminderManager.Data.Save();
        }

        internal string _deleteReminderDataText = string.Empty;
        internal string DeleteReminderDataText
        {
            get => _deleteReminderDataText;
            set
            {
                _deleteReminderDataText = value;
                OnPropertyChanged(nameof(DeleteReminderDataText));
            }
        }

        internal async void DeleteButtonFlyout()
        {
            ContentDialog deleteDataDialog = new ContentDialog
            {
                Title = "您正在尝试删除Reminder的数据",
                Content = "删除的数据不可恢复，确定删除？",
                PrimaryButtonText = "确定",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Secondary
            };
            ContentDialogResult result = await deleteDataDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                DeleteReminderData();
            }
        }

        internal async void DeleteReminderData()
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
                    ClearFolderPath();// 相当于先取消使用外部数据文件

                    ReminderManager.Data.Clear(); // 取消完再删除
                    DeleteReminderDataText = "🗑已删除内部数据文件";
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    ReminderManager.Data.Clear(); // 删除数据
                    await ReminderManager.Data.Save(); // 保存空文件
                    DeleteReminderDataText = "🗑内部数据文件和外部数据文件均已清空";
                }
            }
            else
            {
                // 不使用外部数据，直接删除
                ReminderManager.Data.Clear();
                DeleteReminderDataText = "🗑已删除";
            }
        }

        internal async void SetReminderExternalFolder()
        {
            // 获取文件对象
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                SelectedFolderPath = "选取的文件夹: " + folder.Path;

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
                        await ReminderManager.Data.Save(); //保存文件（包括本地目录）

                        ClearFolderPathButtonEnabled = true; //开启清除按钮
                    }
                    else if (result == ContentDialogResult.Secondary)
                    {
                        // 应用程序现在有了这个文件夹的所有权限，包括子文件夹
                        StorageApplicationPermissions.FutureAccessList.AddOrReplace("ReminderFolderToken", folder);

                        await ReminderManager.Data.Save(); //覆盖文件

                        ClearFolderPathButtonEnabled = true; //开启清除按钮
                    }
                    else //操作取消
                    {
                        if (StorageApplicationPermissions.FutureAccessList.ContainsItem("ReminderFolderToken")) //本来有路径
                        {
                            folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("ReminderFolderToken");
                            SelectedFolderPath = String.Format("取消导入，" +
                                $"路径：{folder.Path}");
                        }
                        else SelectedFolderPath = "操作取消";
                    }
                }
                else //选定的文件夹中没有文件，直接保存
                {
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("ReminderFolderToken", folder);
                    await ReminderManager.Data.Save();
                    ClearFolderPathButtonEnabled = true; //开启清除按钮
                }
            }
            else SelectedFolderPath = "操作取消";
        }

        #endregion

        #region 事件处理
        //事件处理
        internal void ImportReminderTextBox_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy; // 声明拖拽支持文件复制操作
            e.DragUIOverride.Caption = "将Json文件拖到此处"; // 自定义拖拽提示
        }
        internal async void ImportReminderTextBox_Drop(object sender, DragEventArgs e)
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
                        ImportReminderText = await File.ReadAllTextAsync(jsonFile.Path);
                        await jsonFile.DeleteAsync();
                    }
                }
            }
        }

        internal void SettingSyncSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch control = (ToggleSwitch)sender;
            if (control.IsOn == true) SettingsConfig.SaveAllRoaming();
        }
        #endregion
    }
}
