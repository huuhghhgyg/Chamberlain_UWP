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
        public List<PathRecord> BackupFolderList = new List<PathRecord>(); //备份文件夹路径列表
        public List<PathRecord> SaveFolderList = new List<PathRecord>(); //目标文件夹路径列表
        public List<BackupTaskData> BackupTaskList = new List<BackupTaskData>(); //备份任务描述列表
        string backupFolderPath = ""; //备份文件夹的路径
        int _totalFileCount = 0; //需要备份的文件总数
        int _processedFileCount = 0; //已备份文件总数
        string _workingFilePath = ""; //正在备份的文件
        BackupStage _stage = BackupStage.Spare;
        ObservableCollection<string> _errorMessages = new ObservableCollection<string>(); //存放错误信息
        bool _isScanning = false;
        bool _showDetail = true;
        public List<BackupVersionRecord> BackupVersionRecordList = new List<BackupVersionRecord>(); //存放历史备份
        string _backupTaskStageString = "当前无任务";

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

                //更新备份内容
                switch (_stage)
                {
                    case BackupStage.Spare:
                        WorkingFilePath = "已完成";
                        IsScanning = false;
                        BackupTaskStageString = "当前无任务";
                        break;
                    case BackupStage.Preparing: //BackupStage.Preparing
                        WorkingFilePath = "处理中";
                        IsScanning = true;
                        BackupTaskStageString = "正在扫描路径";
                        break;
                    case BackupStage.Comparing:
                        WorkingFilePath = "处理中";
                        IsScanning = true;
                        BackupTaskStageString = "正在比对信息";
                        break;
                    case BackupStage.ScanningHash:
                        IsScanning = false;
                        BackupTaskStageString = "正在计算文件Hash";
                        break;
                    case BackupStage.Backup:
                        IsScanning = false;
                        BackupTaskStageString = "正在备份文件";
                        break;
                    case BackupStage.Restore:
                        IsScanning = false;
                        BackupTaskStageString = "正在恢复文件";
                        break;
                    default:
                        IsScanning = true;
                        BackupTaskStageString = "状态未知";
                        break;
                }

                //更新属性
                OnPropertyChanged(nameof(BackupTaskStage));
                OnPropertyChanged(nameof(BackupTaskStageString));
                OnPropertyChanged(nameof(IsWorking));
            }
        }
        public string BackupTaskStageString
        {
            get => _backupTaskStageString;
            set
            {
                _backupTaskStageString = value;
                OnPropertyChanged(nameof(BackupTaskStageString));
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
        public bool IsScanning //备份页卡片进度条状态是否正在扫描
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        /// 函数区

        //获取文件MD5
        public static async Task<string> GetMD5HashAsync(StorageFile file)
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

        //根据备份路径查询备份任务对应的两个PathRecord
        void QueryBackupTask(string backupPath, string savePath, out PathRecord backupRecord, out PathRecord saveRecord)
        {
            //查询得到数据
            var backup_query = from PathRecord record in BackupFolderList
                               where record.Path == backupPath
                               select record;
            backupRecord = backup_query.FirstOrDefault(); //备份路径记录
            var save_query = from PathRecord record in SaveFolderList
                             where record.Path == savePath
                             select record;
            saveRecord = save_query.FirstOrDefault(); //保存路径记录
        }

        //启动备份
        public async Task RunBackupAsync(string backupPath, string savePath, bool isTotalBackup)
        {
            BackupTaskStage = BackupStage.Preparing; //标记为准备阶段

            //清除记录
            TotalFileCount = 0;
            ProcessedFileCount = 0;

            PathRecord backupRecord, saveRecord;
            QueryBackupTask(backupPath, savePath, out backupRecord, out saveRecord); //查询路径记录

            if (saveRecord.Folder == null || backupRecord.Folder == null) //如果记录不存在
            {
                if (saveRecord == null) AddErrorMessage(0, $"无法获取保存文件夹 {savePath}");
                else AddErrorMessage(0, $"无法获取备份文件夹 {backupPath}");
                //恢复显示
                BackupTaskStage = BackupStage.Spare;
                WorkingFilePath = "错误";
            }
            else
            {
                if (isTotalBackup)
                    await TotalBackup(backupRecord.Hash, saveRecord.Hash);
                else
                    await QuickBackup(backupRecord.Hash, saveRecord.Hash);
            }
        }

        //完整备份（流程）
        async Task TotalBackup(string folderToken, string goalToken)
        {
            StorageFolder rootFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
            StorageFolder goalFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(goalToken);
            if (rootFolder == null || goalFolder == null)
            {
                if (rootFolder == null) throw new Exception("无法访问源文件夹token");
                else throw new Exception("无法访问目标文件夹token");
            }

            // 获取基本信息
            backupFolderPath = rootFolder.Path; //备份文件夹的路径

            // 获取所有文件
            IReadOnlyList<StorageFile> allFiles = await rootFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderBySearchRank);
            TotalFileCount = allFiles.Count; //需要备份的文件总数

            //获取列表条目完整内容，计算Hash
            List<FileNode> fileNodeList = await GetFileNodesData(allFiles);

            //创建备份信息对象
            BackupInfoList backupInfo = new BackupInfoList(fileNodeList, null);

            //文件备份
            await BackupFolder(rootFolder, goalFolder, backupInfo, true);

            BackupTaskStage = BackupStage.Spare; //已完成，将状态恢复为空闲
        }

        //将扫描得到的StorageFile列表转化为FileNode列表
        async Task<List<FileNode>> GetFileNodesData(IReadOnlyList<StorageFile> allFiles)
        {
            //计算Hash，并添加到FileNode列表
            BackupTaskStage = BackupStage.ScanningHash; //将状态改为正在计算Hash
            List<FileNode> fileNodeList = new List<FileNode>();
            foreach (StorageFile file in allFiles)
            {
                string relative_path = file.Path.Substring(backupFolderPath.Length);
                try
                {
                    fileNodeList.Add(new FileNode(file, relative_path, await GetMD5HashAsync(file)));
                }
                catch (FileNotFoundException)
                {
                    AddErrorMessage(0, $"计算Hash时文件不存在：{file.Path}");
                }
                ProcessedFileCount++;
            }
            return fileNodeList;
        }

        //对文件夹进行备份操作（复制流程）
        async Task BackupFolder(StorageFolder rootFolder, StorageFolder goalFolder, BackupInfoList list, bool isTotalBackup)
        {
            //备份版本文件夹名称(如：2022-07-17-110502)
            string backupDateString = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
            StorageFolder versionFolder = await goalFolder.CreateFolderAsync(rootFolder.Name, CreationCollisionOption.OpenIfExists); //存放各种备份版本的文件夹，文件名为备份文件夹的名称
            StorageFolder goalVersionFolder = await versionFolder.CreateFolderAsync(backupDateString); //存至版本文件夹

            List<FileNode> fileNodeList = list.SaveList; //获取新增、更新的文件
            DataSettings.GenerateJsonAsync(new List<BackupInfoList>() { list }, versionFolder, $"{backupDateString}.json"); //导出文件信息至备份文件夹

            //更新属性
            BackupTaskStage = BackupStage.Backup; //将状态改为正在备份
            TotalFileCount = fileNodeList.Count; //更新备份文件总数
            ProcessedFileCount = 0; //已处理文件数清零

            foreach (FileNode fileNode in fileNodeList)
            {
                WorkingFilePath = ShowDetail ? fileNode.File.Path : "正在备份"; //更新正在工作的文件路径

                //获取相对路径
                StorageFolder relativeFolder = await CreateRelativePath(goalVersionFolder, fileNode.RelativePath);

                try //尝试复制
                {
                    await fileNode.File.CopyAsync(relativeFolder);
                }
                catch (FileNotFoundException)
                {
                    AddErrorMessage(0, $"复制时文件不存在：{fileNode.File.Path}");
                }
                ProcessedFileCount++; //增加一个完成文件
            }

            //添加备份记录
            await ExportBackupVersionJsonAsync(goalFolder, rootFolder, backupDateString, isTotalBackup);
            if(TotalFileCount == 0)
            {
                AddErrorMessage(2, "没有检测到文件变更");
                TotalFileCount = 1;
                ProcessedFileCount = 1;
            }
        }

        //清除错误信息列表
        public void ClearErrorMessages()
        {
            ErrorMessages.Clear();
            OnPropertyChanged(nameof(ErrorMessages));
            OnPropertyChanged(nameof(IsAnyError));
        }

        //更新错误信息列表
        public void UpdateErrorMessageList()
        {
            OnPropertyChanged(nameof(ErrorMessages));
            OnPropertyChanged(nameof(IsAnyError));
        }

        //提交错误信息
        public void AddErrorMessage(int level, string msg) //0错误；1警告；2提示
        {
            string levelString;
            switch (level)
            {
                case 0:
                    levelString = "错误❌：";
                    break;
                case 1:
                    levelString = "警告⚠：";
                    break;
                case 2:
                    levelString = "提示ℹ：";
                    break;
                default:
                    levelString = "提示ℹ：";
                    break;
            }
            ErrorMessages.Add($"{levelString}{msg}");
            UpdateErrorMessageList();
        }

        //创建文件夹相对路径（主方法）
        async Task<StorageFolder> CreateRelativePath(StorageFolder rootFolder, string relativePath)
        {
            List<string> subfolders = relativePath.Split("\\").ToList(); //切割相对路径
            subfolders.RemoveAt(0); //第一个值为空，删掉
            subfolders.RemoveAt(subfolders.Count - 1); //删除最后一个（文件名）

            if (subfolders.Count > 0)
                return await CreateRelativePath(rootFolder, subfolders); //是相对路径，执行函数
            else
                return rootFolder; //不是相对路径
        }

        //创建文件夹相对路径（递归）
        async Task<StorageFolder> CreateRelativePath(StorageFolder folder, List<string> paths)
        {
            StorageFolder newFolder = await folder.CreateFolderAsync(paths[0], CreationCollisionOption.OpenIfExists); //如果已经存在，则打开
            paths.RemoveAt(0);
            if (paths.Count > 0) //不是最终路径
            {
                return await CreateRelativePath(newFolder, paths);
            }
            else return newFolder; //最终路径
        }

        //读取所有FileNode中的StorageFile
        async Task GetFileNodesHandleAsync(BackupVersionRecord backupRecord, List<FileNode> fileNodeList)
        {
            string saveVersionFolderPath = $"{backupRecord.SaveFolderPath}\\{backupRecord.BackupFolderName}\\{backupRecord.VersionFolderName}"; //版本路径
            foreach (FileNode node in fileNodeList)
            {
                node.File = await StorageFile.GetFileFromPathAsync($"{saveVersionFolderPath}{node.RelativePath}");
            }
        }

        //从备份恢复
        public async void RestoreAsync(BackupVersionRecord backupRecord, bool isExport)
        {
            BackupTaskStage = BackupStage.Preparing; //将状态改为准备中

            //清除记录
            TotalFileCount = 0;
            ProcessedFileCount = 0;

            BackupInfoList backupInfo = await LoadBackupInfoListAsync(backupRecord); //读取备份信息
            List<FileNode> fileNodeList = backupInfo.SaveList; //获取保存信息

            //读取对应版本文件夹中的文件
            WorkingFilePath = "正在读取备份文件";
            await GetFileNodesHandleAsync(backupRecord, fileNodeList);

            StorageFolder rootFolder; //声明rootFolder
            if (isExport) //判断是否要求导出到库中的下载文件夹
            {
                int folderNameIndex = backupRecord.BackupFolderPath.LastIndexOf('\\'); //路径指向的文件名index
                string folderName = backupRecord.BackupFolderPath.Substring(folderNameIndex + 1);
                //在下载文件夹中创建备份的文件夹，如果发生冲突则自动修改文件夹名称
                rootFolder = await DownloadsFolder.CreateFolderAsync(folderName, CreationCollisionOption.GenerateUniqueName);
            }
            else
            {
                rootFolder = await StorageFolder.GetFolderFromPathAsync(backupRecord.BackupFolderPath);

                //删除文件夹下所有文件
                IReadOnlyList<StorageFolder> folderList = await rootFolder.GetFoldersAsync();
                foreach (StorageFolder subfolder in folderList) await subfolder.DeleteAsync();
            }

            if (!backupRecord.IsFullBackup) //判断是否完整备份
            {
                WorkingFilePath = "正在与完整备份进行比对"; //更新信息

                List<string> deletedFileNodeString =
                    new List<string>(from FileNode node in backupInfo.DeleteList select node.RelativePath); //获取当前版本标记的删除项

                await GetFileNodesHandleAsync(backupRecord, fileNodeList);

                //如果非完整备份，要计算需要恢复的fileNodeList
                BackupVersionRecord lastTotalBackup = QueryLastTotalBackupVersion(backupRecord.BackupFolderPath, backupRecord.BackupTime);
                BackupInfoList lastBackupInfo = await LoadBackupInfoListAsync(lastTotalBackup); //读取备份信息
                List<FileNode> lastFileNodes = lastBackupInfo.SaveList; //获取保存信息

                //获取快速备份的文件路径列表
                List<string> fileNodeRelativePath = (from FileNode node in fileNodeList
                                                     select node.RelativePath).ToList();
                lastFileNodes = (from FileNode node in lastFileNodes
                                 where !fileNodeRelativePath.Contains(node.RelativePath) //删除旧版本的文件（hash不同，无法使用Except）
                                    && !deletedFileNodeString.Contains(node.RelativePath) //排除标记已删除的文件
                                 select node).ToList();

                //读取对应版本的文件夹
                await GetFileNodesHandleAsync(lastTotalBackup, lastFileNodes);

                //添加没有变更的文件
                fileNodeList.AddRange(lastFileNodes);
            }

            BackupTaskStage = BackupStage.Restore; //将状态改为恢复中
            //更新信息
            TotalFileCount = fileNodeList.Count; //需要处理的文件总数
            ProcessedFileCount = 0; //清零

            foreach (FileNode file in fileNodeList)
            {
                WorkingFilePath = ShowDetail ? file.RelativePath : "正在恢复"; //显示正在处理的文件
                StorageFolder relativeFolder = await CreateRelativePath(rootFolder, file.RelativePath);
                await file.File.CopyAsync(relativeFolder, file.File.Name, NameCollisionOption.ReplaceExisting);
                ProcessedFileCount++;
            }

            //更新信息
            BackupTaskStage = BackupStage.Spare;
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
        
        //根据快速备份时间查询最近一个完整备份
        public BackupVersionRecord QueryLastTotalBackupVersion(string backupPath, DateTime quickBackupDateTime)
        {
            var queryResults = (from BackupVersionRecord record in BackupVersionRecordList
                                where record.BackupFolderPath == backupPath
                                where record.IsFullBackup == true
                                where record.BackupTime < quickBackupDateTime
                                orderby record.BackupTime descending
                                select record).ToList();
            return queryResults.FirstOrDefault(); //返回第一个结果
        }


        //快速备份（流程）
        async Task QuickBackup(string folderToken, string goalToken)
        {
            StorageFolder rootFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
            StorageFolder goalFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(goalToken);
            if (rootFolder == null || goalFolder == null)
            {
                if (rootFolder == null) throw new Exception("无法访问源文件夹token");
                else throw new Exception("无法访问目标文件夹token");
            }

            // 获取基本信息
            backupFolderPath = rootFolder.Path; //备份文件夹的路径

            // 获取所有文件
            IReadOnlyList<StorageFile> allFiles = await rootFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderBySearchRank);
            TotalFileCount = allFiles.Count; //需要备份的文件总数

            //读取最后一个完整备份
            BackupVersionRecord lastTotalBackup = QueryLastTotalBackupVersion(backupFolderPath);

            //计算Hash（步骤显示计算Hash）
            List<FileNode> fileNodeList = await GetFileNodesData(allFiles);

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

            ///文件比对
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
            ///Summary:
            //list1：新增项&更改项
            //list2：删除项
            BackupInfoList backupInfo = new BackupInfoList(list1, list2);

            //进行文件备份
            await BackupFolder(rootFolder, goalFolder, backupInfo, false);

            BackupTaskStage = BackupStage.Spare; //已完成，将状态恢复为空闲
        }


        #region 文件保存配置
        // 文件名称访问器
        public string BackupJsonName
        {
            get => DataSettings.BackupJsonName;
        }
        public string SaveJsonName
        {
            get => DataSettings.SaveJsonName;
        }
        public string BackupTaskJsonName
        {
            get => DataSettings.BackupTaskJsonName;
        }
        public string BackupVersionJsonName
        {
            get => DataSettings.BackupVersionJsonName;
        }
        internal StorageFolder AppFolder
        {
            get => DataSettings.AppFolder;
        }

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
                await GetPathRecordFolderAsync(BackupFolderList);

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
                await GetPathRecordFolderAsync(SaveFolderList);

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
                if (record.Folder == null) continue;
                if (await record.Folder.TryGetItemAsync(BackupVersionJsonName) != null) //文件存在
                {
                    StorageFile file = await record.Folder.GetFileAsync(BackupVersionJsonName);
                    List<BackupVersionRecord> list = JsonSerializer.Deserialize<List<BackupVersionRecord>>(await DataSettings.LoadFile(file));
                    BackupVersionRecordList.AddRange(list);
                }
            }
        }

        //读取路径列表中指定的文件夹
        async Task GetPathRecordFolderAsync(List<PathRecord> list)
        {
            foreach (PathRecord record in list)
            {
                try
                {
                    record.Folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(record.Hash);
                }
                catch (FileNotFoundException)
                {
                    AddErrorMessage(1, $"未能读取文件夹 {record.Path}");
                }
            }
        }

        //Query备份路径对应的备份记录列表到备份页的ObservableCollection
        public void GetBackupVersionList(string backupPath)
        {
            if (!string.IsNullOrEmpty(backupPath)) //检测非空
            {
                var queryResults = (from BackupVersionRecord record in BackupVersionRecordList
                                    where record.BackupFolderPath == backupPath
                                    orderby record.BackupTime descending //倒叙排列备份时间（后来的在上面）
                                    select record).ToList();
                BackupPageData._backupVersionRecords = new ObservableCollection<BackupVersionRecord>(queryResults);
            }
        }

        //保存备份文件版本信息到对应文件夹中的Json
        async Task ExportBackupVersionJsonAsync(StorageFolder saveFolder, StorageFolder backupFolder, string versionFolderName, bool isFullBackup) //保存文件夹，备份路径
        {
            List<BackupVersionRecord> list = await LoadBackupVersionFile(saveFolder); //读取列表
            BackupVersionRecord record = new BackupVersionRecord(isFullBackup, DateTime.Now, backupFolder.Path, backupFolder.Name, saveFolder.Path, versionFolderName); //创建新条目
            BackupVersionRecordList.Add(record); //添加到全局列表中
            list.Add(record); //往列表添加新条目

            DataSettings.GenerateJsonAsync(list, saveFolder, BackupVersionJsonName); //导出json
        }

        //从保存目录中读取备份信息
        async Task<List<BackupVersionRecord>> LoadBackupVersionFile(StorageFolder saveFolder)
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

        //根据备份记录删除
        public async Task DelFromBackupVersionListAsync(BackupVersionRecord backupRecord)
        {
            string backupInfoPath = $"{backupRecord.SaveFolderPath}\\{backupRecord.BackupFolderName}\\{backupRecord.VersionFolderName}.json"; //备份信息路径
            string saveVersionFolderPath = $"{backupRecord.SaveFolderPath}\\{backupRecord.BackupFolderName}\\{backupRecord.VersionFolderName}"; //版本路径

            StorageFolder saveFolder = await StorageFolder.GetFolderFromPathAsync(backupRecord.SaveFolderPath); //读取备份信息Json
            List<BackupVersionRecord> list = await LoadBackupVersionFile(saveFolder); //读取保存文件夹下的备份版本列表
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
                AddErrorMessage(1, $"{ex.Message}");
            }

            try
            {
                StorageFile backupInfoJson = await StorageFile.GetFileFromPathAsync(backupInfoPath);
                await backupInfoJson.DeleteAsync(); //删除记录文件
            }
            catch (Exception ex)
            {
                AddErrorMessage(1, $"{ex.Message}");
            }
        }

        //加载备份信息（保存列表，删除列表）
        async Task<BackupInfoList> LoadBackupInfoListAsync(BackupVersionRecord backupRecord)
        {
            string backupInfoPath = $"{backupRecord.SaveFolderPath}\\{backupRecord.BackupFolderName}\\{backupRecord.VersionFolderName}.json"; //备份信息路径

            //读取备份信息Json
            StorageFile backupInfoJson = await StorageFile.GetFileFromPathAsync(backupInfoPath);
            string contents = await DataSettings.LoadFile(backupInfoJson); //读取数据
            List<BackupInfoList> backupInfoList = JsonSerializer.Deserialize<List<BackupInfoList>>(contents); //从文件中读取列表
            return backupInfoList[0];
        }
    }
}
