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
    public class ReminderItem : INotifyPropertyChanged, IComparer<ReminderItem>,IComparable
    {
        //属性区域
        private string title { get; set; }
        public string description { get; set; }
        public List<string> tags { get; set; }
        public DateTime deadline { get; set; }
        public DateTime CreatedTime { get; }
        public int TaskState { get; set; } // 0 - 未完成；1 - 已完成； -1 - 过期

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
                    if (TaskState == 0) TaskState = -1; // 没完成的任务自动过期
                    return 1.ToString("#0.0%"); // ToString()中的%自动将小数转化为百分数形式
                }
                else
                {
                    return ((1 - RemainSpan.TotalSeconds / TaskSpan.TotalSeconds)).ToString("#0.0%"); // 返回剩余时间比例
                }
            }
        }
        public bool IsReminderDone
        {
            get
            {
                return (TaskState == 1) ? true : false; //返回是否已完成
            }
            set
            {
                if (value && TaskState != 1)
                {
                    TaskState = 1;
                    NotifyPropertyChanged("TaskState");
                }
                else if (!value && TaskState != 0)
                {
                    TaskState = 0;
                    NotifyPropertyChanged("TaskState");
                }
            }
        }

        public string TagsString
        {
            get { return "( " + string.Join(", ", tags) + " )"; }
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

        public int Compare(ReminderItem x, ReminderItem y)
        {
            return x.TaskState - y.TaskState;
        }

        public int CompareTo(object obj)
        {
            ReminderItem item = (ReminderItem)obj;
            return item.TaskState - this.TaskState;
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
        public static int FindReminderItemWithTitle(string name) // 返回在List中的下标
        {
            ReminderItem item = ReminderItemList.Find(p => p.Title == name);
            return ReminderItemList.IndexOf(item);
        }

        public static void GetTags(ObservableCollection<string> tagList)
        {
            List<string> tags = new List<string>();
            GetTags(tags); //调用重载方法
            tags.ForEach(tag => tagList.Add(tag)); //List类型转换
        }

        public static void GetTags(List<string> tagList)
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

        // 代码清理时，如果没有引用，可以删除这个重载
        public static void GetReminderList(List<ReminderItem> collection, int taskstate) // 用于按条件复制到List
        {
            var reminder_item_list = ReminderItemList.Where(item => item.TaskState == taskstate).ToList(); // 筛选符合taskstate的元素
            reminder_item_list.ForEach(item => collection.Add(item)); // 添加元素
        }

        public static void UpdateReminderList(ObservableCollection<ReminderItem> collection) // 导入ObservableCollection的数据更新List
        {
            //ReminderItemList.Clear();
            //ReminderItemList.AddRange(collection);
            ReminderItemList = new List<ReminderItem>(collection);
        }

        public static void SortReminderListbyTaskState()
        {
            ReminderItemList.Sort((x,y) => x.TaskState.CompareTo(y.TaskState));
        }

        public static void SortCollectionByTaskState(ObservableCollection<ReminderItem> collection)
        {
            int length = collection.Count;

            // 排序测试（为了UI方面不奇怪，只能这么繁琐...）
            int operate_num = 0; //已完成数
            foreach (ReminderItem item in collection)
            {
                if(item.TaskState == 1) operate_num++;
            }

            int operated = 0;
            operated = 0;
            for (int i = 0; i < length - operated;)
            {
                if (collection[i].TaskState == 1)
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
