using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;

namespace Chamberlain_UWP.Backup.Models
{
    internal class BackupManager:INotifyPropertyChanged
    {
        // 变量区
        private string backup_folder_path = ""; //备份文件夹的路径
        public List<PathRecord> BackupFolderList = new List<PathRecord>(); //备份文件夹路径列表
        public List<PathRecord> SaveFolderList = new List<PathRecord>(); //目标文件夹路径列表
        public List<BackupTask> BackupTaskList = new List<BackupTask>(); //备份任务描述列表
        int _totalFileCount = 0; //需要备份的文件总数
        int _backupedFileCount = 0; //已备份文件总数
        string _workingFilePath = ""; //正在备份的文件

        public int TotalFileCount
        {
            get => _totalFileCount;
            set
            {
                _totalFileCount = value;
                OnPropertyChanged(nameof(TotalFileCount));
            }
        }
        public int BackupedFileCount
        {
            get => _backupedFileCount;
            set
            {
                _backupedFileCount = value;
                OnPropertyChanged(nameof(BackupedFileCount));
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
                if (TotalFileCount != 0) _backupProgress = 100.0 * BackupedFileCount / TotalFileCount;
                return $"{_backupProgress.ToString("0.0")}%";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // 函数区
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task<string> GetMD5HashAsync(StorageFile file) //可用
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

        public void QueryBackupTask(string backup_path, string save_path,out PathRecord backup_record,out PathRecord save_record)
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
                if(rootFolder == null) throw new Exception("无法访问源文件夹token");
                else throw new Exception("无法访问目标文件夹token");
            }

            // 获取基本信息
            backup_folder_path = rootFolder.Path; //备份文件夹的路径

            // 获取所有文件
            IReadOnlyList<StorageFile> all_files = await rootFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderBySearchRank);
            TotalFileCount = all_files.Count; //需要备份的文件总数
            List<FileNode> file_node_list = new List<FileNode>();
            foreach(StorageFile file in all_files)
            {
                string relative_path = file.Path.Substring(backup_folder_path.Length);
                file_node_list.Add(new FileNode(file, relative_path));
            }

            //在保存目录创建文件夹(2022-07-17-1105)
            string backup_date = DateTime.Now.ToString("yyyy-MM-dd-HHmm");

            StorageFolder versionFolder = await goalFolder.CreateFolderAsync(backup_date);

            foreach (FileNode fileNode in file_node_list)
            {
                WorkingFilePath = fileNode.File.Path; //更新正在工作的文件路径

                List<string> subfolders = fileNode.RelativeFolder.Split("\\").ToList(); //切割相对路径
                subfolders.RemoveAt(0); //第一个值为空，删掉

                StorageFolder relative_folder = await CreateRelativePath(versionFolder, subfolders);
                await fileNode.File.CopyAsync(relative_folder);
                BackupedFileCount++; //增加一个完成文件
            }

            ; // 结束标志
        }

        async Task<StorageFolder> CreateRelativePath(StorageFolder folder, List<string> paths)
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
            //清除记录
            TotalFileCount = 0;
            BackupedFileCount = 0;

            PathRecord backup_record, save_record;
            QueryBackupTask(backup_path, save_path, out backup_record, out save_record);

            TotalBackupFolder(backup_record.Hash, save_record.Hash);
        }
    }
}
