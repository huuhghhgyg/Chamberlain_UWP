using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace Chamberlain_UWP.Settings
{
    /// <summary>
    /// 帮助管理应用产生的数据的存取
    /// </summary>
    internal static class DataSettings
    {
        #region 文件名
        /// <summary>
        /// 存放于应用文件夹
        /// </summary>
        public static readonly string BackupJsonName = "BackupFolders.json";
        /// <summary>
        /// 存放于应用文件夹
        /// </summary>
        public static readonly string SaveJsonName = "SaveFolders.json";
        /// <summary>
        /// 存放于应用文件夹
        /// </summary>
        public static readonly string BackupTaskJsonName = "BackupTasks.json";
        /// <summary>
        /// 存放于备份目录文件夹
        /// </summary>
        public static readonly string BackupVersionJsonName = "BackupVersion.json";
        internal static readonly StorageFolder AppFolder = ApplicationData.Current.LocalFolder;
        #endregion
        /// <summary>
        /// 返回保存路径，默认覆盖重名文件
        /// </summary>
        /// <param name="folder">文件夹对象</param>
        /// <param name="file_name">文件名</param>
        /// <param name="contents">内容</param>
        /// <returns></returns>
        public static async Task<string> ExportToFile(StorageFolder folder, string file_name, string contents)
        {
            StorageFile file = await folder.CreateFileAsync(file_name, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, contents);
            return file.Path; //返回保存路径
        }
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="file">文件对象</param>
        /// <returns>输入文件对象的文本内容</returns>
        public static async Task<string> LoadFile(StorageFile file) => await FileIO.ReadTextAsync(file);

        /// <summary>
        /// 根据对象生成Json文本后直接保存到而文件
        /// </summary>
        /// <typeparam name="T">可序列化的类</typeparam>
        /// <param name="list">类的列表</param>
        /// <param name="exportFolder">输出文件夹的文件夹对象</param>
        /// <param name="filename">文件名</param>
        public static async void GenerateJsonAsync<T>(List<T> list, StorageFolder exportFolder, string filename)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
                IncludeFields = true,
            };
            string jsonContent = JsonSerializer.Serialize(list, options); //序列化

            await ExportToFile(exportFolder, filename, jsonContent); //导出到文件
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="folder">文件夹对象</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static async Task DeleteFile(StorageFolder folder, string fileName)
        {
            if (await folder.TryGetItemAsync(fileName) != null)
            {
                StorageFile file = await AppFolder.GetFileAsync(fileName);
                await file.DeleteAsync();
            }
        }
    }
}
