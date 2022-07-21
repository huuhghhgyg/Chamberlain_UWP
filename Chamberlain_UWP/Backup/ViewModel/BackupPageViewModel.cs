using Chamberlain_UWP.Backup.Models;
using Chamberlain_UWP.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Controls;

namespace Chamberlain_UWP.Backup
{
    public static class BackupPageData
    {
        public static ObservableCollection<PathRecord> _backupPathRecords = new ObservableCollection<PathRecord>(); //备份页数据
        public static ObservableCollection<PathRecord> _savePathRecords = new ObservableCollection<PathRecord>(); //保存页数据
        //备份任务DataGrid
        public static ObservableCollection<BackupTaskData> _backupTasks = new ObservableCollection<BackupTaskData>(); //备份任务数据
        public static ObservableCollection<BackupPathString> _backupPathNames = new ObservableCollection<BackupPathString>(); //备份目录路径
        public static ObservableCollection<SavePathString> _savePathNames = new ObservableCollection<SavePathString>(); //保存目录路径
        //备份记录
        public static ObservableCollection<BackupVersionRecord> _backupVersionRecords = new ObservableCollection<BackupVersionRecord>(); //备份记录列表
    }
    public class BackupTaskData //用于DataGrid的绑定
    {
        public string BackupPath { get; set; }
        public string SavePath { get; set; }
        public BackupTaskData(PathRecord BackupFolder, PathRecord SaveFolder)
        {
            BackupPath = BackupFolder.Path;
            SavePath = SaveFolder.Path;
        }
        [JsonConstructor]
        public BackupTaskData(string backupPath, string savePath)
        {
            BackupPath = backupPath;
            SavePath = savePath;
        }
    }
    public class BackupPathString
    {
        public string BackupPath { get; set; }
        public BackupPathString(string backupPath)
        {
            BackupPath = backupPath;
        }
    }
    public class SavePathString
    {
        public string SavePath { get; set; }
        public SavePathString(string savePath)
        {
            SavePath = savePath;
        }
    }
    public class BackupPageViewModel : ViewModelBase
    {
        /// <summary>
        /// 内部变量区
        /// </summary>
        int _backupRecordComboBoxSelectedIndex = -1;
        int _backupTaskSelectedIndex = -1; //任务列表选中任务项
        internal BackupManager Manager = new BackupManager();
        bool _isBackupCardVisible = false;

        /// <summary>
        /// 属性区
        /// </summary>

        public bool IsBackupCardVisible //备份卡片是否可见
        {
            get => _isBackupCardVisible;
            set
            {
                _isBackupCardVisible = value;
                OnPropertyChanged(nameof(IsBackupCardVisible));
                OnPropertyChanged(nameof(IsNoTaskTextVisible));
            }
        }
        public bool IsNoTaskTextVisible //无任务提示是否可见
        {
            get => !IsBackupCardVisible;
        }
        public ObservableCollection<PathRecord> BackupPathRecords //ObservableCollection备份列表
        {
            get => BackupPageData._backupPathRecords;
            set
            {
                BackupPageData._backupPathRecords = value;

                OnPropertyChanged(nameof(BackupPathRecords));
                OnPropertyChanged(nameof(BackupPathNames));
            }
        }
        public int BackupPathRecordsSelectedIndex { get; set; } = -1; //选中的Index
        public ObservableCollection<PathRecord> SavePathRecords //ObservableCollection备份列表
        {
            get => BackupPageData._savePathRecords;
            set
            {
                BackupPageData._savePathRecords = value;

                OnPropertyChanged(nameof(SavePathRecords));
                OnPropertyChanged(nameof(SavePathNames));
            }
        }
        public int SavePathRecordsSelectedIndex { get; set; } = -1; //选中的Index
        public int BackupRecordComboBoxSelectedIndex //备份记录页的ComboBox选择项
        {
            get => _backupRecordComboBoxSelectedIndex;
            set
            {
                _backupRecordComboBoxSelectedIndex = value;
                OnPropertyChanged(nameof(BackupRecordComboBoxSelectedIndex));
                //OnPropertyChanged(nameof(BackupRecordPathText)); //连锁
                Manager.GetBackupVersionList(BackupRecordComboBoxSelectedPath); //获取列表
                OnPropertyChanged(nameof(BackupVersionRecords));
            }
        }
        public string BackupRecordComboBoxSelectedPath //选中项的值
        {
            get => BackupPathRecords[BackupRecordComboBoxSelectedIndex].Path;
        }
        public string BackupRecordPathText //备份记录页ComboBox选择项对应的路径
        {
            get
            {
                if (BackupRecordComboBoxSelectedIndex == -1)
                    return "暂未选中";
                else
                    return BackupPathRecords[BackupRecordComboBoxSelectedIndex].Path;
            }
        }
        public ObservableCollection<BackupTaskData> BackupTasks //ObservableCollection备份列表
        {
            get => BackupPageData._backupTasks;
            set
            {
                BackupPageData._backupTasks = value;
                OnPropertyChanged(nameof(BackupTasks));
            }
        }
        public ObservableCollection<BackupPathString> BackupPathNames
        {
            get => BackupPageData._backupPathNames;
        }
        public ObservableCollection<SavePathString> SavePathNames
        {
            get => BackupPageData._savePathNames;
        }
        public int BackupTaskSelectedIndex
        {
            get => _backupTaskSelectedIndex;
            set
            {
                _backupTaskSelectedIndex = value;
                OnPropertyChanged(nameof(BackupTaskSelectedIndex));
                //Manager.GenerateJsonAsync(Manager.BackupTaskList, Manager.BackupTaskJsonName); //保存备份任务列表
            }
        }
        public ObservableCollection<BackupVersionRecord> BackupVersionRecords
        {
            get => BackupPageData._backupVersionRecords;
            set
            {
                BackupPageData._backupVersionRecords = value;
                OnPropertyChanged(nameof(BackupVersionRecords));
            }
        }
        public int BackupVersionRecordListSelectedIndex { get; set; } = -1;

        /// <summary>
        /// 方法区
        /// </summary>

        public async void Backup_Start()
        {
            if (BackupTaskSelectedIndex != -1)
            {
                BackupTaskData selectedTask = BackupTasks[BackupTaskSelectedIndex];
                ContentDialog selectedTaskDiaglog = new ContentDialog
                {
                    Title = "选中的任务如下",
                    Content = $"\"{selectedTask.BackupPath}\" -> \"{selectedTask.SavePath}\"",
                    CloseButtonText = "Ok"
                };

                ContentDialogResult result = await selectedTaskDiaglog.ShowAsync();

                IsBackupCardVisible = true;
                Manager.RunTotalBackup(selectedTask.BackupPath, selectedTask.SavePath);
            }
        }

        public void Backup_Cancel() //备份取消
        {

        }

        public void SwitchTaskState() //更改备份状态
        {
        }

        async Task<StorageFolder> OpenFolder()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            return folder;
        }

        public async void Add2SavePathList() //添加到保存路径
        {
            StorageFolder folder = await OpenFolder();
            if (folder != null)
            {
                SavePathRecords.Add(new PathRecord(folder)); //UI
                BackupPageData._savePathNames.Add(new SavePathString(folder.Path));
                PathRecord save_path = new PathRecord(folder);
                Manager.SaveFolderList.Add(save_path);
                //添加访问权限
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(save_path.Hash, save_path.Folder); //添加访问token
                DataSettings.GenerateJsonAsync(Manager.SaveFolderList, Manager.AppFolder, Manager.SaveJsonName); //保存目标目录列表
            }
        }
        public void DelFromSavePathList() //从保存路径移除
        {
            if (SavePathRecordsSelectedIndex != -1)
            {
                // 删除访问权限
                PathRecord save_path = Manager.SaveFolderList[SavePathRecordsSelectedIndex];
                StorageApplicationPermissions.FutureAccessList.Remove(save_path.Hash); //删除访问token

                BackupPageData._savePathNames.RemoveAt(SavePathRecordsSelectedIndex);
                Manager.SaveFolderList.RemoveAt(SavePathRecordsSelectedIndex);
                SavePathRecords.RemoveAt(SavePathRecordsSelectedIndex); //UI

                DataSettings.GenerateJsonAsync(Manager.SaveFolderList, Manager.AppFolder, Manager.SaveJsonName); //保存目标目录列表
            }
        }
        public async void Add2BackupPathList() //添加到备份路径
        {
            StorageFolder folder = await OpenFolder();
            if (folder != null)
            {
                BackupPathRecords.Add(new PathRecord(folder)); //UI
                BackupPageData._backupPathNames.Add(new BackupPathString(folder.Path));
                PathRecord backup_path = new PathRecord(folder);
                Manager.BackupFolderList.Add(backup_path);
                //添加访问权限
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(backup_path.Hash, backup_path.Folder); //添加访问token
                DataSettings.GenerateJsonAsync(Manager.BackupFolderList, Manager.AppFolder, Manager.BackupJsonName); //保存备份文件列表

            }
        }
        public void DelFromBackupPathList() //从删除路径中移除
        {
            if (BackupPathRecordsSelectedIndex != -1)
            {
                // 删除访问权限
                PathRecord backup_path = Manager.BackupFolderList[BackupPathRecordsSelectedIndex];
                StorageApplicationPermissions.FutureAccessList.Remove(backup_path.Hash); //删除访问token

                BackupPageData._backupPathNames.RemoveAt(BackupPathRecordsSelectedIndex);
                Manager.BackupFolderList.RemoveAt(BackupPathRecordsSelectedIndex);
                BackupPathRecords.RemoveAt(BackupPathRecordsSelectedIndex); //UI

                DataSettings.GenerateJsonAsync(Manager.BackupFolderList, Manager.AppFolder, Manager.BackupJsonName); //保存备份文件列表
            }
        }
        public void AddBackupTask()
        {
            if (BackupPathRecords.Count > 0 && SavePathRecords.Count > 0)
            {
                BackupTaskData backupTask = new BackupTaskData(BackupPathRecords[0], SavePathRecords[0]);
                BackupTasks.Add(backupTask); //添加到ObservableCollection
                Manager.BackupTaskList.Add(backupTask); //同步到Manager
                DataSettings.GenerateJsonAsync(Manager.BackupTaskList, Manager.AppFolder, Manager.BackupTaskJsonName); //保存备份任务列表
            }
        }
        public void DelFromBackupTask()
        {
            if (BackupTaskSelectedIndex != -1)
            {
                Manager.BackupTaskList.RemoveAt(BackupTaskSelectedIndex); //先删除Manager中的数据
                BackupTasks.RemoveAt(BackupTaskSelectedIndex); //移除ObservableCollection中的条目

                DataSettings.GenerateJsonAsync(Manager.BackupTaskList, Manager.AppFolder, Manager.BackupTaskJsonName); //保存备份任务列表
            }
        }
        public void ClearErrorMessages() => Manager.ErrorMessages.Clear();

        public void RefershData()
        {
            OnPropertyChanged(nameof(BackupPathRecords));
            OnPropertyChanged(nameof(BackupPathNames));
            OnPropertyChanged(nameof(SavePathRecords));
            OnPropertyChanged(nameof(SavePathNames));
            OnPropertyChanged(nameof(BackupTasks));
        }

        public void SaveBackupTaskList()
        {
            Manager.BackupTaskList = new List<BackupTaskData>(BackupTasks);
            DataSettings.GenerateJsonAsync(Manager.BackupTaskList, Manager.AppFolder, Manager.BackupTaskJsonName); //保存备份任务列表
        }
    }
}