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

        /* 问题：如果已经按照日期排了序，这里就没有必要考虑TaskState=OutOfDate的情况，
         *      TaskState是根据日期计算出来的。
         *      所以还有必要考虑OutOfDate吗
         * */
        public static void SortCollectionByTaskState(ObservableCollection<ReminderItem> collection)
        {
            int length = collection.Count;

            //排序测试（为了UI方面不奇怪，只能这么繁琐...）
            int finished_num = 0; //操作计数
            int operate_num = 0;
            foreach (ReminderItem item in collection)
            {
                if (item.TaskState == TaskState.Finished) finished_num++;
                if (item.TaskState != TaskState.Onwork) operate_num++;
            }

            if (operate_num > 0) // 有必要进行排序
            {
                int finished = 0;
                int operated = 0;
                for (int i = 0; i < length - finished;)
                {
                    if (operated < operate_num)
                    {
                        if (collection[i].TaskState == TaskState.Finished)//已完成
                        {
                            if (finished < finished_num)
                            {
                                collection.Move(i, length - 1); // 移到末尾
                                finished++;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else if (collection[i].TaskState == TaskState.OutOfDate)//过期
                        {
                            collection.Move(i, 0); // 移到开头
                            i++;
                        }
                        else i++; // 正在进行
                    }
                    else break; // 排序已完成
                }
            }

        }

        public static void SortCollectionByDeadline(ObservableCollection<ReminderItem> collection) // 先做。否则影响动画
        {
            List<ReminderItem> list = new List<ReminderItem>(collection);
            list.Sort((x, y) => x.Deadline.CompareTo(y.Deadline));
            collection.Clear();
            list.ForEach(item => collection.Add(item));
        }

    }
}
