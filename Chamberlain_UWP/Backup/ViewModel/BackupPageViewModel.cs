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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chamberlain_UWP.Backup
{
    internal static class BackupPageData
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

    public class BackupTaskSequenceItem //备份序列项
    {
        public BackupTaskData ItemBackupTaskData { get; set; }
        public bool IsFullBackup { get; set; }
        public string BackupPath => ItemBackupTaskData.BackupPath;
        public string SavePath => ItemBackupTaskData.SavePath;
        public BackupTaskSequenceItem(BackupTaskData itemBackupTaskData, bool isTotalBackup)
        {
            ItemBackupTaskData = itemBackupTaskData;
            IsFullBackup = isTotalBackup;
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
        int _backupPathRecordsSelectedIndex = -1;
        int _savePathRecordsSelectedIndex = -1;
        internal BackupManager Manager = new BackupManager();
        bool _isBackupCardVisible = false;
        int _backupVersionRecordListSelectedIndex = -1;
        bool _isRecordListOnLoading = false;
        ObservableCollection<BackupTaskSequenceItem> _backupTaskSequence = new ObservableCollection<BackupTaskSequenceItem>(); //备份序列

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

        public int BackupPathRecordsSelectedIndex //备份路径页选中的Index
        {
            get => _backupPathRecordsSelectedIndex;
            set
            {
                _backupPathRecordsSelectedIndex = value;
                OnPropertyChanged(nameof(BackupPathRecordsSelectedIndex));
                OnPropertyChanged(nameof(IsBackupDeleteButtonEnabled));
            }
        }

        public bool IsBackupDeleteButtonEnabled //备份路径页删除按钮是否启用
        {
            get => BackupPathRecordsSelectedIndex != -1 && BackupPathRecords.Count > 0;
        }

        public int SavePathRecordsSelectedIndex //保存路径页选中的Index
        {
            get => _savePathRecordsSelectedIndex;
            set
            {
                _savePathRecordsSelectedIndex = value;
                OnPropertyChanged(nameof(SavePathRecordsSelectedIndex));
                OnPropertyChanged(nameof(IsSaveDeleteButtonEnabled));
            }
        }

        public bool IsSaveDeleteButtonEnabled //保存路径页的删除按钮是否启用
        {
            get => SavePathRecordsSelectedIndex != -1 && SavePathRecords.Count > 0;
        }

        public int BackupRecordComboBoxSelectedIndex //备份记录页的ComboBox选择项
        {
            get => _backupRecordComboBoxSelectedIndex;
            set
            {
                _backupRecordComboBoxSelectedIndex = value;
                OnPropertyChanged(nameof(BackupRecordComboBoxSelectedIndex));
                Manager.GetBackupVersionList(BackupRecordComboBoxSelectedPath); //获取选中路径的备份列表
                OnPropertyChanged(nameof(BackupVersionRecords)); //备份版本记录的内容也要随之改变
                OnPropertyChanged(nameof(IsRecoveryButtonEnabled)); //启用按钮
                OnPropertyChanged(nameof(IsRecoveryDeleteButtionEnabled)); //启用删除按钮
            }
        }
        public string BackupRecordComboBoxSelectedPath //选中项的值
        {
            get => BackupRecordComboBoxSelectedIndex == -1 ? null : BackupPathRecords[BackupRecordComboBoxSelectedIndex].Path;
        }
        public string BackupRecordPathText //备份路径页ComboBox选择项对应的路径
        {
            get
            {
                if (BackupRecordComboBoxSelectedIndex == -1)
                    return Strings.Resources.Unselected; //暂未选中
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
                OnPropertyChanged(nameof(SelectedTask));
                OnPropertyChanged(nameof(IsQuickBackupAllowed));
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

        //备份记录DataGrid选中index
        public int BackupVersionRecordListSelectedIndex
        {
            get => _backupVersionRecordListSelectedIndex;
            set
            {
                _backupVersionRecordListSelectedIndex = value;
                OnPropertyChanged(nameof(BackupVersionRecordListSelectedIndex));
                OnPropertyChanged(nameof(IsRecoveryDeleteButtionEnabled));
            }
        }

        public bool IsRecordListOnLoading //记录页是否正在加载
        {
            get => _isRecordListOnLoading;
            set
            {
                _isRecordListOnLoading = value;
                OnPropertyChanged(nameof(IsRecordListOnLoading));
                OnPropertyChanged(nameof(IsRecordListProgressVisible)); //修改可见性
            }
        }

        public Visibility IsRecordListProgressVisible
        {
            get => IsRecordListOnLoading ? Visibility.Visible : Visibility.Collapsed;
        }

        BackupTaskData SelectedTask //选中的备份任务
        {
            get
            {
                if (BackupTaskSelectedIndex != -1)
                    return BackupTasks[BackupTaskSelectedIndex];
                else
                    return null;
            }
        }

        public bool IsQuickBackupAllowed //是否允许快速备份
        {
            get
            {
                if (BackupTaskSelectedIndex == -1) //是否选中
                    return false;
                else
                {
                    if (SelectedTask != null)
                    {
                        bool result = Manager.QueryLastTotalBackupVersion(SelectedTask.BackupPath) != null;
                        return result;
                    }
                    else return false;
                }
            }
        }

        public bool IsRecoveryButtonEnabled //恢复页面的操作按钮是否启用
        {
            get => BackupRecordComboBoxSelectedIndex != -1 && BackupVersionRecords.Count > 0;
        }

        public bool IsRecoveryDeleteButtionEnabled //恢复页面的删除按钮是否启用
        {
            get
            {
                if (IsRecoveryButtonEnabled) //备份页面的Button是否开启
                {
                    List<BackupVersionRecord> records = new List<BackupVersionRecord>(BackupVersionRecords);
                    var lastFullBackupRecords = from BackupVersionRecord record in records
                                                where record.IsFullBackup
                                                orderby record.BackupTime descending
                                                select record;
                    DateTime lastFullBackupTime = lastFullBackupRecords.ToList()[0].BackupTime;
                    var quickBackupRecords = from BackupVersionRecord record in records
                                             where record.BackupTime > lastFullBackupTime
                                             select record;
                    if (BackupVersionRecordListSelectedIndex == -1 ||
                        lastFullBackupTime == BackupVersionRecords[BackupVersionRecordListSelectedIndex].BackupTime &&
                        quickBackupRecords.ToList().Count > 0) //列表未选中，或如果最后一个完整备份前有快速备份
                        return false; //不允许删除
                    else return true;
                }
                else return false;
            }
        }

        public ObservableCollection<BackupTaskSequenceItem> BackupTaskSequence
        {
            get => _backupTaskSequence;
            set
            {
                BackupTaskSequence = value;
                RefreshBackupTaskSequenceData();
            }
        }

        public bool IsBackupTaskSequenceVisible
        {
            get => BackupTaskSequence.Count > 0;
        }

        public int BackupTaskSequenceSelectedIndex { get; set; } = -1;

        public string BackupTaskSequenceDescription //剩余任务显示
        {
            get => Strings.Resources.BackupTaskSequenceDesc(BackupTaskSequence.Count); //正在处理的也算：剩余{BackupTaskSequence.Count}条任务
        }

        /// <summary>
        /// 方法区
        /// </summary>

        public void TotalBackup()
        {
            if (BackupTaskSelectedIndex != -1)
            {
                BackupTaskData selectedTask = BackupTasks[BackupTaskSelectedIndex];
                //ContentDialog selectedTaskDiaglog = new ContentDialog
                //{
                //    Title = "完整备份",
                //    Content = $"选中的任务如下\n\"{selectedTask.BackupPath}\" -> \"{selectedTask.SavePath}\"",
                //    PrimaryButtonText = "开始备份",
                //    CloseButtonText = "取消"
                //};

                //ContentDialogResult result = await selectedTaskDiaglog.ShowAsync();

                //if (result == ContentDialogResult.Primary) //如果确认开始备份
                //{
                //    IsBackupCardVisible = true;
                //    //await Manager.RunBackupAsync(selectedTask.BackupPath, selectedTask.SavePath, true);
                //    //OnPropertyChanged(nameof(IsQuickBackupAllowed)); //允许快速备份
                //}
                BackupTaskSequence.Add(new BackupTaskSequenceItem(selectedTask, true)); //将完整备份任务添加至任务序列
                RefreshBackupTaskSequenceData();
            }
            else
            {
                if (BackupTasks.Count > 0)
                    DisplayDialog(Strings.Resources.NoBackupTaskSelectedTitle, Strings.Resources.NoBackupTaskSelectedDesc); //没有选中备份任务; 请先选中备份任务再进行备份
                else
                    DisplayDialog(Strings.Resources.NoBackupTaskTitle, Strings.Resources.NoBackupTaskDesc); //没有备份任务; 请先添加备份任务
            }
        }

        public void QuickBackup()
        {
            if (BackupTaskSelectedIndex != -1)
            {
                BackupTaskData selectedTask = BackupTasks[BackupTaskSelectedIndex];
                //ContentDialog selectedTaskDiaglog = new ContentDialog
                //{
                //    Title = "快速备份",
                //    Content = $"选中的任务如下\n\"{selectedTask.BackupPath}\" -> \"{selectedTask.SavePath}\"",
                //    PrimaryButtonText = "开始备份",
                //    CloseButtonText = "取消"
                //};

                //ContentDialogResult result = await selectedTaskDiaglog.ShowAsync();

                //if (result == ContentDialogResult.Primary) //如果确认开始备份
                //{
                //    IsBackupCardVisible = true;
                //    await Manager.RunBackupAsync(selectedTask.BackupPath, selectedTask.SavePath, false);
                //}
                BackupTaskSequence.Add(new BackupTaskSequenceItem(selectedTask, false)); //将快速备份任务添加至任务序列
                RefreshBackupTaskSequenceData();
            }
        }

        public async Task StartBackup()
        {
            while (BackupTaskSequence.Count > 0)
            {
                IsBackupCardVisible = true;
                BackupTaskSequenceItem item = BackupTaskSequence[0];
                BackupTaskSequence.RemoveAt(0); //从任务序列中删除即将执行的任务

                RefreshBackupTaskSequenceData();

                await Manager.RunBackupAsync(item.BackupPath, item.SavePath, item.IsFullBackup);
                OnPropertyChanged(nameof(IsQuickBackupAllowed));
            }
        }

        async Task<StorageFolder> OpenFolder()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            return folder;
        }

        public void DelBackupTaskSequenceItem() //删除选中的任务序列项
        {
            if(BackupTaskSelectedIndex != -1)
                BackupTaskSequence.RemoveAt(BackupTaskSequenceSelectedIndex);

            RefreshBackupTaskSequenceData();
        }

        public async void Add2SavePathList() //添加到保存路径
        {
            StorageFolder folder = await OpenFolder();
            if (folder != null)
            {
                List<string> list = (from PathRecord record in Manager.SaveFolderList
                                     select record.Path).ToList();
                if (list.Contains(folder.Path))
                    DisplayDialog(Strings.Resources.Prompt, Strings.Resources.BackupPathExistDesc); //提示; 检测到路径已经存在，路径将不会被添加
                else
                {
                    SavePathRecords.Add(new PathRecord(folder)); //UI
                    BackupPageData._savePathNames.Add(new SavePathString(folder.Path));
                    PathRecord save_path = new PathRecord(folder);
                    Manager.SaveFolderList.Add(save_path);
                    //添加访问权限
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(save_path.Hash, save_path.Folder); //添加访问token
                    DataSettings.GenerateJsonAsync(Manager.SaveFolderList, Manager.AppFolder, Manager.SaveJsonName); //保存目标目录列表
                    OnPropertyChanged(nameof(SavePathRecords));
                }
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
                List<string> list = (from PathRecord record in Manager.BackupFolderList
                                     select record.Path).ToList();
                if (list.Contains(folder.Path))
                    DisplayDialog(Strings.Resources.Prompt, Strings.Resources.BackupPathExistDesc); //提示; 检测到路径已经存在，路径将不会被添加
                else
                {
                    BackupPathRecords.Add(new PathRecord(folder)); //UI
                    BackupPageData._backupPathNames.Add(new BackupPathString(folder.Path));
                    PathRecord backup_path = new PathRecord(folder);
                    Manager.BackupFolderList.Add(backup_path);
                    //添加访问权限
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(backup_path.Hash, backup_path.Folder); //添加访问token
                    DataSettings.GenerateJsonAsync(Manager.BackupFolderList, Manager.AppFolder, Manager.BackupJsonName); //保存备份文件列表
                    OnPropertyChanged(nameof(BackupPathRecords));
                }
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
        public async void SaveErrorMessages()
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.Desktop;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add(Strings.Resources.PlainText, new List<string>() { ".txt" }); //描述：纯文本
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = $"log{DateTime.Now.ToString("MMddHHmm")}";

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                //写入完成后才会通知
                CachedFileManager.DeferUpdates(file);

                StringBuilder sb = new StringBuilder();
                Manager.ErrorMessages.ToList().ForEach(item => sb.AppendLine($"{item}\n"));

                await FileIO.WriteTextAsync(file, sb.ToString());

                Windows.Storage.Provider.FileUpdateStatus status =
                    await CachedFileManager.CompleteUpdatesAsync(file);
                if (status != Windows.Storage.Provider.FileUpdateStatus.Complete)
                    Manager.AddErrorMessage(0, Strings.Resources.ErrorMessageUnableSaveTo(file.Name)); //无法保存到 {file.Name}
            }
        }

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

        public void RefreshBackupRecordData()
        {
            if (BackupRecordComboBoxSelectedIndex != -1) //已有选中项
            {
                Task loadDataFromJson = Task.Run(async () => await Manager.LoadData()); //等待读取数据
                loadDataFromJson.Wait();
                Manager.GetBackupVersionList(BackupRecordComboBoxSelectedPath); //获取相关列表
                OnPropertyChanged(nameof(BackupVersionRecords)); //刷新备份版本记录列表
            }
        }

        public void RecoverFolder() //恢复文件夹
        {
            if (BackupVersionRecordListSelectedIndex != -1)
            {
                IsBackupCardVisible = true;
                BackupVersionRecord backupRecord = BackupVersionRecords[BackupVersionRecordListSelectedIndex]; //获取选中的备份记录
                Manager.RestoreAsync(backupRecord, false);
            }
        }

        public void ExportBackup() //导出备份文件夹到下载目录
        {
            if (BackupVersionRecordListSelectedIndex != -1)
            {
                IsBackupCardVisible = true;
                BackupVersionRecord backupRecord = BackupVersionRecords[BackupVersionRecordListSelectedIndex]; //获取选中的备份记录
                Manager.RestoreAsync(backupRecord, true);
            }
        }

        public async void DelBackup() //删除备份
        {
            if (BackupVersionRecordListSelectedIndex != -1)
            {
                BackupVersionRecord backupRecord = BackupVersionRecords[BackupVersionRecordListSelectedIndex]; //获取选中的备份记录
                IsRecordListOnLoading = true; //正在删除（设进度条状态为正在处理）
                await Manager.DelFromBackupVersionListAsync(backupRecord);
                RefreshBackupRecordData();
                IsRecordListOnLoading = false; //进度条取消处理状态
            }
        }

        private async void DisplayDialog(string title, string content)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = Strings.Resources.Confirm //确定
            };

            await dialog.ShowAsync();
        }

        private void RefreshBackupTaskSequenceData() //刷新备份任务序列的属性
        {
            OnPropertyChanged(nameof(BackupTaskSequence));
            OnPropertyChanged(nameof(IsBackupTaskSequenceVisible));
            OnPropertyChanged(nameof(BackupTaskSequenceDescription));
        }
    }
}