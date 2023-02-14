using Chamberlain_UWP.Backup;
using Chamberlain_UWP.Settings.Update;
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
            bool roaming_settings = IsSettingsRoamingEnabled = LoadSetting("IsSettingsRoamingEnabled", false); //设置是否漫游，默认不漫游

            IsNotificationEnabled = LoadSetting("IsNotificationEnabled", true, roaming_settings); //是否开启通知，默认开启

            IsNotificationBlockingVisible = LoadSetting("IsNotificationBlockingVisible", false, roaming_settings); //是否显示通知阻塞信息，默认关闭

            UpdateTriggerInterval = LoadSetting("UpdateTriggerInterval", 15, roaming_settings); //通知更新间隔，最小、默认=15

            TimepickerInterval = LoadSetting("TimepickerInterval", 1, false); //时间选择器时间间隔，最小、默认=1

            IsRemindOnTimeEnabled = LoadSetting("IsRemindOnTimeEnabled", true, roaming_settings); //是否开启每日定时通知，默认开启

            RemindTime = LoadSetting("RemindTime", new TimeSpan(17, 0, 0), roaming_settings); //每日定时通知时间，默认17：00

            CheckUpdate = LoadSetting("CheckUpdate", "Enabled", roaming_settings); //检测更新的状态，默认Enabled

            IsPaneOpen = LoadSetting("IsPaneOpen", false, roaming_settings); //程序开启时左导航栏是否打开，默认关闭
        }

        #region 数据区：
        /// 程序启动的时候读取一次，之后都是写入。
        /// 所以读取时InitialLoad()只读取非空的值，对于空值返回**默认值**。
        /// 存入的值都要在字段里面经过检验。
        /// 变量先存到程序内部，再存到设置中。
        /// * 新添加数据时，要同时添加到InitialLoad()读取、SaveAllRoaming()，如果可以漫游，添加到漫游
        private static int _updateTriggerInterval;
        private static bool _isNotificationEnabled;
        private static bool _isSettingsRoamingEnabled;
        private static bool _isNotificationBlockingVisible;
        private static int _timepickerInterval;
        private static bool _isRemindOnTimeEnabled;
        private static TimeSpan _RemindTime;
        private static string _checkUpdate;
        private static bool _isPaneOpen;

        /// <summary>
        /// 是否允许设置漫游
        /// 此项不允许添加到漫游设置中！
        /// </summary>
        public static bool IsSettingsRoamingEnabled
        {
            get { return _isSettingsRoamingEnabled; }
            set { _isSettingsRoamingEnabled = value; SaveSetting(value, "IsSettingsRoamingEnabled", false); }
        }
        /// <summary>
        /// 后台更新时间间隔（分钟）
        /// </summary>
        public static int UpdateTriggerInterval
        {
            get { return _updateTriggerInterval; }
            set
            {
                //先存到程序内部
                _updateTriggerInterval = value < 15 ? 15 : value; //不允许小于15
                //再存到设置中
                SaveSetting(value, "UpdateTriggerInterval", true); //保存数据
            }
        }
        /// <summary>
        /// 是否允许使用通知
        /// </summary>
        public static bool IsNotificationEnabled
        {
            get { return _isNotificationEnabled; }
            set { _isNotificationEnabled = value; SaveSetting(value, "IsNotificationEnabled", true); }
        }
        /// <summary>
        /// 是否显示线程阻塞信息
        /// </summary>
        public static bool IsNotificationBlockingVisible
        {
            get { return _isNotificationBlockingVisible; }
            set { _isNotificationBlockingVisible = value; SaveSetting(value, "IsNotificationBlockingVisible", true); }
        }
        /// <summary>
        /// 时间选择器最小时间间隔，默认=1
        /// </summary>
        public static int TimepickerInterval
        {
            get { return _timepickerInterval; }
            set { _timepickerInterval = value; SaveSetting(value, "TimepickerInterval", false); }
        }
        /// <summary>
        /// 是否允许每日定时通知
        /// </summary>
        public static bool IsRemindOnTimeEnabled
        {
            get { return _isRemindOnTimeEnabled; }
            set { _isRemindOnTimeEnabled = value; SaveSetting(value, "IsRemindOnTimeEnabled", true); }
        }
        /// <summary>
        /// 每日定时通知时间
        /// </summary>
        public static TimeSpan RemindTime
        {
            get { return _RemindTime; }
            set { _RemindTime = value; SaveSetting(value, "RemindTime", true); }
        }
        /// <summary>
        /// 检测更新的状态：
        /// auto（开启时自动检查更新）、false（关闭自动检查更新）、1.0.3（版本号，跳过此版本）
        /// </summary>
        public static string CheckUpdate
        {
            get { return _checkUpdate; }
            set { _checkUpdate = value; SaveSetting(value, "CheckUpdate", true); }
        }

        public static bool IsPaneOpen
        {
            get => _isPaneOpen;
            set { _isPaneOpen = value; SaveSetting(value, "IsPaneOpen", true); }
        }
        #endregion

        #region 公共变量
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings; //本地存储的设置
        private static ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings; //漫游存储的设置
        #endregion

        #region
        // 设置的有关文档参考：https://docs.microsoft.com/zh-cn/windows/uwp/get-started/settings-learning-track
        /// <summary>
        /// 保存单项设置
        /// </summary>
        /// <typeparam name="T">要保存的值的类型</typeparam>
        /// <param name="data">要保存的数据</param>
        /// <param name="name">要保存设置的名称</param>
        /// <param name="roaming">是否允许数据漫游保存。如果允许，需要到SaveAllRoaming()中添加保存项</param>
        private static void SaveSetting<T>(T data, string name, bool roaming = false) //保存设置。如果允许漫游则漫游。字段值更改的时候引用。
        {
            Type t = (localSettings.Values[name] ?? data).GetType();
            if (t != typeof(T)) //如果类型不符，则删除数据记录
            {
                if (roaming)
                    roamingSettings.Values.Remove(name);
                localSettings.Values.Remove(name);
            }

            if (roaming)
                roamingSettings.Values[name] = data; //保存到漫游数据
            localSettings.Values[name] = data; //不管是否漫游，都要保存到本地数据
        }

        /// <summary>
        /// 允许设置漫游时执行，将所有可以漫游的数据保存到Roaming
        /// 取消漫游时不需要额外执行，取消后做的更改不会再漫游
        /// </summary>
        public static void SaveAllRoaming()
        {
            SaveSetting(UpdateTriggerInterval, "UpdateTriggerInterval", true);
            SaveSetting(IsNotificationEnabled, "IsNotificationEnabled", true);
            SaveSetting(IsRemindOnTimeEnabled, "IsRemindOnTimeEnabled", true);
            SaveSetting(RemindTime, "RemindTime", true);
            SaveSetting(CheckUpdate, "CheckUpdate", true);
            SaveSetting(IsPaneOpen, "IsPaneOpen", true);
        }
        /// <summary>
        /// 读取设置
        /// </summary>
        /// <typeparam name="T">设置值的类型</typeparam>
        /// <param name="dataName">设置的名称</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="roaming">是否允许数据漫游，如果不允许直接填入false</param>
        /// <returns>读取到的值</returns>
#nullable enable
        private static T LoadSetting<T>(string dataName, T defaultValue, bool roaming = false)
        {
            try
            {
                object data = roaming ? roamingSettings.Values[dataName] : localSettings.Values[dataName];
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
                return (T)(data ?? defaultValue);
#pragma warning restore CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
            }
            catch (System.InvalidCastException)
            {
                return defaultValue;
            }
        }
        #endregion
    }
}