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

    public class ReminderItem : INotifyPropertyChanged
    {
        //属性区域
        private string title { get; set; }
        private string description { get; set; }
        private List<string> tags { get; set; }
        private DateTime deadline { get; set; }
        private DateTime CreatedTime { get; } // 一经创建，不可更改
        public TaskState TaskState { get; set; }
        //旧： 0 - 未完成；1 - 已完成； -1 - 过期
        //新： 0 - 已过期；1 - 未完成；2 - 已完成
        public Priority priority { get; set; }

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
                NotifyPropertyChanged("ProgressString");
            }
        }

        public Priority Priority
        {
            get
            {
                return priority;
            }
            set
            {
                priority = value;
                NotifyPropertyChanged("PriorityString");
            }
        }

        //辅助prop
        [JsonIgnore]
        public string DeadlineString
        {
            get
            {
                return Deadline.ToString("MM/dd HH:mm");
            }
        }

        [JsonIgnore]
        public double ProgressValue
        {
            get
            {
                TimeSpan TaskSpan = Deadline - CreatedTime; // 任务时间长度
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

        public string ProgressString
        {
            get
            {
                if(TaskState > 0) // 判断任务是否过期
                {
                    // 没过期
                    if (TaskState == TaskState.Onwork) return ProgressValue.ToString("#0.0%"); // 正在进行
                    else return "已完成"; // 已完成
                }
                else
                {
                    return "已过期";
                }
            }
        }

        public bool IsReminderDone
        {
            get
            {
                return (TaskState == TaskState.Finished) ? true : false; //返回是否已完成
            }
            set
            {
                if (value)
                {
                    TaskState = TaskState.Finished;
                }
                else if (ProgressValue < 100)
                {
                    TaskState = TaskState.Onwork;
                }
                else
                {
                    TaskState = TaskState.OutOfDate;
                }

                NotifyPropertyChanged("TaskState");
                NotifyPropertyChanged("ProgressValue");
                NotifyPropertyChanged("ProgressString");
            }
        }

        public string TagsString
        {
            get { return "( " + string.Join(", ", tags) + " )"; }
        }

        public string PriorityString
        {
            get
            {
                switch (Priority)
                {
                    case Priority.Middle: return "（中优先级）";
                    case Priority.High: return "（高优先级）";
                    default: return "";
                }
            }
        }

        public ReminderItem(string title, string desc, List<string> tags, DateTime ddl, TaskState taskstate)
        {
            Title = title;
            Description = desc;
            Tags = new List<string>();
            Tags.AddRange(tags);
            CreatedTime = DateTime.Now;
            Deadline = ddl;
            TaskState = taskstate;
            Priority = Priority.Default; // 0
        }
        public ReminderItem(string title, string desc, List<string> tags, DateTime ddl, TaskState taskstate, Priority pri)
        {
            Title = title;
            Description = desc;
            Tags = new List<string>();
            Tags.AddRange(tags);
            CreatedTime = DateTime.Now;
            Deadline = ddl;
            TaskState = taskstate;
            Priority = pri;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetDeadline(DateTime dt)
        {
            Deadline = dt;
            TaskState = TaskState.Onwork; // 重置任务状态
        }

        // 如果需要重新设置创建时间，则需要重新创建一个ReminderItem

        public void RefreshProgress()
        {
            NotifyPropertyChanged("ProgressValue");
        }
    }
}
