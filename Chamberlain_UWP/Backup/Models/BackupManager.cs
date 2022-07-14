using System;
using System.Collections.Generic;
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
    internal static class BackupManager
    {
        // 变量区
        private static string backup_folder_path = ""; //备份文件夹的路径
        public static List<PathRecord> BackupFolderList = new List<PathRecord>(); //备份文件夹路径列表
        public static List<PathRecord> GoalFolderList = new List<PathRecord>(); //目标文件夹路径列表
        public static List<BackupTask> BackupTaskList = new List<BackupTask>(); //备份任务描述列表

        // 函数区
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

        public static async void QuickBackupFolder(string folder_token, string goal_token) // 快速备份
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

        public static async void TotalBackupFolder(string folder_token, string goal_token) // 完整备份
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
            List<FileNode> file_node_list = new List<FileNode>();
            foreach(StorageFile file in all_files)
            {
                string md5 = await GetMD5HashAsync(file);
                string relative_path = file.Path.Substring(backup_folder_path.Length);
                file_node_list.Add(new FileNode(file, md5, relative_path));
            }
        }
    }
}
