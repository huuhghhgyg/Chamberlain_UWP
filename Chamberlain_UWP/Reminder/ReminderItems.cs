using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Chamberlain_UWP.Reminder
{
    public class ReminderItem : INotifyPropertyChanged
    {
        //属性区域
        public string title { get; set; }
        public string description { get; set; }
        public List<string> tags { get; set; }
        public DateTime deadline { get; set; }
        public DateTime CreatedTime { get; }
        public int TaskState { get; set; } // 0 - 未完成；1 - 已完成； 2 - 过期

        //访问器区域
        public string Title
        {
            get { return title; }
            set { title = value; NotifyPropertyChanged("Title"); }
        }
        public string Description
        {
            get { return description; }
            set { description = value; NotifyPropertyChanged("Description"); }
        }
        public List<string> Tags
        {
            get { return tags; }
            set { tags = value; NotifyPropertyChanged("Tags"); }
        }
        public DateTime Deadline
        {
            get { return deadline; }
            set { 
                deadline = value; 
                NotifyPropertyChanged("DeadlineString"); //注意控件里面绑定的是DeadlineString，所以Notify要填DeadlineString
                NotifyPropertyChanged("ProgressValue");
            }
        }

        //辅助prop
        public string DeadlineString
        {
            get
            {
                return Deadline.ToString("MM/dd HH:mm");
            }
        }
        public string ProgressValue
        {
            get
            {
                TimeSpan TaskSpan = Deadline - CreatedTime; // 任务时间长度
                TimeSpan RemainSpan = Deadline - DateTime.Now; // 任务剩余时间
                if (RemainSpan < TimeSpan.Zero) // 剩余时间为负数
                {
                    if (TaskState == 0) TaskState = 2; // 没完成的任务自动过期
                    return 1.ToString("#0.0%"); // ToString()中的%自动将小数转化为百分数形式
                }
                else
                {
                    return ((1 - RemainSpan.TotalSeconds / TaskSpan.TotalSeconds)).ToString("#0.0%"); // 返回剩余时间比例
                }
            }
        }

        public ReminderItem(string title, string desc, List<string> tags, DateTime ddl, int taskstate)
        {
            Title = title;
            Description = desc;
            Tags = new List<string>();
            Tags.AddRange(tags);
            CreatedTime = DateTime.Now;
            Deadline = ddl;
            TaskState = taskstate;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetDeadline(DateTime dt)
        {
            Deadline = dt;
            TaskState = 0; // 重置任务状态
        }

        // 如果需要重新设置创建时间，则需要重新创建一个ReminderItem

        public void RefreshProgress()
        {
            NotifyPropertyChanged("ProgressValue");
        }
    }

    public class ReminderManager
    {
        private static List<ReminderItem> ReminderItemList = new List<ReminderItem>(); // 只能通过以下的访问器访问

        public static void AddReminder(ReminderItem item)
        {
            ReminderItemList.Add(item);
        }

        public static List<ReminderItem> FindReminderItemWithTag(string tag)
        {
            List<ReminderItem> filterd = ReminderItemList.FindAll(
                delegate (ReminderItem item)
                {
                    return item.Tags.Contains(tag);
                });
            return filterd;
        }

        public static void GetTags(ObservableCollection<string> tagList)
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

        public static void GetReminderList(ObservableCollection<ReminderItem> collection) // 用于初始化ObservableCollection
        {
            ReminderItemList.ForEach(item => collection.Add(item));
        }
        public static void GetReminderList(ObservableCollection<ReminderItem> collection, int taskstate) // 用于初始化ObservableCollection
        {
            var reminder_item_list = ReminderItemList.Where(item => item.TaskState == taskstate).ToList(); // 筛选符合taskstate的元素
            reminder_item_list.ForEach(item => collection.Add(item)); // 添加元素
        }


        public static void UpdateReminderList(ObservableCollection<ReminderItem> collection) // 导入ObservableCollection的数据更新List
        {
            ReminderItemList.Clear();
            ReminderItemList.AddRange(collection);
        }
    }
}
