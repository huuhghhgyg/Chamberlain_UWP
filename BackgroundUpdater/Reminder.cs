using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using static BackgroundUpdater.Notify;

namespace BackgroundUpdater
{
    public enum TaskState
    {
        OutOfDate,
        Onwork,
        Finished
    }

    public enum Priority
    {
        Default,
        Middle,
        High
    }
    
    internal class ReminderItem 
    {
        // 仅用于提醒的ReminderItem对象

        public TaskState TaskState { get; set; } 
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime Deadline { get; set; }

        public Priority Priority { get; set; }

        //辅助prop
        [JsonIgnore]
        public string DeadlineString //ddl的显示字符串
        {
            get
            {
                if (Deadline.ToString("d") == DateTime.Today.ToString("d")) return "今天 " + Deadline.ToString("HH:mm");
                else if (Deadline.ToString("d") == DateTime.Today.AddDays(1).ToString("d")) return "明天 " + Deadline.ToString("HH:mm");
                else if (Deadline.ToString("d") == DateTime.Today.AddDays(2).ToString("d")) return "后天 " + Deadline.ToString("HH:mm");
                else if (Deadline - DateTime.Today > TimeSpan.Zero && Deadline - DateTime.Today < new TimeSpan(7, 0, 0, 0)) return Deadline.ToString("dddd HH:mm");
                return Deadline.ToString("MM/dd HH:mm");
            }
        }

        [JsonIgnore]
        public TimeSpan TaskSpan //任务时间段长度
        {
            get
            {
                return Deadline - CreatedTime;
            }
        }

        [JsonIgnore]
        public double ProgressValue //任务时间进度（此处仅用作排序）
        {
            get
            {
                TimeSpan RemainSpan = Deadline - DateTime.Now; // 任务剩余时间

                if (RemainSpan < TimeSpan.Zero) // 判断剩余时间是否是负数
                {
                    // 是负数：未完成但过期、过期、已完成
                    if (TaskState == TaskState.Onwork) TaskState = TaskState.OutOfDate; // 没完成的任务自动过期
                    return 1;
                }
                else if (TaskState == TaskState.Finished) // 提前完成
                    return 1;
                else
                    return (1 - RemainSpan.TotalSeconds / TaskSpan.TotalSeconds); // 剩余时间不是负数，返回剩余时间比例
            }
        }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; } //其他使用不到的项，暂存并原路返回
    }

    /// <summary>
    /// ReminderManager下属两个类
    /// - Statistics ReminderItemList的项数据
    /// - Data 用于存储/读取数据
    /// </summary>
    internal static class ReminderManager
    {
        public static bool ReminderItemListLoaded = false; //已从文件读取标记（用于阻塞）

        private static List<ReminderItem> ReminderItemList = new List<ReminderItem>(); // 只能通过以下的访问器访问

        internal static class Statistics
        {
            public static int ItemCount //总项数
            {
                get { return ReminderItemList.Count; }
            }
            public static int Outdated //过期项数
            {
                get { return ReminderItemList.Where(item => item.TaskState == TaskState.OutOfDate).Count(); }
            }
            public static List<ReminderItem> AdjournmentItemList(int remain_hours) // 选出剩余时间为n小时的未完成项的列表。相关数据可以从返回的表中获取。
            {
                return ReminderItemList
                    .Where(item => item.TaskState == TaskState.Onwork)
                    .Where(item => item.TaskSpan < new TimeSpan(remain_hours, 0, 0))
                    .ToList();
            }
        }

        public static void RefreshList() // 更新列表状态
        {
            ReminderItemList.ForEach(item =>
            {
                if (item.TaskState == TaskState.Onwork && item.TaskSpan < TimeSpan.Zero) // 标记正在进行但已经过期的任务
                    item.TaskState = TaskState.OutOfDate; // 更新任务状态
            });
        }

        public static void SortListByDefault() // 按照默认顺序进行排序。建议首次使用前先更新列表状态(RefreshList)。
        {
            ReminderItemList = ReminderItemList
                .OrderBy(item => item.TaskState)
                .ThenByDescending(item => item.Priority)
                .ThenBy(item => item.Deadline)
                .ThenByDescending(item => item.ProgressValue)
                .ToList();
        }

        public static void UpdateTile() //更新磁贴内容
        {
            List<Notification4Tile> tileNotiList = new List<Notification4Tile>();
            ReminderItemList
                .Where(item => item.TaskState != TaskState.Finished)
                .OrderBy(item => item.Deadline)
                .ToList()
                .ForEach(item =>
                {
                    Notification4Tile tile = new Notification4Tile();
                    tile.title = item.Title;
                    tile.description = item.Description;
                    tile.displayName = item.DeadlineString;
                    tileNotiList.Add(tile);
                });
            NotificationManager.UpdateTileNotification(tileNotiList);
        }

        public static string ImportByJsonAsync(string json_str)
        {
            string message = "";
            try
            {
                ReminderItemList = JsonSerializer.Deserialize<List<ReminderItem>>(json_str);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return message;
        }

        public static string GenerateJson() //将数据输出为Json字符串
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
                IncludeFields = true,
            };
            string json = JsonSerializer.Serialize(ReminderItemList, options);
            return json;
        }

        public static class Data
        {
            private static string DataFilename = "ReminderData.json";
            public static bool IsDataEmpty
            {
                get
                {
                    return ReminderItemList.Count > 0 ? false : true;
                }
            }

            private static async Task<string> ExportToFile(StorageFolder folder, string file_name, CreationCollisionOption file_option, bool return_path)//导出数据到文件
            {
                string msg = "";
                try
                {
                    StorageFile file = await folder.CreateFileAsync(file_name, file_option); // 替换现有项
                    await FileIO.WriteTextAsync(file, GenerateJson());
                    if (return_path) msg = file.Path;
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                }
                return msg;
            }

            private static async Task<string> LoadFile(StorageFile file) //从文件读取数据
            {
                string msg;
                try
                {
                    string jsonContent = await FileIO.ReadTextAsync(file); // 通过传入的文件对象进行访问
                    msg = ImportByJsonAsync(jsonContent);
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                }
                return msg;
            }

            public static async Task<string> Save() //将数据保存到程序目录
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                return await ExportToFile(folder, DataFilename, CreationCollisionOption.ReplaceExisting, false); // 将数据导出到文件（本地路径）
            }

            public static async Task<string> Load() //从程序目录读取数据
            {
                string msg = "";
                StorageFolder folder; // 创建本地目录文件夹对象
                //检查是否需从指定目录读取
                if (StorageApplicationPermissions.FutureAccessList.ContainsItem("ReminderFolderToken"))
                {
                    try
                    {
                        folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("ReminderFolderToken");
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        StorageApplicationPermissions.FutureAccessList.Remove("ReminderFolderToken"); //清除指定项
                        msg = "指定的文件夹不存在，已清除";
                        folder = ApplicationData.Current.LocalFolder;
                    }
                }
                else folder = ApplicationData.Current.LocalFolder;

                try
                {
                    StorageFile file = await folder.GetFileAsync(DataFilename); // 创建文件对象
                    msg = await LoadFile(file);
                }
                catch (Exception ex)
                {
                    msg += ex.Message;
                }
                return msg;
            }

        }

    }

}