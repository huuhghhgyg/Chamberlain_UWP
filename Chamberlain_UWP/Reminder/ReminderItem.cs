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

    public class ReminderItem : INotifyPropertyChanged
    {
        //属性区域
        private string title { get; set; }
        public string description { get; set; }
        public List<string> tags { get; set; }
        public DateTime deadline { get; set; }
        public DateTime CreatedTime { get; } // 一经创建，不可更改
        public TaskState TaskState { get; set; } // 0 - 未完成；1 - 已完成； -1 - 过期

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
        public double ProgressValue
        {
            get
            {
                TimeSpan TaskSpan = Deadline - CreatedTime; // 任务时间长度
                TimeSpan RemainSpan = Deadline - DateTime.Now; // 任务剩余时间

                if(RemainSpan < TimeSpan.Zero) // 判断剩余时间是否是负数
                {
                    // 是负数：未完成但过期、过期、已完成
                    if (TaskState == TaskState.Onwork) TaskState = TaskState.OutOfDate; // 没完成的任务自动过期
                    return 1;
                }
                else return (1 - RemainSpan.TotalSeconds / TaskSpan.TotalSeconds); // 剩余时间不是负数，返回剩余时间比例
            }
        }

        public string ProgressString
        {
            get
            {
                if(TaskState > 0) // 判断任务是否过期
                {
                    return ProgressValue.ToString("#0.0%"); // 没过期
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

        public ReminderItem(string title, string desc, List<string> tags, DateTime ddl, TaskState taskstate)
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
}
