using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Chamberlain_UWP.Settings
{
    internal static class DataSettings //帮助管理应用数据存取
    {
        public static async Task<string> ExportToFile(StorageFolder folder, string file_name, string contents) //返回保存路径，默认覆盖重名文件
        {
            StorageFile file = await folder.CreateFileAsync(file_name, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, contents);
            return file.Path; //返回保存路径
        }
        public static async Task<string> LoadFile(StorageFile file) => await FileIO.ReadTextAsync(file); //读取文件内容
    }
}
