using Chamberlain_UWP.Backup.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Controls;

namespace Chamberlain_UWP.Backup
{
    public class BackupTaskData //用于DataGrid的绑定
    {
        public string BackupPath { get; set; }
        public string SavePath { get; set; }
        public BackupTaskData(PathRecord BackupFolder, PathRecord SaveFolder)
        {
            BackupPath = BackupFolder.Path;
            SavePath = SaveFolder.Path;
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
        string _backupTitle = "第1项，共3项"; //备份卡片标题
        string _backupDesc = "正在备份[具体的文件]"; //备份卡片描述（正在备份的文件）
        double _backupProgress = 20.0; //备份进度（max=100）
        bool _isAnyBackupTask = false; //是否正在执行备份任务

        ObservableCollection<PathRecord> _backupPathRecords = new ObservableCollection<PathRecord>(); //备份页数据
        int _backupRecordComboBoxSelectedIndex = -1;
        ObservableCollection<PathRecord> _savePathRecords = new ObservableCollection<PathRecord>(); //保存页数据
        //备份任务DataGrid
        ObservableCollection<BackupTaskData> _backupTasks = new ObservableCollection<BackupTaskData>(); //备份任务数据
        ObservableCollection<BackupPathString> _backupPathNames = new ObservableCollection<BackupPathString>();
        ObservableCollection<SavePathString> _savePathNames = new ObservableCollection<SavePathString>();
        internal BackupManager Manager = new BackupManager();

        /// <summary>
        /// 属性区
        /// </summary>
        public string BackupTitle //备份卡片标题
        {
            get { return _backupTitle; }
            set
            {
                _backupTitle = value;
                OnPropertyChanged(nameof(BackupTitle));
            }
        }
        public string BackupDescription //备份卡片描述
        {
            get { return _backupDesc; }
            set
            {
                _backupDesc = value;
                OnPropertyChanged(nameof(BackupDescription));
            }
        }
        public double BackupProgress //备份卡片进度
        {
            get { return _backupProgress; }
            set
            {
                _backupProgress = value;
                OnPropertyChanged(nameof(BackupProgress));
                //OnPropertyChanged(nameof(BackupProgressString));
            }
        }

        public bool IsBackupCardVisible //备份卡片是否可见
        {
            get => _isAnyBackupTask;
            set
            {
                _isAnyBackupTask = value;
                OnPropertyChanged(nameof(IsBackupCardVisible));
                OnPropertyChanged(nameof(IsNoTaskTextVisible));
            }
        }
        public bool IsNoTaskTextVisible //无任务提示是否可见
        {
            get => !_isAnyBackupTask;
            set
            {
                _isAnyBackupTask = value;
                OnPropertyChanged(nameof(IsBackupCardVisible));
                OnPropertyChanged(nameof(IsNoTaskTextVisible));
            }
        }
        public ObservableCollection<PathRecord> BackupPathRecords //ObservableCollection备份列表
        {
            get => _backupPathRecords;
            set
            {
                _backupPathRecords = value;

                OnPropertyChanged(nameof(BackupPathRecords));
                OnPropertyChanged(nameof(BackupPathNames));
            }
        }
        public int BackupPathRecordsSelectedIndex { get; set; } = -1; //选中的Index
        public ObservableCollection<PathRecord> SavePathRecords //ObservableCollection备份列表
        {
            get => _savePathRecords;
            set
            {
                _savePathRecords = value;
                OnPropertyChanged(nameof(SavePathRecords));
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
                OnPropertyChanged(nameof(BackupRecordPathText)); //连锁
            }
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
            get => _backupTasks;
            set
            {
                _backupTasks = value;
                OnPropertyChanged(nameof(BackupTasks));
            }
        }
        public ObservableCollection<BackupPathString> BackupPathNames
        {
            get => _backupPathNames;
        }
        public ObservableCollection<SavePathString> SavePathNames
        {
            get => _savePathNames;
        }
        public int BackupTaskSelectedIndex { get; set; } = -1; //任务列表选中任务项

        /// <summary>
        /// 方法区
        /// </summary>
        public void Backup_Suspend() //备份暂停
        {
            if (BackupProgress >= 90)
            {
                BackupProgress = 10;
            }
            else
            {
                BackupProgress += 10;
            }
        }

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

                Manager.RunTotalBackup(selectedTask.BackupPath, selectedTask.SavePath);
            }
        }

        public void Backup_Cancel() //备份取消
        {

        }

        public void SwitchTaskState() //更改备份状态
        {
            IsBackupCardVisible = !IsBackupCardVisible;
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
                _savePathNames.Add(new SavePathString(folder.Path));
                PathRecord save_path = new PathRecord(folder);
                Manager.SaveFolderList.Add(save_path);
                //添加访问权限
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(save_path.Hash, save_path.Folder); //添加访问token
            }
        }
        public void DelFromSavePathList() //从保存路径移除
        {
            if (SavePathRecordsSelectedIndex != -1)
            {
                // 删除访问权限
                PathRecord save_path = Manager.SaveFolderList[SavePathRecordsSelectedIndex];
                StorageApplicationPermissions.FutureAccessList.Remove(save_path.Hash); //删除访问token

                _savePathNames.RemoveAt(SavePathRecordsSelectedIndex);
                Manager.SaveFolderList.RemoveAt(SavePathRecordsSelectedIndex);
                SavePathRecords.RemoveAt(SavePathRecordsSelectedIndex); //UI
            }
        }
        public async void Add2BackupPathList() //添加到备份路径
        {
            StorageFolder folder = await OpenFolder();
            if (folder != null)
            {
                BackupPathRecords.Add(new PathRecord(folder)); //UI
                _backupPathNames.Add(new BackupPathString(folder.Path));
                PathRecord backup_path = new PathRecord(folder);
                Manager.BackupFolderList.Add(backup_path);
                //添加访问权限
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(backup_path.Hash, backup_path.Folder); //添加访问token
            }
        }
        public void DelFromBackupPathList() //从删除路径中移除
        {
            if (BackupPathRecordsSelectedIndex != -1)
            {
                // 删除访问权限
                PathRecord backup_path = Manager.BackupFolderList[BackupPathRecordsSelectedIndex];
                StorageApplicationPermissions.FutureAccessList.Remove(backup_path.Hash); //删除访问token

                BackupPathNames.RemoveAt(BackupPathRecordsSelectedIndex);
                Manager.BackupFolderList.RemoveAt(BackupPathRecordsSelectedIndex);
                BackupPathRecords.RemoveAt(BackupPathRecordsSelectedIndex); //UI
            }
        }
        public void AddBackupTask()
        {
            if (BackupPathRecords.Count > 0 && SavePathRecords.Count > 0)
            {
                BackupTasks.Add(new BackupTaskData(BackupPathRecords[0], SavePathRecords[0]));
            }
        }
        public void DelFromBackupTask()
        {
            if (BackupTaskSelectedIndex != -1) BackupTasks.RemoveAt(BackupTaskSelectedIndex);
        }
    }
}
