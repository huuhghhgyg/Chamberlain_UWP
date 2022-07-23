using Chamberlain_UWP.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
        Comparing, //正在比对
        ScanningHash, //正在计算Hash
        Backup, //正在备份
        Restore //正在恢复
    }

    internal class BackupManager : INotifyPropertyChanged
    {
        #region 私有变量
        // 变量区
        private string backupFolderPath = ""; //备份文件夹的路径
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

        #endregion

        #region 属性
        public int TotalFileCount //需要处理的文件总数
        {
            get => _totalFileCount;
            set
            {
                _totalFileCount = value;
                OnPropertyChanged(nameof(TotalFileCount));
            }
        }
        public int ProcessedFileCount //已经处理的文件个数
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
                    case BackupStage.Comparing:
                        WorkingFilePath = "处理中";
                        IsScanning = true;
                        return "正在比对信息";
                    case BackupStage.ScanningHash:
                        IsScanning = false;
                        return "正在计算文件Hash";
                    case BackupStage.Backup:
                        IsScanning = false;
                        return "正在备份文件";
                    case BackupStage.Restore:
                        IsScanning = false;
                        return "正在恢复文件";
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

        #endregion

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
            StorageFolder goalFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(goal_token);
            if (rootFolder == null || goalFolder == null)
            {
                if (rootFolder == null) throw new Exception("无法访问源文件夹token");
                else throw new Exception("无法访问目标文件夹token");
            }

            // 获取基本信息
            backupFolderPath = rootFolder.Path; //备份文件夹的路径

            // 获取所有文件
            IReadOnlyList<StorageFile> all_files = await rootFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderBySearchRank);
            TotalFileCount = all_files.Count; //需要备份的文件总数

            //读取最后一个完整备份
            BackupVersionRecord lastTotalBackup = QueryLastTotalBackupVersion(backupFolderPath);

            //计算Hash（步骤显示计算Hash）
            List<FileNode> fileNodeList = await GetFileNodes(all_files);

            //将状态改为正在比对信息
            BackupTaskStage = BackupStage.Comparing;

            //最后一个完整备份的备份信息路径
            string backupInfoPath = $"{lastTotalBackup.SaveFolderPath}\\{lastTotalBackup.BackupFolderName}\\{lastTotalBackup.VersionFolderName}.json"; //备份信息路径
            string saveVersionFolderPath = $"{lastTotalBackup.SaveFolderPath}\\{lastTotalBackup.BackupFolderName}\\{lastTotalBackup.VersionFolderName}"; //版本路径

            //读取备份信息Json
            StorageFile backupInfoJson = await StorageFile.GetFileFromPathAsync(backupInfoPath);
            string contents = await DataSettings.LoadFile(backupInfoJson); //读取数据
            List<BackupInfoList> lastTotalBackupInfo = JsonSerializer.Deserialize<List<BackupInfoList>>(contents); //从文件中读取备份信息
            List<FileNode> lastTotalFileNodeList = lastTotalBackupInfo[0].SaveList; //从备份信息中读取保存文件列表

            //文件比对
            //找到当前列表中不存在于旧列表中的项（新增&更改）list1
            List<FileNode> list1 = fileNodeList.Except(lastTotalFileNodeList).ToList();

            //找到旧列表中不存在于当前列表中的项（删除&更改）list2
            List<FileNode> list2 = lastTotalFileNodeList.Except(fileNodeList).ToList();

            //list2删除与list1中相同的部分，得到删除的列表（仅删除）list2
            List<string> pathList = (from FileNode node in list1
                                     select node.RelativePath).ToList();
            list2 = (from FileNode node in list2
                     where !pathList.Contains(node.RelativePath)
                     select node).ToList();
            //Summary:
            //list1：新增项&更改项
            //list2：删除项
            BackupInfoList backupInfo = new BackupInfoList(list1, list2);

            //文件备份
            await BackupFolder(rootFolder, goalFolder, backupInfo, false);

            BackupTaskStage = BackupStage.Spare; //已完成，将状态恢复为空闲
        }

        //通过备份文件夹的路径查询已有记录中最后一个完整备份
        public BackupVersionRecord QueryLastTotalBackupVersion(string backupPath)
        {
            var queryResults = (from BackupVersionRecord record in BackupVersionRecordList
                                where record.BackupFolderPath == backupPath
                                where record.IsFullBackup == true
                                orderby record.BackupTime descending
                                select record).ToList();
            return queryResults.FirstOrDefault(); //返回第一个结果
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
            backupFolderPath = rootFolder.Path; //备份文件夹的路径

            // 获取所有文件
            IReadOnlyList<StorageFile> all_files = await rootFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderBySearchRank);
            TotalFileCount = all_files.Count; //需要备份的文件总数

            //获取列表条目完整内容，计算Hash
            List<FileNode> fileNodeList = await GetFileNodes(all_files);

            BackupInfoList backupInfo = new BackupInfoList(fileNodeList, null);

            //文件备份
            await BackupFolder(rootFolder, goalFolder, backupInfo, true);

            BackupTaskStage = BackupStage.Spare; //已完成，将状态恢复为空闲
        }

        public async Task<List<FileNode>> GetFileNodes(IReadOnlyList<StorageFile> all_files)
        {
            //添加到操作列表，并计算Hash
            BackupTaskStage = BackupStage.ScanningHash; //将状态改为正在计算Hash
            List<FileNode> fileNodeList = new List<FileNode>();
            foreach (StorageFile file in all_files)
            {
                string relative_path = file.Path.Substring(backupFolderPath.Length);
                try
                {
                    fileNodeList.Add(new FileNode(file, relative_path, await GetMD5HashAsync(file)));
                }
                catch (FileNotFoundException)
                {
                    ErrorMessages.Add($"❌文件不存在（计算Hash时）：{file.Path}");
                    OnPropertyChanged(nameof(ErrorMessages));
                    OnPropertyChanged(nameof(IsAnyError));
                }
                ProcessedFileCount++;
            }
            return fileNodeList;
        }

        public async Task BackupFolder(StorageFolder rootFolder, StorageFolder goalFolder, BackupInfoList list, bool isTotalBackup)
        {
            //备份版本文件夹名称(如：2022-07-17-110502)
            string backup_date = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
            StorageFolder versionFolder = await goalFolder.CreateFolderAsync(rootFolder.Name, CreationCollisionOption.OpenIfExists); //存放各种备份版本的文件夹，文件名为备份文件夹的名称
            StorageFolder goalVersionFolder = await versionFolder.CreateFolderAsync(backup_date); //存至版本文件夹

            List<FileNode> fileNodeList = list.SaveList; //获取新增、更新的文件
            DataSettings.GenerateJsonAsync(new List<BackupInfoList>() { list }, versionFolder, $"{backup_date}.json"); //导出文件信息至备份文件夹

            BackupTaskStage = BackupStage.Backup; //将状态改为正在备份
            TotalFileCount = fileNodeList.Count; //更新备份文件总数
            ProcessedFileCount = 0; //已处理文件数清零

            foreach (FileNode fileNode in fileNodeList)
            {
                if (ShowDetail) WorkingFilePath = fileNode.File.Path; //更新正在工作的文件路径
                else WorkingFilePath = "正在备份";

                //获取相对路径
                StorageFolder relativeFolder = await CreateRelativePath(goalVersionFolder, fileNode.RelativePath);

                try //尝试复制
                {
                    await fileNode.File.CopyAsync(relativeFolder);
                }
                catch (FileNotFoundException)
                {
                    ErrorMessages.Add($"❌文件不存在（复制时）：{fileNode.File.Path}");
                    UpdateErrorMessageList();
                }
                ProcessedFileCount++; //增加一个完成文件
            }

            //添加备份记录
            await AddBackupVersionFileAsync(goalFolder, rootFolder, backup_date, isTotalBackup);
        }

        public void ClearErrorMessages()
        {
            ErrorMessages.Clear();
            OnPropertyChanged(nameof(ErrorMessages));
            OnPropertyChanged(nameof(IsAnyError));
        }
        public void UpdateErrorMessageList()
        {
            OnPropertyChanged(nameof(ErrorMessages));
            OnPropertyChanged(nameof(IsAnyError));
        }

        async Task<StorageFolder> CreateRelativePath(StorageFolder rootFolder, string relativePath) //创建文件夹目录
        {
            List<string> subfolders = relativePath.Split("\\").ToList(); //切割相对路径
            subfolders.RemoveAt(0); //第一个值为空，删掉
            subfolders.RemoveAt(subfolders.Count - 1); //删除最后一个（文件名）

            if (subfolders.Count > 0)
                return await CreateRelativePath(rootFolder, subfolders); //是相对路径，执行函数
            else
                return rootFolder; //不是相对路径
        }

        async Task<StorageFolder> CreateRelativePath(StorageFolder folder, List<string> paths) //创建文件夹目录（递归）
        {
            StorageFolder newFolder = await folder.CreateFolderAsync(paths[0], CreationCollisionOption.OpenIfExists); //如果已经存在，则打开
            paths.RemoveAt(0);
            if (paths.Count > 0) //不是最终路径
            {
                return await CreateRelativePath(newFolder, paths);
            }
            else return newFolder; //最终路径
        }

        public void RunBackup(string backup_path, string save_path, bool isTotalBackup)
        {
            BackupTaskStage = BackupStage.Preparing; //准备阶段
            //清除记录
            TotalFileCount = 0;
            ProcessedFileCount = 0;

            PathRecord backup_record, save_record;
            QueryBackupTask(backup_path, save_path, out backup_record, out save_record); //查询路径记录

            if (isTotalBackup)
                TotalBackupFolder(backup_record.Hash, save_record.Hash);
            else
                QuickBackupFolder(backup_record.Hash, save_record.Hash);
        }

        #region 文件保存配置
        // 文件保存
        public readonly string BackupJsonName = "BackupFolders.json";
        public readonly string SaveJsonName = "SaveFolders.json";
        public readonly string BackupTaskJsonName = "BackupTasks.json";
        public readonly string BackupVersionJsonName = "BackupVersion.json";
        internal readonly StorageFolder AppFolder = ApplicationData.Current.LocalFolder;

        #endregion

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

            BackupVersionRecordList.Clear();
            foreach (PathRecord record in SaveFolderList) //读取备份版本列表（可能散落在各个文件夹）
            {
                if (await record.Folder.TryGetItemAsync(BackupVersionJsonName) != null) //文件存在
                {
                    StorageFile file = await record.Folder.GetFileAsync(BackupVersionJsonName);
                    List<BackupVersionRecord> list = JsonSerializer.Deserialize<List<BackupVersionRecord>>(await DataSettings.LoadFile(file));
                    BackupVersionRecordList.AddRange(list);
                }
            }
        }

        //Query备份路径对应的备份记录列表到备份页的ObservableCollection
        public void GetBackupVersionList(string backupPath)
        {
            var queryResults = (from BackupVersionRecord record in BackupVersionRecordList
                                where record.BackupFolderPath == backupPath
                                select record).ToList();
            BackupPageData._backupVersionRecords = new ObservableCollection<BackupVersionRecord>(queryResults);
        }

        //保存备份文件版本信息到对应文件夹中的Json
        public async Task AddBackupVersionFileAsync(StorageFolder saveFolder, StorageFolder backupFolder, string versionFolderName, bool isFullBackup) //保存文件夹，备份路径
        {
            List<BackupVersionRecord> list = await LoadBackupVersionFile(saveFolder); //读取列表
            BackupVersionRecord record = new BackupVersionRecord(isFullBackup, DateTime.Now, backupFolder.Path, backupFolder.Name, saveFolder.Path, versionFolderName); //创建新条目
            BackupVersionRecordList.Add(record); //添加到全局列表中
            list.Add(record); //往列表添加新条目

            DataSettings.GenerateJsonAsync(list, saveFolder, BackupVersionJsonName); //导出json
        }

        public async Task<List<BackupVersionRecord>> LoadBackupVersionFile(StorageFolder saveFolder) //从保存目录中读取备份信息
        {
            //读取文件
            List<BackupVersionRecord> list = new List<BackupVersionRecord>(); //预先创建列表
            if (await saveFolder.TryGetItemAsync(BackupVersionJsonName) != null) //判断文件是否存在
            {
                //文件存在，读取内容到列表
                string contents = await DataSettings.LoadFile(await saveFolder.GetFileAsync(BackupVersionJsonName)); //读取数据
                list = JsonSerializer.Deserialize<List<BackupVersionRecord>>(contents); //从文件中读取列表
            }
            return list;
        }

        public async Task DelFromBackupVersionListAsync(BackupVersionRecord backupRecord) //根据备份记录删除
        {
            string backupInfoPath = $"{backupRecord.SaveFolderPath}\\{backupRecord.BackupFolderName}\\{backupRecord.VersionFolderName}.json"; //备份信息路径
            string saveVersionFolderPath = $"{backupRecord.SaveFolderPath}\\{backupRecord.BackupFolderName}\\{backupRecord.VersionFolderName}"; //版本路径

            StorageFolder saveFolder = await StorageFolder.GetFolderFromPathAsync(backupRecord.SaveFolderPath); //读取备份信息Json
            List<BackupVersionRecord> list = await LoadBackupVersionFile(saveFolder); //读取列表
            var delList = from BackupVersionRecord record in list
                          where record.VersionFolderName != backupRecord.VersionFolderName
                          select record; //选中不同的项
            list = new List<BackupVersionRecord>(delList.ToList()); //更新列表（删除对应记录）
            if (list.Count > 0)
                DataSettings.GenerateJsonAsync(list, saveFolder, BackupVersionJsonName); //保存到目录
            else
            {
                StorageFile saveFolderJsonFile = await saveFolder.GetFileAsync(BackupVersionJsonName);
                await saveFolderJsonFile.DeleteAsync();
            }

            try
            {
                StorageFolder saveVersionFolder = await StorageFolder.GetFolderFromPathAsync(saveVersionFolderPath);
                await saveVersionFolder.DeleteAsync(); //删除版本文件夹
            }
            catch (Exception ex)
            {
                ErrorMessages.Add($"⚠{ex.Message}");
            }

            try
            {
                StorageFile backupInfoJson = await StorageFile.GetFileFromPathAsync(backupInfoPath);
                await backupInfoJson.DeleteAsync(); //删除记录文件
            }
            catch (Exception ex)
            {
                ErrorMessages.Add($"⚠{ex.Message}");
            }
        }

        async Task<BackupInfoList> LoadBackupInfoListAsync(BackupVersionRecord backupRecord) //加载备份信息（保存列表，删除列表）
        {
            string backupInfoPath = $"{backupRecord.SaveFolderPath}\\{backupRecord.BackupFolderName}\\{backupRecord.VersionFolderName}.json"; //备份信息路径
            //string saveVersionFolderPath = $"{backupRecord.SaveFolderPath}\\{backupRecord.BackupFolderName}\\{backupRecord.VersionFolderName}"; //版本路径

            //读取备份信息Json
            StorageFile backupInfoJson = await StorageFile.GetFileFromPathAsync(backupInfoPath);
            string contents = await DataSettings.LoadFile(backupInfoJson); //读取数据
            List<BackupInfoList> backupInfoList = JsonSerializer.Deserialize<List<BackupInfoList>>(contents); //从文件中读取列表
            return backupInfoList[0];
        }

        //读取所有FileNode中的StorageFile
        public void GetFileNodeListHandle(BackupVersionRecord backupRecord, List<FileNode> fileNodeList)
        {
            string saveVersionFolderPath = $"{backupRecord.SaveFolderPath}\\{backupRecord.BackupFolderName}\\{backupRecord.VersionFolderName}"; //版本路径
            fileNodeList.ForEach(async item => item.File = await StorageFile.GetFileFromPathAsync($"{saveVersionFolderPath}{item.RelativePath}"));
        }

        public async void RestoreAsync(BackupVersionRecord backupRecord, bool isExport)
        {
            BackupTaskStage = BackupStage.Preparing; //将状态改为准备中

            BackupInfoList backupInfo = await LoadBackupInfoListAsync(backupRecord); //读取备份信息

            List<FileNode> fileNodeList = backupInfo.SaveList; //获取保存信息

            //版本文件夹路径
            string saveVersionFolderPath = $"{backupRecord.SaveFolderPath}\\{backupRecord.BackupFolderName}\\{backupRecord.VersionFolderName}";
            //读取对应版本的文件夹
            GetFileNodeListHandle(backupRecord, fileNodeList);

            StorageFolder rootFolder; //声明rootFolder
            if (isExport) //判断是否要求导出到库中的下载文件夹
            {
                int folderNameIndex = backupRecord.BackupFolderPath.LastIndexOf('\\'); //路径指向的文件名index
                string folderName = backupRecord.BackupFolderPath.Substring(folderNameIndex + 1);
                //在下载文件夹中创建备份的文件夹，如果发生冲突则自动修改文件夹名称
                rootFolder = await DownloadsFolder.CreateFolderAsync(folderName, CreationCollisionOption.GenerateUniqueName);
            }
            else
                rootFolder = await StorageFolder.GetFolderFromPathAsync(backupRecord.BackupFolderPath);

            if (!backupRecord.IsFullBackup) //判断是否完整备份
            {
                List<string> deletedFileNodeString =
                    new List<string>(from FileNode node in backupInfo.DeleteList select node.RelativePath); //获取当前版本标记的删除项

                GetFileNodeListHandle(backupRecord, fileNodeList);

                //如果非完整备份，要计算需要恢复的fileNodeList
                BackupVersionRecord lastTotalBackup = QueryLastTotalBackupVersion(backupRecord.BackupFolderPath);
                BackupInfoList lastBackupInfo = await LoadBackupInfoListAsync(lastTotalBackup); //读取备份信息
                List<FileNode> lastFileNodeList = lastBackupInfo.SaveList; //获取保存信息

                //获取快速备份的文件路径列表
                List<string> fileNodeRelativePath = (from FileNode node in fileNodeList
                                                     select node.RelativePath).ToList();
                lastFileNodeList = (from FileNode node in lastFileNodeList
                                    where !fileNodeRelativePath.Contains(node.RelativePath) //删除旧版本的文件（hash不同，无法使用Except）
                                       && !deletedFileNodeString.Contains(node.RelativePath) //排除标记已删除的文件
                                    select node).ToList();

                //读取对应版本的文件夹
                GetFileNodeListHandle(lastTotalBackup, lastFileNodeList);

                //添加没有变更的文件
                fileNodeList.AddRange(lastFileNodeList);
            }

            BackupTaskStage = BackupStage.Restore; //将状态改为恢复中
            //更新信息
            TotalFileCount = fileNodeList.Count; //需要处理的文件总数
            ProcessedFileCount = 0; //清零

            foreach (FileNode file in fileNodeList)
            {
                StorageFolder relativeFolder = await CreateRelativePath(rootFolder, file.RelativePath);
                await file.File.CopyAsync(relativeFolder, file.File.Name, NameCollisionOption.ReplaceExisting);
                ProcessedFileCount++;
            }

            //更新信息
            BackupTaskStage = BackupStage.Spare;
        }
    }
}
