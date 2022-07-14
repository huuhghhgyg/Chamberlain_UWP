using Chamberlain_UWP.Backup.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;

namespace Chamberlain_UWP.Backup
{
    public class BackupPageViewModel : ViewModelBase
    {
        /// <summary>
        /// 内部变量区
        /// </summary>
        string _backupTitle = "第1项，共3项"; //备份卡片标题
        string _backupDesc = "正在备份[具体的文件]"; //备份卡片描述（正在备份的文件）
        double _backupProgress = 20.0; //备份进度（max=100）
        bool _backupTask = false; //是否正在执行备份任务
        ObservableCollection<string> _saveList = new ObservableCollection<string>(); //备份路径列表
        ObservableCollection<string> _backupList = new ObservableCollection<string>(); //备份目标列表
        ObservableCollection<BackupPathRecord> _fullbackupList = new ObservableCollection<BackupPathRecord>(); //备份目标列表(test)
        ObservableCollection<string> _backupNameList = new ObservableCollection<string>(); //备份目标名称列表
        ObservableCollection<BackupTask> _backupTasks = new ObservableCollection<BackupTask>(); //备份任务列表


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
                OnPropertyChanged(nameof(BackupProgressString));
            }
        }
        public string BackupProgressString //备份卡片进度标志
        {
            get => $"{_backupProgress}%";
        }
        public bool IsBackupCardVisible //备份卡片是否可见
        {
            get => _backupTask;
            set
            {
                _backupTask = value;
                OnPropertyChanged(nameof(IsBackupCardVisible));
                OnPropertyChanged(nameof(IsNoTaskTextVisible));
            }
        }
        public bool IsNoTaskTextVisible //无任务提示是否可见
        {
            get => !_backupTask;
            set
            {
                _backupTask = value;
                OnPropertyChanged(nameof(IsBackupCardVisible));
                OnPropertyChanged(nameof(IsNoTaskTextVisible));
            }
        }
        public ObservableCollection<string> SavePathList //保存列表
        {
            get => _saveList;
            set
            {
                _saveList = value;
                OnPropertyChanged(nameof(SavePathList)); //保存列表选中index
            }
        }
        public int SavePathListSelectedIndex { get; set; } = -1;
        public ObservableCollection<string> BackupPathList //备份列表
        {
            get => _backupList;
            set
            {
                _backupList = value;
                OnPropertyChanged(nameof(BackupPathList));
            }
        }
        public int BackupPathListSelectedIndex { get; set; } = -1; //备份列表选中index
        public ObservableCollection<string> BackupNameList //备份名称列表
        {
            get => _backupNameList;
            set
            {
                _backupNameList = value;
                OnPropertyChanged(nameof(BackupNameList));
            }
        }
        public int BackupNameListSelectedIndex { get; set; } = 0; //备份名称列表选中index
        public ObservableCollection<BackupTask> BackupTaskList
        {
            get => _backupTasks;
            set
            {
                _backupTasks = value;
                BackupManager.BackupTaskList.Clear(); //与BackupManager中的项同步
                foreach (BackupTask item in _backupTasks)
                {
                    BackupManager.BackupTaskList.Add(item);
                }
                OnPropertyChanged(nameof(BackupTaskList));
            }
        }
        public ObservableCollection<BackupPathRecord> BackupPathRecordList //完整的记录列表
        {
            get => _fullbackupList;
            set
            {
                _fullbackupList = value;
                OnPropertyChanged(nameof(BackupPathRecordList));
            }
        }

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

        public void Backup_Cancel() //备份取消
        {

        }

        public void SwitchTaskState()
        {
            IsBackupCardVisible = !IsBackupCardVisible;
        }

        async Task<StorageFolder> OpenFolder()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                //GetFolder(folder);
                return folder;
            }
            else return null;
        }

        //async void GetFolder(StorageFolder folder)
        //{
        //    // Application now has read/write access to all contents in the picked folder
        //    // (including other sub-folder contents)
        //    Windows.Storage.AccessCache.StorageApplicationPermissions.
        //    FutureAccessList.AddOrReplace("PickedFolderToken", folder);
        //    resultText.Text = "Picked folder: " + folder.Name + '\n';

        //    IReadOnlyList<StorageFolder> folder_list = await folder.GetFoldersAsync();
        //    //ui
        //    resultText.Text += "\nFolders:\n";
        //    foreach (StorageFolder subfolder in folder_list) resultText.Text += (subfolder.Name + '\n');

        //    IReadOnlyList<StorageFile> file_list = await folder.GetFilesAsync();
        //    //ui
        //    resultText.Text += "\nFiles:\n";
        //    foreach (StorageFile subfile in file_list) resultText.Text += (subfile.Name + '\n');
        //}

        public async void Add2SavePathList()
        {
            StorageFolder folder = await OpenFolder();
            if (folder != null)
            {
                SavePathList.Add(folder.Path);
                BackupManager.GoalFolderList.Add(new SavePathRecord(folder.Path, folder));
            }
        }
        public void DelFromSavePathList()
        {
            if (SavePathListSelectedIndex != -1)
            {
                BackupManager.GoalFolderList.RemoveAt(SavePathListSelectedIndex);
                SavePathList.RemoveAt(SavePathListSelectedIndex);
            }
        }
        public async void Add2BackupPathList()
        {
            StorageFolder folder = await OpenFolder();
            if (folder != null)
            {
                BackupPathList.Add(folder.Path);
                BackupNameList.Add(folder.Name);
                BackupPathRecordList.Add(new BackupPathRecord(folder.Path, folder));
                BackupManager.BackupFolderList.Add(new BackupPathRecord(folder.Path, folder));
            }
        }
        public void DelFromBackupPathList()
        {
            if (BackupPathListSelectedIndex != -1)
            {
                BackupManager.BackupFolderList.RemoveAt(BackupPathListSelectedIndex);
                BackupPathList.RemoveAt(BackupPathListSelectedIndex);
                //根据任务删除
            }
        }
        public void AddBackupTask()
        {
            if (BackupPathList.Count > 0 && SavePathList.Count > 0)
            {
                BackupTaskList.Add(new BackupTask(BackupManager.BackupFolderList[0], BackupManager.GoalFolderList[0]));
            }
        }
    }
}
