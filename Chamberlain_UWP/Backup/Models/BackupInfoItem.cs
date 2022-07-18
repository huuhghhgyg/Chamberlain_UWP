using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Chamberlain_UWP.Backup.Models
{
    public class FileNode //文件节点
    {
        public StorageFile File { get; set; } //可供操作的文件对象
        public int Hash { get => File.GetHashCode(); } //文件哈希值
        public string RelativePath { get; set; } //文件相对路径
        public string RelativeFolder { get; set; } //文件相对文件夹
        public FileNode(StorageFile file, string relative_path)
        {
            File = file;
            RelativePath = relative_path;
            RelativeFolder = relative_path.Substring(0, relative_path.Length - file.Name.Length - 1);
        }
    }
    public class PathRecord //路径记录
    {
        public StorageFolder Folder { get; set; }
        public string Path { get => Folder.Path; }
        public string Name { get => Folder.Name; }
        public string Hash { get => Folder.Path.GetHashCode().ToString(); } //路径本身的Hash

        public PathRecord(StorageFolder folder)
        {
            Folder = folder;
        }
    }

    public class BackupTask //备份任务描述
    {
        public PathRecord BackupFolder { get; set; }
        public PathRecord SaveFolder { get; set; }
        public string BackupFolderName { get => BackupFolder.Folder.Name; }
        public string BackupFolderPath { get => BackupFolder.Path; }
        public string SaveFolderName { get => SaveFolder.Folder.Name; }
        public string SaveFolderPath { get => SaveFolder.Path; }
        public int BackupTaskHash { get => (BackupFolderPath + SaveFolderPath).GetHashCode(); }

        public BackupTask(PathRecord backupFolder, PathRecord goalFolder)
        {
            BackupFolder = backupFolder;
            SaveFolder = goalFolder;
        }
    }
}
