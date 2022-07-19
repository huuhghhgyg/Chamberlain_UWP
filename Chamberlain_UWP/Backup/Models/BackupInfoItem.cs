using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;

namespace Chamberlain_UWP.Backup.Models
{
    public class FileNode //文件节点
    {
        public StorageFile File { get; set; } //可供操作的文件对象
        public string Hash { get; set; }  //文件Hash值
        public string RelativePath { get; set; } //文件相对路径
        public string RelativeFolder { get; set; } //文件相对文件夹
        public FileNode(StorageFile file, string relative_path)
        {
            File = file;
            RelativePath = relative_path;
            RelativeFolder = relative_path.Substring(0, relative_path.Length - file.Name.Length - 1);
        }
        public FileNode(StorageFile file, string relative_path, string hash)
        {
            File = file;
            RelativePath = relative_path;
            RelativeFolder = relative_path.Substring(0, relative_path.Length - file.Name.Length - 1);
            Hash = hash;
        }

    }
    public class PathRecord //路径记录
    {
        [JsonIgnore]
        public StorageFolder Folder { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; } //路径本身的Hash

        public PathRecord(StorageFolder folder)
        {
            Folder = folder;
            Path = Folder.Path;
            Name = Folder.Name;
            Hash = Math.Abs(Folder.Path.GetHashCode()).ToString();
        }

        [JsonConstructor]
        public PathRecord(string path, string name, string hash)
        {
            Path = path;
            Name = name;
            Hash = hash;
        }
    }
}
