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

        ObservableCollection<PathRecord> _backupPathRecords = new ObservableCollection<PathRecord>();
        ObservableCollection<PathRecord> _savePathRecords = new ObservableCollection<PathRecord>();
        ObservableCollection<BackupTask> _backupTasks = new ObservableCollection<BackupTask>();
        int _backupRecordComboBoxSelectedIndex = -1;

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
        public ObservableCollection<PathRecord> BackupPathRecords //ObservableCollection备份列表
        {
            get => _backupPathRecords;
            set
            {
                _backupPathRecords = value;
                OnPropertyChanged(nameof(_backupPathRecords));
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
        public ObservableCollection<BackupTask> BackupTasks //ObservableCollection备份列表
        {
            get => _backupTasks;
            set
            {
                _backupTasks = value;
                OnPropertyChanged(nameof(BackupTasks));
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

        public async void Add2SavePathList()
        {
            StorageFolder folder = await OpenFolder();
            if (folder != null)
            {
                SavePathRecords.Add(new PathRecord(folder));
            }
        }
        public void DelFromSavePathList()
        {
            if (SavePathRecordsSelectedIndex != -1) SavePathRecords.RemoveAt(SavePathRecordsSelectedIndex);
        }
        public async void Add2BackupPathList()
        {
            StorageFolder folder = await OpenFolder();
            if (folder != null)
            {
                BackupPathRecords.Add(new PathRecord(folder));
            }
        }
        public void DelFromBackupPathList()
        {
            if (BackupPathRecordsSelectedIndex != -1) BackupPathRecords.RemoveAt(BackupPathRecordsSelectedIndex);
        }
        public void AddBackupTask()
        {
            //if(BackupPathRecords.Count>0 && SavePathRecords.Count > 0)
            //    BackupTasks.Add(new BackupTask(BackupPathRecords[0],SavePathRecords[0]));
        }
    }
}
