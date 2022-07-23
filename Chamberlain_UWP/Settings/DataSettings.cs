using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        public static async void GenerateJsonAsync<T>(List<T> list, StorageFolder exportFolder, string filename) //根据对象生成Json文本后直接保存到而文件
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
                IncludeFields = true,
            };
            string jsonContent = JsonSerializer.Serialize(list, options); //序列化

            await ExportToFile(exportFolder, filename, jsonContent); //导出到文件
        }
    }
}
