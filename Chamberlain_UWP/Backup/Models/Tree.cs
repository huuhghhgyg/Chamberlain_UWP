using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Chamberlain_UWP.Backup.Models
{
    internal class FolderNode //文件夹节点
    {
        public string Name { get; set; } //文件夹名称
        public int foldernum { get => Folders.Count; } //文件夹下的文件夹数量
        public StorageFolder Folder { get; set; } //可供操作的文件夹对象
        public List<FolderNode> Folders { get; set; } //此文件夹下的文件夹列表，便于往下访问
        public List<FileNode> FileNodes { get; set; } //此文件夹下的文件节点列表

        public FolderNode(string name, StorageFolder folder, List<FolderNode> folders, List<FileNode> fileNodes)
        {
            Name = name;
            Folder = folder;
            Folders = folders;
            FileNodes = fileNodes;
        }
    }

    internal class FileNode //文件节点
    {
        public StorageFile File { get; set; } //可供操作的文件对象
        public string Hash { get; set; } //文件哈希值

        public FileNode(StorageFile file, string hash)
        {
            File = file;
            Hash = hash;
        }
    }
}
