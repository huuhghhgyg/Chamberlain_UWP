using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Chamberlain_UWP.Backup.Models
{
    internal class FileNode //文件节点
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
    internal class PathRecord //路径记录
    {
        public string Path { get; set; }
        public StorageFolder Folder { get; set; }
        public int Hash { get => Folder.Path.GetHashCode(); }
    }
    internal class BackupTask //备份任务描述
    {
        public PathRecord BackupFolder { get; set; }
        public PathRecord GoalFolder { get; set; }

        public BackupTask(PathRecord backupFolder, PathRecord goalFolder)
        {
            BackupFolder = backupFolder;
            GoalFolder = goalFolder;
        }
    }
}
