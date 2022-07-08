using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Chamberlain_UWP.Backup.Models
{
    internal static class TreeManager
    {
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

        static async Task<FolderNode> GetFolder(StorageFolder folder)
        {
            // Application now has read/write access to all contents in the picked folder
            // (including other sub-folder contents)
            //Windows.Storage.AccessCache.StorageApplicationPermissions.
            //FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            string folderName = folder.Name;

            IReadOnlyList<StorageFolder> folder_list = await folder.GetFoldersAsync();

            IReadOnlyList<StorageFile> file_list = await folder.GetFilesAsync();
            
            return null;
        }

    }
}
