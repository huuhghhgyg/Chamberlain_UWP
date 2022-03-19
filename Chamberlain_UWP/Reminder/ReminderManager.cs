using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chamberlain_UWP.Reminder
{
    public static class ReminderManager
    {
        private static List<ReminderItem> ReminderItemList = new List<ReminderItem>(); // 只能通过以下的访问器访问

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

        public static void UpdateList(ObservableCollection<ReminderItem> collection) // 导入ObservableCollection的数据更新List
        {
            ReminderItemList = new List<ReminderItem>(collection);
        }

        public static void SortListByDefault()
        {
            ReminderItemList = ReminderItemList
                .OrderBy(item => item.TaskState)
                .ThenByDescending(item => item.Priority)
                .ThenByDescending(item => item.ProgressValue)
                .ToList();
        }

        /* 问题：如果已经按照日期排了序，这里就没有必要考虑TaskState=OutOfDate的情况，
        {
            int length = collection.Count;

            {
                if (item.TaskState == TaskState.Finished) finished_num++;
            }

            {
                int finished = 0;
        }

        }

    }
}
