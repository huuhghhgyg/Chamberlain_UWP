using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Windows.Storage;
using static Chamberlain_UWP.Notify;
using Windows.Storage.AccessCache;

namespace Chamberlain_UWP.Reminder
{
    public static class ReminderManager
    {
        private static List<ReminderItem> ReminderItemList = new List<ReminderItem>(); // 只能通过以下的访问器访问

        private static string DataFilename = "ReminderData.json";

        public static int ItemCountOnwork
        {
            get { return ReminderItemList.Where(item => item.TaskState != TaskState.Finished).ToList().Count; }
        }

        public static void Add(ReminderItem item)
        {
            ReminderItemList.Add(item);
        }

        // 名称以Find开头，返回index
        public static int FindItemWithTitle(string name) // 返回在List中的下标
        {
            ReminderItem item = ReminderItemList.Find(p => p.Title == name);
            return ReminderItemList.IndexOf(item);
        }

        // 名称以Get开头，返回List或Collection
        public static void GetTagCollection(ObservableCollection<string> tagList)
        {
            List<string> tags = new List<string>();
            GetTagList(tags); //调用重载方法
            tags.ForEach(tag => tagList.Add(tag)); //List类型转换
        }

        public static void GetTagList(List<string> tagList)
        {
            List<string> AllTags = new List<string>(); // 存放所有的tag
            //foreach (ReminderItem reminder_item in ReminderItemList)
            //{
            //    AllTags.AddRange(reminder_item.Tags
            //                        .Where(t => !AllTags.Contains(t)) // AllTag列表里面没有的才储存
            //                        .ToList()); // 导入AllTags
            //}
            ReminderItemList.ForEach(reminder_item =>
                AllTags.AddRange(reminder_item.Tags
                        .Where(t => !AllTags.Contains(t)) // AllTag列表里面没有的才储存
                        .ToList())); // 导入AllTags
            tagList.AddRange(AllTags); // 导出
        }

        public static void GetList(ObservableCollection<ReminderItem> collection) // 用于初始化ObservableCollection
        {
            ReminderItemList.ForEach(item => collection.Add(item));
        }

        public static void GetList(ObservableCollection<ReminderItem> collection, TaskState taskstate) // 用于初始化ObservableCollection（有条件）
        {
            var reminder_item_list = ReminderItemList.Where(item => item.TaskState == taskstate).ToList(); // 筛选符合taskstate的元素
            reminder_item_list.ForEach(item => collection.Add(item)); // 添加元素
        }

        public static void GetList(List<ReminderItem> list) // 只读访问函数
        {
            ReminderItemList.ForEach(item => list.Add(item));
        }

        public static void GetList(List<ReminderItem> collection, TaskState taskstate) // 用于按条件复制到List
        {
            var reminder_item_list = ReminderItemList.Where(item => item.TaskState == taskstate).ToList(); // 筛选符合taskstate的元素
            reminder_item_list.ForEach(item => collection.Add(item)); // 添加元素
        }

        public static async void UpdateList(ObservableCollection<ReminderItem> collection) // 导入ObservableCollection的数据更新List
        {
            ReminderItemList = new List<ReminderItem>(collection);
            await Data.Save();
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

        static int onwork_count_cache = 0;
        static int update_timespan_cache = 1000;
        public static int UpdateTimeSpan //计算进度条更新的时间间隔
        {
            get
            {
                if (onwork_count_cache != ItemCountOnwork)
                {
                    onwork_count_cache = ItemCountOnwork;
                    TimeSpan task_span = ReminderItemList[0].TaskSpan;
                    ReminderItemList.Where(item => item.TaskState == TaskState.Onwork) // 筛选正在进行的项（需要更新进度）
                        .ToList()
                        .ForEach(item =>
                        {
                            if (task_span > item.TaskSpan)
                                task_span = item.TaskSpan;
                        });// 找到时间间隔最小的项
                    update_timespan_cache = (int)task_span.TotalMilliseconds / 1000; // 计算时间间隔
                }
                return update_timespan_cache;
            }
        }

        public static void SortListByDefault() // 按照默认顺序进行排序
        {
            ReminderItemList = ReminderItemList
                .OrderBy(item => item.TaskState)
                .ThenByDescending(item => item.Priority)
                .ThenBy(item => item.Deadline)
                .ThenByDescending(item => item.ProgressValue)
                .ToList();
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

        public static string GenerateJson()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
                IncludeFields = true,
            };
            return JsonSerializer.Serialize(ReminderItemList, options);
        }

        public static void UpdateListProgress()
        {
            ReminderItemList.Where(item => item.TaskState == TaskState.Onwork)
                .ToList()
                .ForEach(item => item.RefreshProgress());
        }

        /// <summary>
        /// 数据操作类：负责进行各种文件数据操作
        /// </summary>
        public static class Data
        {
            // 导出为文件
            public static async Task<string> ExportToFile(StorageFolder folder, string file_name, CreationCollisionOption file_option, bool return_path)
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

            // 读取文件
            public static async Task<string> LoadFile(StorageFile file)
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

            public static async Task<string> Save()
            {
                UpdateTile(); //更新磁贴
                
                StorageFolder folder;
                //检查是否需要存到指定目录
                if (StorageApplicationPermissions.FutureAccessList.ContainsItem("ReminderFolderToken"))
                {
                    try
                    {
                        folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("ReminderFolderToken");
                        await ExportToFile(folder, DataFilename, CreationCollisionOption.ReplaceExisting, true); // 将数据先导出到本地数据文件夹
                    }
                    catch (System.IO.FileNotFoundException) //文件夹不存在
                    {
                        StorageApplicationPermissions.FutureAccessList.Remove("ReminderFolderToken"); //清除指定项
                        folder = ApplicationData.Current.LocalFolder;
                    }
                }
                else folder = ApplicationData.Current.LocalFolder;
                return await ExportToFile(folder, DataFilename, CreationCollisionOption.ReplaceExisting, true); // 将数据导出到文件（本地路径）
            }

            public static async Task<string> Load()
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
                    catch(System.IO.FileNotFoundException)
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

            public static bool IsDataEmpty
            {
                get
                {
                    return ReminderItemList.Count > 0 ? false : true;
                }
            }

            //public static async Task<bool> IsDataNullOrEmpty() // 无法作为字段，只能作为方法
            //{
            //    StorageFolder folder = ApplicationData.Current.LocalFolder;

            //    if (System.IO.File.Exists(folder.Path + DataFilename))
            //    {
            //        try
            //        {
            //            StorageFile file = await folder.GetFileAsync(DataFilename);
            //            string content = await FileIO.ReadTextAsync(file); // 通过传入的文件对象进行访问
            //            if (string.IsNullOrEmpty(content)) return true; // 文件为空
            //            else return false; // 文件不为空
            //        }
            //        catch
            //        {
            //            return true;
            //        }
            //    }
            //    else return true;
            //}

            public static async void Clear() // 清除保存在本地目录的数据文件
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                string path = folder.Path + '\\' + DataFilename;
                if (System.IO.File.Exists(path))
                {
                    StorageFile file = await folder.GetFileAsync(DataFilename);
                    await file.DeleteAsync();
                    ReminderItemList.Clear(); // 清除内存中的list
                }
            }
        }

    }
}
