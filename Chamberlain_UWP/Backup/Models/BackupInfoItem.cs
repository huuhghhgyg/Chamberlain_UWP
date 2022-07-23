using System;
using System.Collections;
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
        [JsonIgnore]
        public StorageFile File { get; set; } //可供操作的文件对象
        public string Hash { get; set; }  //文件Hash值
        public string RelativePath { get; set; } //文件相对路径(\folder\file1.txt)
        public string RelativeFolder { get; set; } //文件相对文件夹(\folder)
        public FileNode(StorageFile file, string relativePath)
        {
            File = file;
            RelativePath = relativePath;
            RelativeFolder = relativePath.Substring(0, relativePath.Length - file.Name.Length - 1);
        }
        public FileNode(StorageFile file, string relativePath, string hash)
        {
            File = file;
            RelativePath = relativePath;
            RelativeFolder = relativePath.Substring(0, relativePath.Length - file.Name.Length - 1);
            Hash = hash;
        }
        [JsonConstructor]
        public FileNode(string relativePath, string relativeFolder, string hash) //需要后期手动添加File
        {
            RelativePath = relativePath;
            RelativeFolder = relativeFolder;
            Hash = hash;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            FileNode nodeObj = (FileNode)obj;
            return RelativePath == nodeObj.RelativePath && Hash == nodeObj.Hash;
        }
        public override int GetHashCode()
        {
            return (RelativePath + Hash).GetHashCode();
        }
        public static bool operator ==(FileNode a, FileNode b)
        {
            if (ReferenceEquals(a, b)) //都为null或者都指向同一个实例
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null)) //其中一个为null
            {
                return false;
            }

            return a.RelativePath == b.RelativePath && a.Hash == b.Hash;
        }
        public static bool operator !=(FileNode a, FileNode b) => !(a == b);
    }

    public class BackupInfoList //保存更新、添加、删除的文件节点列表
    {
        public List<FileNode> SaveList { get; set; } //更新、添加的文件节点列表
        public List<FileNode> DeleteList { get; set; } //删除的文件节点列表
        public BackupInfoList(List<FileNode> saveList, List<FileNode> deleteList)
        {
            SaveList = saveList;
            DeleteList = deleteList;
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
    public class BackupVersionRecord //备份历史版本记录
    {
        public bool IsFullBackup { get; set; } //是否完整备份
        public DateTime BackupTime { get; set; } //备份执行时间
        public string BackupFolderPath { get; set; } //备份文件夹路径
        public string BackupFolderName { get; set; } //备份文件夹名称
        public string VersionFolderName { get; set; } //备份版本文件夹名称（完整路径可以通过相加得到）
        public string SaveFolderPath { get; set; } //保存路径
        public BackupVersionRecord(bool isFullBackup, DateTime backupTime, string backupFolderPath, string backupFolderName, string saveFolderPath, string versionFolderName)
        {
            IsFullBackup = isFullBackup;
            BackupTime = backupTime;
            BackupFolderPath = backupFolderPath;
            BackupFolderName = backupFolderName;
            SaveFolderPath = saveFolderPath;
            VersionFolderName = versionFolderName;
        }
    }
}
