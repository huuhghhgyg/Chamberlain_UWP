using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
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
        /// <summary>
        /// 属性区域：大部分都是私有的局部属性，一些没有必要用访问器的直接做成public
        /// 访问器区域：修改私有属性，顺便做其它操作。
        /// 辅助prop区域：基本上是get类型的访问器，用于辅助xaml的x:bind
        /// </summary>

        #region 属性

        private string _title;
        private string _description;
        private List<string> _tags;
        private DateTime _deadline;
        [JsonInclude]
        public readonly DateTime CreatedTime; // 创建后不更改，只读的public属性
        [JsonInclude]
        public TaskState TaskState { get; set; } // 更改不需要触发事件，public
        //旧： 0 - 未完成；1 - 已完成； -1 - 过期
        //新： 0 - 已过期；1 - 未完成；2 - 已完成
        [JsonIgnore]
        public Priority _priority;

        #endregion

        #region 访问器
        [JsonInclude]
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                NotifyPropertyChanged(nameof(Title));
            }
        }

        [JsonInclude]
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                NotifyPropertyChanged(nameof(Description));
            }
        }

        [JsonInclude]
        public List<string> Tags
        {
            get => _tags;
            set
            {
                _tags = value;
                NotifyPropertyChanged(nameof(Tags));
            }
        }

        [JsonInclude]
        public DateTime Deadline
        {
            get => _deadline;
            set
            {
                _deadline = value;
                TaskSpan = _deadline - CreatedTime; //更改deadline时会影响到taskspan的长度
                NotifyPropertyChanged(nameof(DeadlineString)); //注意控件里面绑定的是DeadlineString，所以Notify要填DeadlineString
                NotifyPropertyChanged(nameof(ProgressValue));
                NotifyPropertyChanged(nameof(ProgressString));
            }
        }

        [JsonInclude]
        public Priority Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                _priority = value;
                NotifyPropertyChanged(nameof(PriorityString));
            }
        }

        //辅助prop
        [JsonIgnore]
        public string DeadlineString
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
        public TimeSpan TaskSpan; // 任务时间长度

        [JsonIgnore]
        public double ProgressValue
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

        [JsonIgnore]
        public string ProgressString
        {
            get
            {
                if (TaskState > 0) // 判断任务是否过期
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

        [JsonIgnore]
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

                NotifyPropertyChanged(nameof(TaskState));
                NotifyPropertyChanged(nameof(ProgressValue));
                NotifyPropertyChanged(nameof(ProgressString));
                NotifyPropertyChanged(nameof(IsReminderDone));
            }
        }

        [JsonIgnore]
        public string TagsString
        {
            get => "标签：" + string.Join(", ", _tags);
        }

        [JsonIgnore]
        public string PriorityString
        {
            get
            {
                switch (Priority)
                {
                    case Priority.Middle: return "🟡(优先)";
                    case Priority.High: return "🔴(紧急)";
                    default: return "";
                }
            }
        }

        #endregion

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
            TaskSpan = Deadline - CreatedTime; // 计算得到任务时长
        }

        // 存在引用，仅用于JsonSerializer创建实例
        [JsonConstructor]
        public ReminderItem(string title, string description, List<string> tags, DateTime deadline, TaskState taskstate, Priority priority, DateTime createdTime)
        {
            //仅用于JsonSerializer创建实例
            Title = title;
            Description = description;
            Tags = new List<string>();
            Tags.AddRange(tags);
            Deadline = deadline;
            TaskState = taskstate;
            Priority = priority;
            CreatedTime = createdTime;
            TaskSpan = Deadline - CreatedTime; // 计算得到任务时长
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetDeadline(DateTime dt)
        {
            TaskState = TaskState.Onwork; // 重置任务状态
            Deadline = dt; // 先更新TaskState，deadline状态需要用到taskstate
        }

        // 如果需要重新设置创建时间，则需要重新创建一个ReminderItem

        public void RefreshProgress()
        {
            NotifyPropertyChanged(nameof(TaskState));
            NotifyPropertyChanged(nameof(ProgressValue));
            NotifyPropertyChanged(nameof(ProgressString));
        }
    }
}
