using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Chamberlain_UWP.Settings
{
    internal static class SettingsConfig
    {
        /// <summary>
        /// 程序启动时加载所有设置
        /// 如果没有设置（初始情况），读取的值为null，需要设置初始值
        /// 按照目前的模型，如果需要更改默认值，还需要更改后台任务中读取时的默认值设定
        /// </summary>
        public static void InitialLoad()
        {
            object cache;

            cache = localSettings.Values["IsSettingsRoamingEnabled"];
            bool roaming_settings = IsSettingsRoamingEnabled = (cache == null) ? false : (bool)cache; //设置是否漫游，默认不漫游

            cache = roaming_settings ? roamingSettings.Values["IsNotificationEnabled"] : localSettings.Values["IsNotificationEnabled"];
            IsNotificationEnabled = (cache == null) ? true : (bool)cache; //是否开启通知，默认开启

            cache = roaming_settings ? roamingSettings.Values["IsNotificationBlockingVisible"] : localSettings.Values["IsNotificationBlockingVisible"];
            IsNotificationBlockingVisible = (cache == null) ? false : (bool)cache; //是否显示通知阻塞信息，默认关闭

            cache = roaming_settings ? roamingSettings.Values["UpdateTriggerInterval"] : localSettings.Values["UpdateTriggerInterval"];
            UpdateTriggerInterval = (cache == null) ? 15 : (int)cache; ; //通知更新间隔，最小、默认=15

            cache = localSettings.Values["TimepickerInterval"];
            TimepickerInterval = (cache == null) ? 1 : (int)cache; ; //时间选择器时间间隔，最小、默认=1

            cache = roaming_settings ? roamingSettings.Values["IsRemindOnTimeEnabled"] : localSettings.Values["IsRemindOnTimeEnabled"];
            IsRemindOnTimeEnabled = (cache == null) ? true : (bool)cache; //是否开启每日定时通知，默认开启

            cache = roaming_settings ? roamingSettings.Values["RemindTime"] : localSettings.Values["RemindTime"];
            RemindTime = (cache == null) ? new TimeSpan(17,0,0) : (TimeSpan)cache; //每日定时通知时间，默认17：00
        }

        /// <summary>
        /// 数据区：
        /// 程序启动的时候读取一次，之后都是写入。
        /// 所以读取时InitialLoad()只读取非空的值，对于空值返回**默认值**。
        /// 存入的值都要在字段里面经过检验。
        /// 变量先存到程序内部，再存到设置中。
        /// * 新添加数据时，要同时添加到InitialLoad()读取、SaveAllRoaming()，如果可以漫游，添加到漫游
        /// </summary>
        private static int _updateTriggerInterval;
        private static bool _isNotificationEnabled;
        private static bool _isSettingsRoamingEnabled;
        private static bool _isNotificationBlockingVisible;
        private static int _timepickerInterval;
        private static bool _isRemindOnTimeEnabled;
        private static TimeSpan _RemindTime;

        public static bool IsSettingsRoamingEnabled //是否允许设置漫游，不允许添加到漫游设置中！
        {
            get { return _isSettingsRoamingEnabled; }
            set { _isSettingsRoamingEnabled = value; SaveSetting(value, "IsSettingsRoamingEnabled", false); }
        } 
        public static int UpdateTriggerInterval //后台更新时间间隔（分钟）
        {
            get { return _updateTriggerInterval; }
            set
            {
                //先存到程序内部
                _updateTriggerInterval = value < 15 ? 15 : value; //不允许小于15
                //再存到设置中
                SaveSetting(value, "UpdateTriggerInterval",true); //保存数据
            }
        }
        public static bool IsNotificationEnabled //是否允许使用通知
        {
            get { return _isNotificationEnabled; }
            set { _isNotificationEnabled = value; SaveSetting(value, "IsNotificationEnabled", true); }
        }
        public static bool IsNotificationBlockingVisible //是否显示线程阻塞信息
        {
            get { return _isNotificationBlockingVisible; }
            set { _isNotificationBlockingVisible = value; SaveSetting(value, "IsNotificationBlockingVisible", true); }
        }
        public static int TimepickerInterval
        {
            get { return _timepickerInterval; }
            set { _timepickerInterval = value; SaveSetting(value, "TimepickerInterval", false); }
        }
        public static bool IsRemindOnTimeEnabled //是否允许每日定时通知
        {
            get { return _isRemindOnTimeEnabled; }
            set { _isRemindOnTimeEnabled = value; SaveSetting(value, "IsRemindOnTimeEnabled", true); }
        }
        public static TimeSpan RemindTime //每日定时通知时间
        {
            get { return _RemindTime; }
            set { _RemindTime = value; SaveSetting(value, "RemindTime", true); }
        }


        /// <summary>
        /// 公共变量
        /// </summary>
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings; //本地存储的设置
        private static ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings; //漫游存储的设置

        /// <summary>
        /// 设置的有关文档参考：https://docs.microsoft.com/zh-cn/windows/uwp/get-started/settings-learning-track
        /// </summary>
        private static void SaveSetting(object data, string name, bool roaming_granted) //保存设置。如果允许漫游则漫游。字段值更改的时候引用。
        {
            if (roaming_granted)
                roamingSettings.Values[name] = data; //保存到漫游数据
            localSettings.Values[name] = data; //不管是否漫游，都要保存到本地数据
        }
        public static void SaveAllRoaming() //允许设置漫游时执行，将所有可以漫游的数据保存到Roaming
        {
            SaveSetting(UpdateTriggerInterval, "UpdateTriggerInterval", true);
            SaveSetting(IsNotificationEnabled, "IsNotificationEnabled", true);
            SaveSetting(IsRemindOnTimeEnabled, "IsRemindOnTimeEnabled", true);
            SaveSetting(RemindTime, "RemindTime", true);
        }
        //取消漫游时不需要额外执行，取消后做的更改不会再漫游
    }
}