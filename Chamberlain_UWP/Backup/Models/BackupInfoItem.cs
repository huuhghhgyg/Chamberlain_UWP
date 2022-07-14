using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Chamberlain_UWP.Backup.Models
{
    public class FileNode //文件节点
    {
        public StorageFile File { get; set; } //可供操作的文件对象
        public string Hash { get; set; } //文件哈希值
        public string RelativePath { get; set; } //文件相对路径
        public FileNode(StorageFile file, string hash, string relative_path)
        {
            File = file;
            Hash = hash;
            RelativePath = relative_path;
        }
    }
    public abstract class PathRecord //路径记录
    {
        public string Path { get; set; }
        public StorageFolder Folder { get; set; }
        public int Hash { get => Folder.Path.GetHashCode(); }

        public PathRecord(string path, StorageFolder folder)
        {
            Path = path;
            Folder = folder;
        }
    }
    public class BackupPathRecord : PathRecord //备份路径记录
    {
        public string BackupFolderPath
        {
            get => Path;
            set => Path = value;
        }
        public StorageFolder BackupFolder
        {
            get => Folder;
            set => Folder = value;
        }
        public BackupPathRecord(string path, StorageFolder folder) : base(path, folder)
        {
            BackupFolderPath = path;
            Folder = folder;
        }
    }
    public class SavePathRecord : PathRecord //备份路径记录
    {
        public string SaveFolderPath
        {
            get => Path;
            set => Path = value;
        }
        public SavePathRecord(string path, StorageFolder folder) : base(path, folder)
        {
            SaveFolderPath = path;
            Folder = folder;
        }
    }

    public class BackupTask //备份任务描述
    {
        public PathRecord BackupFolder { get; set; }
        public PathRecord GoalFolder { get; set; }
        public string BackupFolderName { get => BackupFolder.Folder.Name; }
        public string BackupFolderPath { get => BackupFolder.Path; }
        public string SaveFolderName { get => GoalFolder.Folder.Name; }
        public string SaveFolderPath { get => GoalFolder.Path; }
        public int BackupTaskHash { get => (BackupFolderPath + SaveFolderPath).GetHashCode(); }

        public BackupTask(PathRecord backupFolder, PathRecord goalFolder)
        {
            BackupFolder = backupFolder;
            GoalFolder = goalFolder;
        }
    }
}
