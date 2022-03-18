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

        // Find开头返回index
        public static int FindItemWithTitle(string name) // 返回在List中的下标
        {
            ReminderItem item = ReminderItemList.Find(p => p.Title == name);
            return ReminderItemList.IndexOf(item);
        }

        // Get开头返回List或Collection
        public static void GetTagCollection(ObservableCollection<string> tagList)
        {
            List<string> tags = new List<string>();
            GetTagList(tags); //调用重载方法
            tags.ForEach(tag => tagList.Add(tag)); //List类型转换
        }

        public static void GetTagList(List<string> tagList)
        {
            List<string> AllTags = new List<string>(); // 存放所有的tag
            List<string> TagsCache = new List<string>(); // 存放在一个实例中找到的tag
            foreach (ReminderItem reminder_item in ReminderItemList)
            {
                TagsCache = reminder_item.Tags.Where(t => !AllTags.Contains(t)).ToList(); // AllTag列表里面没有的才储存
                AllTags.AddRange(TagsCache); // 导入AllTags
                TagsCache.Clear();
            }
            AllTags.ForEach(t => tagList.Add(t)); // 添加标签
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

        public static void GetList(List<ReminderItem> collection, TaskState taskstate) // 用于按条件复制到List
        {
            var reminder_item_list = ReminderItemList.Where(item => item.TaskState == taskstate).ToList(); // 筛选符合taskstate的元素
            reminder_item_list.ForEach(item => collection.Add(item)); // 添加元素
        }

        public static void UpdateList(ObservableCollection<ReminderItem> collection) // 导入ObservableCollection的数据更新List
        {
            ReminderItemList = new List<ReminderItem>(collection);
        }

        public static void SortListByTaskState()
        {
            ReminderItemList.Sort((x, y) => x.TaskState.CompareTo(y.TaskState));
        }

        public static void SortCollectionByTaskState(ObservableCollection<ReminderItem> collection)
        {
            int length = collection.Count;

            // 排序测试（为了UI方面不奇怪，只能这么繁琐...）
            int operate_num = 0; //已完成数
            foreach (ReminderItem item in collection)
            {
                if (item.TaskState == TaskState.Finished) operate_num++;
            }

            int operated = 0;
            operated = 0;
            for (int i = 0; i < length - operated;)
            {
                if (collection[i].TaskState == TaskState.Finished)
                {
                    //已完成
                    collection.Move(i, length - 1);
                    operated++;
                }
                else if (i < length - 1 - operate_num) i++; // 不能确定是否需要排序
                else break; // 确定不需要排序
            }
        }
    }
}
