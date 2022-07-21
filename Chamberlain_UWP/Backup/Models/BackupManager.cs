using Chamberlain_UWP.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;

namespace Chamberlain_UWP.Backup.Models
{
    public enum BackupStage //备份阶段枚举值
    {
        Spare, //空闲
        Preparing, //正在准备
        ScanningHash, //正在计算Hash
        Backup //正在备份
    }

    internal class BackupManager : INotifyPropertyChanged
    {
        // 变量区
        private string backup_folder_path = ""; //备份文件夹的路径
        public List<PathRecord> BackupFolderList = new List<PathRecord>(); //备份文件夹路径列表
        public List<PathRecord> SaveFolderList = new List<PathRecord>(); //目标文件夹路径列表
        public List<BackupTaskData> BackupTaskList = new List<BackupTaskData>(); //备份任务描述列表
        int _totalFileCount = 0; //需要备份的文件总数
        int _processedFileCount = 0; //已备份文件总数
        string _workingFilePath = ""; //正在备份的文件
        BackupStage _stage = BackupStage.Spare;
        ObservableCollection<string> _errorMessages = new ObservableCollection<string>(); //存放错误信息
        bool _isScanning = false;
        bool _showDetail = true;
        public List<BackupVersionRecord> BackupVersionRecordList = new List<BackupVersionRecord>(); //存放历史备份

        public int TotalFileCount
        {
            get => _totalFileCount;
            set
            {
                _totalFileCount = value;
                OnPropertyChanged(nameof(TotalFileCount));
            }
        }
        public int ProcessedFileCount
        {
            get => _processedFileCount;
            set
            {
                _processedFileCount = value;
                OnPropertyChanged(nameof(ProcessedFileCount));
                OnPropertyChanged(nameof(BackupProgressString));
            }
        }
        public string WorkingFilePath
        {
            get => _workingFilePath;
            set
            {
                _workingFilePath = value;
                OnPropertyChanged(nameof(WorkingFilePath));
            }
        }
        public string BackupProgressString //备份卡片进度标志
        {
            get
            {
                double _backupProgress = 0;
                if (TotalFileCount != 0) _backupProgress = 100.0 * ProcessedFileCount / TotalFileCount;
                return $"{_backupProgress.ToString("0.0")}%";
            }
        }
        public BackupStage BackupTaskStage
        {
            get => _stage;
            set
            {
                _stage = value;
                OnPropertyChanged(nameof(BackupTaskStage));
                OnPropertyChanged(nameof(BackupTaskStageString));
                OnPropertyChanged(nameof(IsWorking));
            }
        }
        public string BackupTaskStageString
        {
            get
            {
                switch (_stage)
                {
                    case BackupStage.Spare:
                        WorkingFilePath = "已完成";
                        IsScanning = false;
                        return "当前无任务";
                    case BackupStage.Preparing: //BackupStage.Preparing
                        WorkingFilePath = "处理中";
                        IsScanning = true;
                        return "正在扫描路径";
                    case BackupStage.ScanningHash:
                        IsScanning = false;
                        return "正在计算文件Hash";
                    case BackupStage.Backup:
                        IsScanning = false;
                        return "正在备份文件";
                    default:
                        IsScanning = true;
                        return "状态未知";
                }
            }
        }
        public bool IsWorking
        {
            get => _stage > 0; //如果不是BackupStage.Spare，就正在工作
        }
        public ObservableCollection<string> ErrorMessages
        {
            get => _errorMessages;
            set
            {
                _errorMessages = value;
                OnPropertyChanged(nameof(ErrorMessages));
                OnPropertyChanged(nameof(IsAnyError));
            }
        }
        public bool IsAnyError { get => ErrorMessages.Count > 0; } //如果存在条目则存在错误
        public bool IsScanning
        {
            get => _isScanning;
            set
            {
                _isScanning = value;
                OnPropertyChanged(nameof(IsScanning));
            }
        }
        public bool ShowDetail
        {
            get => _showDetail;
            set
            {
                _showDetail = value;
                OnPropertyChanged(nameof(ShowDetail));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// 函数区
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public static async Task<string> GetMD5HashAsync(StorageFile file) //可用
        {
            string algorithmName = HashAlgorithmNames.Md5;
            IBuffer buffer = await FileIO.ReadBufferAsync(file);

            HashAlgorithmProvider algorithmProvider = HashAlgorithmProvider.OpenAlgorithm(algorithmName);

            IBuffer buffHash = algorithmProvider.HashData(buffer);

            if (buffHash.Length != algorithmProvider.HashLength)
            {
                throw new Exception("创建hash时出现问题");
            }

            string hex = CryptographicBuffer.EncodeToHexString(buffHash);

            return hex;
        }

        public void QueryBackupTask(string backup_path, string save_path, out PathRecord backup_record, out PathRecord save_record)
        {
            //查询得到数据
            var backup_query = from PathRecord record in BackupFolderList
                               where record.Path == backup_path
                               select record;
            backup_record = backup_query.FirstOrDefault(); //备份路径记录
            var save_query = from PathRecord record in SaveFolderList
                             where record.Path == save_path
                             select record;
            save_record = save_query.FirstOrDefault(); //保存路径记录

            if (save_record == null || backup_record == null)
                throw new Exception("路径不存在"); //需要返回重新选择
        }

        public async void QuickBackupFolder(string folder_token, string goal_token) // 快速备份
        {
            StorageFolder rootFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folder_token);
            if (rootFolder == null)
            {
                throw new Exception("无法访问token");
            }
            // 获取所有文件
            IReadOnlyList<StorageFile> all_files = await rootFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderBySearchRank);

            /* 1.计算所有的hash并保存到列表，方便与备份的目录进行比对
             * 2.快速备份时也创建列表，与旧列表中比对：
             *   a.文件存在于旧列表，hash相同 => 跳过
             *   b.文件存在于旧列表，hash不同 => 备份
             *   c.文件不存在于旧列表（新增） => 备份
             * 3.反向比对（旧列表与新列表比对）:
             *   a. 如果文件不存在于新列表 => 添加删除标记
             * 4.输出结果到json
             */
        }

        public async void TotalBackupFolder(string folder_token, string goal_token) // 完整备份
        {
            StorageFolder rootFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folder_token);
            StorageFolder goalFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(goal_token);
            if (rootFolder == null || goalFolder == null)
            {
                if (rootFolder == null) throw new Exception("无法访问源文件夹token");
                else throw new Exception("无法访问目标文件夹token");
            }

            // 获取基本信息
            backup_folder_path = rootFolder.Path; //备份文件夹的路径

            // 获取所有文件
            IReadOnlyList<StorageFile> all_files = await rootFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderBySearchRank);
            TotalFileCount = all_files.Count; //需要备份的文件总数

            //添加到操作列表，并计算Hash
            BackupTaskStage = BackupStage.ScanningHash; //将状态改为正在计算Hash
            List<FileNode> file_node_list = new List<FileNode>();
            foreach (StorageFile file in all_files)
            {
                string relative_path = file.Path.Substring(backup_folder_path.Length);
                try
                {
                    file_node_list.Add(new FileNode(file, relative_path, await GetMD5HashAsync(file)));
                }
                catch (System.IO.FileNotFoundException)
                {
                    ErrorMessages.Add($"❌文件不存在（计算Hash时）：{file.Path}");
                    OnPropertyChanged(nameof(ErrorMessages));
                    OnPropertyChanged(nameof(IsAnyError));
                }
                ProcessedFileCount++;
            }
            ;//for debug

            //在保存目录创建文件夹(如：2022-07-17-1105)
            BackupTaskStage = BackupStage.Backup; //将状态改为正在备份
            ProcessedFileCount = 0; //已处理文件数清零

            string backup_date = DateTime.Now.ToString("yyyy-MM-dd-HHmm");
            StorageFolder goalVersionFolder = await goalFolder.CreateFolderAsync(rootFolder.Name, CreationCollisionOption.OpenIfExists); //变换目标文件为保存目录下该文件夹的名称
            StorageFolder versionFolder = await goalVersionFolder.CreateFolderAsync(backup_date); //存至版本文件夹

            foreach (FileNode fileNode in file_node_list)
            {
                if (ShowDetail) WorkingFilePath = fileNode.File.Path; //更新正在工作的文件路径
                else WorkingFilePath = "正在备份";

                List<string> subfolders = fileNode.RelativeFolder.Split("\\").ToList(); //切割相对路径
                subfolders.RemoveAt(0); //第一个值为空，删掉

                try
                {
                    if (subfolders.Count != 0) //是相对路径
                    {
                        StorageFolder relative_folder = await CreateRelativePath(versionFolder, subfolders);
                        await fileNode.File.CopyAsync(relative_folder);
                    }
                    else //在根目录
                    {
                        await fileNode.File.CopyAsync(versionFolder);
                    }
                }
                catch (System.IO.FileNotFoundException)
                {
                    ErrorMessages.Add($"❌文件不存在（复制时）：{fileNode.File.Path}");
                    OnPropertyChanged(nameof(ErrorMessages));
                    OnPropertyChanged(nameof(IsAnyError));
                }
                ProcessedFileCount++; //增加一个完成文件
            }

            //添加备份记录
            await SaveBackupVersionFileAsync(goalFolder, rootFolder.Path, backup_date, true);

            BackupTaskStage = BackupStage.Spare; //已完成，将状态恢复为空闲
        }

        public void ClearErrorMessages()
        {
            ErrorMessages.Clear();
            OnPropertyChanged(nameof(ErrorMessages));
            OnPropertyChanged(nameof(IsAnyError));
        }

        async Task<StorageFolder> CreateRelativePath(StorageFolder folder, List<string> paths) //创建文件夹目录
        {
            StorageFolder newFolder = await folder.CreateFolderAsync(paths[0], CreationCollisionOption.OpenIfExists); //如果已经存在，则打开
            paths.RemoveAt(0);
            if (paths.Count > 0)
            {
                return await CreateRelativePath(newFolder, paths);
            }
            else return newFolder;
        }

        public void RunTotalBackup(string backup_path, string save_path)
        {
            BackupTaskStage = BackupStage.Preparing; //准备阶段
            //清除记录
            TotalFileCount = 0;
            ProcessedFileCount = 0;

            PathRecord backup_record, save_record;
            QueryBackupTask(backup_path, save_path, out backup_record, out save_record); //查询路径记录

            TotalBackupFolder(backup_record.Hash, save_record.Hash);
        }


        // 文件保存
        public readonly string BackupJsonName = "BackupFolders.json";
        public readonly string SaveJsonName = "SaveFolders.json";
        public readonly string BackupTaskJsonName = "BackupTasks.json";
        public readonly string BackupVersionJsonName = "BackupVersion.json";
        internal readonly StorageFolder AppFolder = ApplicationData.Current.LocalFolder;
        public async Task LoadData() //读取数据
        {
            //确认文件是否存在
            bool isExistBackupJson = await AppFolder.TryGetItemAsync(BackupJsonName) != null; //备份目录列表Json
            bool isExistSaveJson = await AppFolder.TryGetItemAsync(SaveJsonName) != null; //保存目录列表Json
            bool isExistBackupTaskJson = await AppFolder.TryGetItemAsync(BackupTaskJsonName) != null; //备份任务列表Json

            if (isExistBackupJson) //备份目录
            {
                StorageFile backupFile = await AppFolder.GetFileAsync(BackupJsonName); //获取文件
                BackupFolderList = JsonSerializer.Deserialize<List<PathRecord>>(await DataSettings.LoadFile(backupFile)); //从文件中读取列表
                BackupFolderList.ForEach(async item => item.Folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(item.Hash)); //获取folder
                //恢复备份文件夹列表
                BackupPageData._backupPathRecords = new ObservableCollection<PathRecord>(BackupFolderList);
                List<BackupPathString> BackupFolderNames = (from PathRecord record in BackupFolderList
                                                            select new BackupPathString(record.Path)).ToList();
                BackupPageData._backupPathNames = new ObservableCollection<BackupPathString>(BackupFolderNames);
            }

            if (isExistSaveJson) //保存目录
            {
                StorageFile saveFile = await AppFolder.GetFileAsync(SaveJsonName); //获取文件
                SaveFolderList = JsonSerializer.Deserialize<List<PathRecord>>(await DataSettings.LoadFile(saveFile)); //从文件中读取列表
                SaveFolderList.ForEach(async item => item.Folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(item.Hash)); //获取folder
                //恢复目标文件夹列表
                BackupPageData._savePathRecords = new ObservableCollection<PathRecord>(SaveFolderList);
                List<SavePathString> SaveFolderNames = (from PathRecord record in SaveFolderList
                                                        select new SavePathString(record.Path)).ToList();
                BackupPageData._savePathNames = new ObservableCollection<SavePathString>(SaveFolderNames);
            }

            if (isExistBackupTaskJson) //备份任务
            {
                StorageFile backupTaskFile = await AppFolder.GetFileAsync(BackupTaskJsonName);
                BackupTaskList = JsonSerializer.Deserialize<List<BackupTaskData>>(await DataSettings.LoadFile(backupTaskFile));
                //恢复任务列表
                BackupPageData._backupTasks = new ObservableCollection<BackupTaskData>(BackupTaskList);
            }

            foreach (PathRecord record in SaveFolderList) //读取备份版本列表
            {
                if (await record.Folder.TryGetItemAsync(BackupVersionJsonName) != null) //文件存在
                {
                    StorageFile file = await record.Folder.GetFileAsync(BackupVersionJsonName);
                    List<BackupVersionRecord> list = JsonSerializer.Deserialize<List<BackupVersionRecord>>(await DataSettings.LoadFile(file));
                    BackupVersionRecordList.AddRange(list);
                }
            }
        }

        //Query对应数据
        public void GetBackupVersionList(string backupPath)
        {
            var queryResults = (from BackupVersionRecord record in BackupVersionRecordList
                                where record.BackupFolderPath == backupPath
                                select record).ToList();
            BackupPageData._backupVersionRecords = new ObservableCollection<BackupVersionRecord>(queryResults);
        }

        //保存备份文件版本到对应文件夹
        public async Task SaveBackupVersionFileAsync(StorageFolder saveFolder, string backupPath, string versionFolderName, bool isFullBackup) //保存文件夹，备份路径
        {
            List<BackupVersionRecord> list = new List<BackupVersionRecord>(); //预先创建列表
            if (await saveFolder.TryGetItemAsync(BackupVersionJsonName) != null) //判断文件是否存在
            {
                //文件存在，读取内容到列表
                string contents = await DataSettings.LoadFile(await saveFolder.GetFileAsync(BackupVersionJsonName)); //读取数据
                list = JsonSerializer.Deserialize<List<BackupVersionRecord>>(contents); //从文件中读取列表
            }
            BackupVersionRecord record = new BackupVersionRecord(isFullBackup, DateTime.Now, backupPath, saveFolder.Path, versionFolderName); //创先新条目
            BackupVersionRecordList.Add(record); //添加到全局列表中
            list.Add(record); //往列表添加新条目

            DataSettings.GenerateJsonAsync(list, saveFolder, BackupVersionJsonName); //导出json
        }
    }
}
