// File generated automatically by ReswPlus. https://github.com/DotNetPlus/ReswPlus
// The NuGet package ReswPlusLib is necessary to support Pluralization.
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Data;

namespace Chamberlain_UWP.Strings{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("DotNetPlus.ReswPlus", "2.1.3")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public static class Resources {
        private static ResourceLoader _resourceLoader;
        static Resources()
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");
        }

        #region About
        /// <summary>
        ///   Looks up a localized string similar to: 关于
        /// </summary>
        public static string About
        {
            get
            {
                return _resourceLoader.GetString("About");
            }
        }
        #endregion

        #region Add
        /// <summary>
        ///   Looks up a localized string similar to: 添加
        /// </summary>
        public static string Add
        {
            get
            {
                return _resourceLoader.GetString("Add");
            }
        }
        #endregion

        #region AddItem
        /// <summary>
        ///   Looks up a localized string similar to: 添加项
        /// </summary>
        public static string AddItem
        {
            get
            {
                return _resourceLoader.GetString("AddItem");
            }
        }
        #endregion

        #region Application
        /// <summary>
        ///   Looks up a localized string similar to: 应用
        /// </summary>
        public static string Application
        {
            get
            {
                return _resourceLoader.GetString("Application");
            }
        }
        #endregion

        #region ApplicationFolder
        /// <summary>
        ///   Looks up a localized string similar to: 应用程序文件夹
        /// </summary>
        public static string ApplicationFolder
        {
            get
            {
                return _resourceLoader.GetString("ApplicationFolder");
            }
        }
        #endregion

        #region AutoDetectUpdateTitle
        /// <summary>
        ///   Looks up a localized string similar to: 自动检测更新
        /// </summary>
        public static string AutoDetectUpdateTitle
        {
            get
            {
                return _resourceLoader.GetString("AutoDetectUpdateTitle");
            }
        }
        #endregion

        #region BackgroundTask
        /// <summary>
        ///   Looks up a localized string similar to: 后台任务
        /// </summary>
        public static string BackgroundTask
        {
            get
            {
                return _resourceLoader.GetString("BackgroundTask");
            }
        }
        #endregion

        #region BackgroundWakeSpan
        /// <summary>
        ///   Looks up a localized string similar to: 后台唤醒间隔（分钟）
        /// </summary>
        public static string BackgroundWakeSpan
        {
            get
            {
                return _resourceLoader.GetString("BackgroundWakeSpan");
            }
        }
        #endregion

        #region BackupDone
        /// <summary>
        ///   Looks up a localized string similar to: 已完成
        /// </summary>
        public static string BackupDone
        {
            get
            {
                return _resourceLoader.GetString("BackupDone");
            }
        }
        #endregion

        #region BackupInfoTitle
        /// <summary>
        ///   Looks up a localized string similar to: 备份信息
        /// </summary>
        public static string BackupInfoTitle
        {
            get
            {
                return _resourceLoader.GetString("BackupInfoTitle");
            }
        }
        #endregion

        #region Backuping
        /// <summary>
        ///   Looks up a localized string similar to: 正在备份文件
        /// </summary>
        public static string Backuping
        {
            get
            {
                return _resourceLoader.GetString("Backuping");
            }
        }
        #endregion

        #region BackupPath
        /// <summary>
        ///   Looks up a localized string similar to: 备份路径
        /// </summary>
        public static string BackupPath
        {
            get
            {
                return _resourceLoader.GetString("BackupPath");
            }
        }
        #endregion

        #region BackupPathExistDesc
        /// <summary>
        ///   Looks up a localized string similar to: 检测到路径已经存在，路径将不会被添加
        /// </summary>
        public static string BackupPathExistDesc
        {
            get
            {
                return _resourceLoader.GetString("BackupPathExistDesc");
            }
        }
        #endregion

        #region BackupProcessing
        /// <summary>
        ///   Looks up a localized string similar to: 处理中
        /// </summary>
        public static string BackupProcessing
        {
            get
            {
                return _resourceLoader.GetString("BackupProcessing");
            }
        }
        #endregion

        #region BackupRestoring
        /// <summary>
        ///   Looks up a localized string similar to: 正在恢复文件
        /// </summary>
        public static string BackupRestoring
        {
            get
            {
                return _resourceLoader.GetString("BackupRestoring");
            }
        }
        #endregion

        #region BackupState
        /// <summary>
        ///   Looks up a localized string similar to: 备份状态
        /// </summary>
        public static string BackupState
        {
            get
            {
                return _resourceLoader.GetString("BackupState");
            }
        }
        #endregion

        #region BackupTask
        /// <summary>
        ///   Looks up a localized string similar to: 备份任务
        /// </summary>
        public static string BackupTask
        {
            get
            {
                return _resourceLoader.GetString("BackupTask");
            }
        }
        #endregion

        #region BackupTaskSeq
        /// <summary>
        ///   Looks up a localized string similar to: 任务序列
        /// </summary>
        public static string BackupTaskSeq
        {
            get
            {
                return _resourceLoader.GetString("BackupTaskSeq");
            }
        }
        #endregion

        #region BackupTaskSequenceDesc
        /// <summary>
        ///   Looks up a localized string similar to: 剩余{0}条任务
        /// </summary>
        public static string BackupTaskSequenceDesc(int count)
        {
            return string.Format(_resourceLoader.GetString("BackupTaskSequenceDesc"), count);
        }
        #endregion

        #region BackupTitle
        /// <summary>
        ///   Looks up a localized string similar to: 备份
        /// </summary>
        public static string BackupTitle
        {
            get
            {
                return _resourceLoader.GetString("BackupTitle");
            }
        }
        #endregion

        #region BasicInfo
        /// <summary>
        ///   Looks up a localized string similar to: 基本信息
        /// </summary>
        public static string BasicInfo
        {
            get
            {
                return _resourceLoader.GetString("BasicInfo");
            }
        }
        #endregion

        #region CalculatingHash
        /// <summary>
        ///   Looks up a localized string similar to: 正在计算文件Hash
        /// </summary>
        public static string CalculatingHash
        {
            get
            {
                return _resourceLoader.GetString("CalculatingHash");
            }
        }
        #endregion

        #region Cancel
        /// <summary>
        ///   Looks up a localized string similar to: 取消
        /// </summary>
        public static string Cancel
        {
            get
            {
                return _resourceLoader.GetString("Cancel");
            }
        }
        #endregion

        #region CheckUpdateWhenStartup
        /// <summary>
        ///   Looks up a localized string similar to: {0}开启应用时自动检查更新
        /// </summary>
        public static string CheckUpdateWhenStartup(string hintString)
        {
            return string.Format(_resourceLoader.GetString("CheckUpdateWhenStartup"), hintString);
        }
        #endregion

        #region Clear
        /// <summary>
        ///   Looks up a localized string similar to: 清除
        /// </summary>
        public static string Clear
        {
            get
            {
                return _resourceLoader.GetString("Clear");
            }
        }
        #endregion

        #region ClearBackupDataConfirmDesc
        /// <summary>
        ///   Looks up a localized string similar to: 是否要清除备份模块的数据？保存于备份文件夹中的数据将不会被清除
        /// </summary>
        public static string ClearBackupDataConfirmDesc
        {
            get
            {
                return _resourceLoader.GetString("ClearBackupDataConfirmDesc");
            }
        }
        #endregion

        #region ClearBackupDataTitle
        /// <summary>
        ///   Looks up a localized string similar to: 清除备份列表
        /// </summary>
        public static string ClearBackupDataTitle
        {
            get
            {
                return _resourceLoader.GetString("ClearBackupDataTitle");
            }
        }
        #endregion

        #region ClearDataListDesc
        /// <summary>
        ///   Looks up a localized string similar to: 这可能有助于修复崩溃问题
        /// </summary>
        public static string ClearDataListDesc
        {
            get
            {
                return _resourceLoader.GetString("ClearDataListDesc");
            }
        }
        #endregion

        #region ClearDataListTitle
        /// <summary>
        ///   Looks up a localized string similar to: 清除所有列表数据
        /// </summary>
        public static string ClearDataListTitle
        {
            get
            {
                return _resourceLoader.GetString("ClearDataListTitle");
            }
        }
        #endregion

        #region ClearExternalReminderDataDesc
        /// <summary>
        ///   Looks up a localized string similar to: 是否一并删除？
        /// </summary>
        public static string ClearExternalReminderDataDesc
        {
            get
            {
                return _resourceLoader.GetString("ClearExternalReminderDataDesc");
            }
        }
        #endregion

        #region ClearExternalReminderDataTitle
        /// <summary>
        ///   Looks up a localized string similar to: 您正在使用指定文件夹中的数据
        /// </summary>
        public static string ClearExternalReminderDataTitle
        {
            get
            {
                return _resourceLoader.GetString("ClearExternalReminderDataTitle");
            }
        }
        #endregion

        #region ClearReminderData
        /// <summary>
        ///   Looks up a localized string similar to: 清除Reminder数据
        /// </summary>
        public static string ClearReminderData
        {
            get
            {
                return _resourceLoader.GetString("ClearReminderData");
            }
        }
        #endregion

        #region ClearReminderDataDialogDesc
        /// <summary>
        ///   Looks up a localized string similar to: 删除的数据不可恢复，确定删除？
        /// </summary>
        public static string ClearReminderDataDialogDesc
        {
            get
            {
                return _resourceLoader.GetString("ClearReminderDataDialogDesc");
            }
        }
        #endregion

        #region ClearReminderDataDialogTitle
        /// <summary>
        ///   Looks up a localized string similar to: 您正在尝试删除Reminder的数据
        /// </summary>
        public static string ClearReminderDataDialogTitle
        {
            get
            {
                return _resourceLoader.GetString("ClearReminderDataDialogTitle");
            }
        }
        #endregion

        #region Close
        /// <summary>
        ///   Looks up a localized string similar to: 关闭
        /// </summary>
        public static string Close
        {
            get
            {
                return _resourceLoader.GetString("Close");
            }
        }
        #endregion

        #region ComparingInfo
        /// <summary>
        ///   Looks up a localized string similar to: 正在比对信息
        /// </summary>
        public static string ComparingInfo
        {
            get
            {
                return _resourceLoader.GetString("ComparingInfo");
            }
        }
        #endregion

        #region ComparingToFullBackup
        /// <summary>
        ///   Looks up a localized string similar to: 正在与完整备份进行比对
        /// </summary>
        public static string ComparingToFullBackup
        {
            get
            {
                return _resourceLoader.GetString("ComparingToFullBackup");
            }
        }
        #endregion

        #region ComponentSettings
        /// <summary>
        ///   Looks up a localized string similar to: 组件设置
        /// </summary>
        public static string ComponentSettings
        {
            get
            {
                return _resourceLoader.GetString("ComponentSettings");
            }
        }
        #endregion

        #region Confirm
        /// <summary>
        ///   Looks up a localized string similar to: 确定
        /// </summary>
        public static string Confirm
        {
            get
            {
                return _resourceLoader.GetString("Confirm");
            }
        }
        #endregion

        #region ControlSettings
        /// <summary>
        ///   Looks up a localized string similar to: 控件设置
        /// </summary>
        public static string ControlSettings
        {
            get
            {
                return _resourceLoader.GetString("ControlSettings");
            }
        }
        #endregion

        #region CreatedTime
        /// <summary>
        ///   Looks up a localized string similar to: 创建时间
        /// </summary>
        public static string CreatedTime
        {
            get
            {
                return _resourceLoader.GetString("CreatedTime");
            }
        }
        #endregion

        #region DailyNotification
        /// <summary>
        ///   Looks up a localized string similar to: 每日定时通知
        /// </summary>
        public static string DailyNotification
        {
            get
            {
                return _resourceLoader.GetString("DailyNotification");
            }
        }
        #endregion

        #region DailyNotificationTime
        /// <summary>
        ///   Looks up a localized string similar to: 每日定时提醒时间
        /// </summary>
        public static string DailyNotificationTime
        {
            get
            {
                return _resourceLoader.GetString("DailyNotificationTime");
            }
        }
        #endregion

        #region DataSavePath
        /// <summary>
        ///   Looks up a localized string similar to: 指定位置存放数据
        /// </summary>
        public static string DataSavePath
        {
            get
            {
                return _resourceLoader.GetString("DataSavePath");
            }
        }
        #endregion

        #region DateEmptyPrompt
        /// <summary>
        ///   Looks up a localized string similar to: 日期不能为空
        /// </summary>
        public static string DateEmptyPrompt
        {
            get
            {
                return _resourceLoader.GetString("DateEmptyPrompt");
            }
        }
        #endregion

        #region DateEmptyPromptDesc
        /// <summary>
        ///   Looks up a localized string similar to: ddl日期必须要有📅
        /// </summary>
        public static string DateEmptyPromptDesc
        {
            get
            {
                return _resourceLoader.GetString("DateEmptyPromptDesc");
            }
        }
        #endregion

        #region DeadlineDate
        /// <summary>
        ///   Looks up a localized string similar to: Deadline日期
        /// </summary>
        public static string DeadlineDate
        {
            get
            {
                return _resourceLoader.GetString("DeadlineDate");
            }
        }
        #endregion

        #region DeadlineTime
        /// <summary>
        ///   Looks up a localized string similar to: Deadline时间
        /// </summary>
        public static string DeadlineTime
        {
            get
            {
                return _resourceLoader.GetString("DeadlineTime");
            }
        }
        #endregion

        #region DebugOption
        /// <summary>
        ///   Looks up a localized string similar to: 调试选项
        /// </summary>
        public static string DebugOption
        {
            get
            {
                return _resourceLoader.GetString("DebugOption");
            }
        }
        #endregion

        #region Delete
        /// <summary>
        ///   Looks up a localized string similar to: 删除
        /// </summary>
        public static string Delete
        {
            get
            {
                return _resourceLoader.GetString("Delete");
            }
        }
        #endregion

        #region DetailedInfo
        /// <summary>
        ///   Looks up a localized string similar to: 详细信息
        /// </summary>
        public static string DetailedInfo
        {
            get
            {
                return _resourceLoader.GetString("DetailedInfo");
            }
        }
        #endregion

        #region Developer
        /// <summary>
        ///   Looks up a localized string similar to: 开发者
        /// </summary>
        public static string Developer
        {
            get
            {
                return _resourceLoader.GetString("Developer");
            }
        }
        #endregion

        #region DoNotCheckUpdate
        /// <summary>
        ///   Looks up a localized string similar to: {0}不检查更新
        /// </summary>
        public static string DoNotCheckUpdate(string hintString)
        {
            return string.Format(_resourceLoader.GetString("DoNotCheckUpdate"), hintString);
        }
        #endregion

        #region DownloadNewVersion
        /// <summary>
        ///   Looks up a localized string similar to: 前往下载
        /// </summary>
        public static string DownloadNewVersion
        {
            get
            {
                return _resourceLoader.GetString("DownloadNewVersion");
            }
        }
        #endregion

        #region DragJsonFileHere
        /// <summary>
        ///   Looks up a localized string similar to: 将Json文件拖到此处
        /// </summary>
        public static string DragJsonFileHere
        {
            get
            {
                return _resourceLoader.GetString("DragJsonFileHere");
            }
        }
        #endregion

        #region DueTime
        /// <summary>
        ///   Looks up a localized string similar to: 到期时间
        /// </summary>
        public static string DueTime
        {
            get
            {
                return _resourceLoader.GetString("DueTime");
            }
        }
        #endregion

        #region EmptyIndicator
        /// <summary>
        ///   Looks up a localized string similar to: （空）
        /// </summary>
        public static string EmptyIndicator
        {
            get
            {
                return _resourceLoader.GetString("EmptyIndicator");
            }
        }
        #endregion

        #region EnableNotification
        /// <summary>
        ///   Looks up a localized string similar to: 开启通知
        /// </summary>
        public static string EnableNotification
        {
            get
            {
                return _resourceLoader.GetString("EnableNotification");
            }
        }
        #endregion

        #region EncounterProblem
        /// <summary>
        ///   Looks up a localized string similar to: 遇到问题？
        /// </summary>
        public static string EncounterProblem
        {
            get
            {
                return _resourceLoader.GetString("EncounterProblem");
            }
        }
        #endregion

        #region Error
        /// <summary>
        ///   Looks up a localized string similar to: 错误
        /// </summary>
        public static string Error
        {
            get
            {
                return _resourceLoader.GetString("Error");
            }
        }
        #endregion

        #region ErrorInformation
        /// <summary>
        ///   Looks up a localized string similar to: 错误信息
        /// </summary>
        public static string ErrorInformation
        {
            get
            {
                return _resourceLoader.GetString("ErrorInformation");
            }
        }
        #endregion

        #region ErrorMessageUnableSaveTo
        /// <summary>
        ///   Looks up a localized string similar to: 无法保存到 {0}
        /// </summary>
        public static string ErrorMessageUnableSaveTo(string filename)
        {
            return string.Format(_resourceLoader.GetString("ErrorMessageUnableSaveTo"), filename);
        }
        #endregion

        #region ExportToDownloadFolder
        /// <summary>
        ///   Looks up a localized string similar to: 导出至下载
        /// </summary>
        public static string ExportToDownloadFolder
        {
            get
            {
                return _resourceLoader.GetString("ExportToDownloadFolder");
            }
        }
        #endregion

        #region ExternalReminderDataAccessible
        /// <summary>
        ///   Looks up a localized string similar to: 已启用，具有访问权限：{0}
        /// </summary>
        public static string ExternalReminderDataAccessible(string path)
        {
            return string.Format(_resourceLoader.GetString("ExternalReminderDataAccessible"), path);
        }
        #endregion

        #region ExternalReminderFolderDataDetectedDesc
        /// <summary>
        ///   Looks up a localized string similar to: 从这个文件内导入，还是覆盖这个文件？
        /// </summary>
        public static string ExternalReminderFolderDataDetectedDesc
        {
            get
            {
                return _resourceLoader.GetString("ExternalReminderFolderDataDetectedDesc");
            }
        }
        #endregion

        #region ExternalReminderFolderDataDetectedTitle
        /// <summary>
        ///   Looks up a localized string similar to: 检测到选定的文件夹内存在数据文件
        /// </summary>
        public static string ExternalReminderFolderDataDetectedTitle
        {
            get
            {
                return _resourceLoader.GetString("ExternalReminderFolderDataDetectedTitle");
            }
        }
        #endregion

        #region ExternalReminderFolderUnavailable
        /// <summary>
        ///   Looks up a localized string similar to: 指定文件夹不存在，已被清除
        /// </summary>
        public static string ExternalReminderFolderUnavailable
        {
            get
            {
                return _resourceLoader.GetString("ExternalReminderFolderUnavailable");
            }
        }
        #endregion

        #region ExternalReminderFolderUnspecified
        /// <summary>
        ///   Looks up a localized string similar to: 未指定任何文件夹
        /// </summary>
        public static string ExternalReminderFolderUnspecified
        {
            get
            {
                return _resourceLoader.GetString("ExternalReminderFolderUnspecified");
            }
        }
        #endregion

        #region FeedbackOnGithub
        /// <summary>
        ///   Looks up a localized string similar to: 前往Github反馈
        /// </summary>
        public static string FeedbackOnGithub
        {
            get
            {
                return _resourceLoader.GetString("FeedbackOnGithub");
            }
        }
        #endregion

        #region FileNotExist
        /// <summary>
        ///   Looks up a localized string similar to: 复制时文件不存在：{0}
        /// </summary>
        public static string FileNotExist(string path)
        {
            return string.Format(_resourceLoader.GetString("FileNotExist"), path);
        }
        #endregion

        #region Finished
        /// <summary>
        ///   Looks up a localized string similar to: 已完成
        /// </summary>
        public static string Finished
        {
            get
            {
                return _resourceLoader.GetString("Finished");
            }
        }
        #endregion

        #region GetBackupPathFailed
        /// <summary>
        ///   Looks up a localized string similar to: 无法获取备份文件夹 {0}
        /// </summary>
        public static string GetBackupPathFailed(string backupPath)
        {
            return string.Format(_resourceLoader.GetString("GetBackupPathFailed"), backupPath);
        }
        #endregion

        #region GetSavePathFailed
        /// <summary>
        ///   Looks up a localized string similar to: 无法获取保存文件夹 {0}
        /// </summary>
        public static string GetSavePathFailed(string savePath)
        {
            return string.Format(_resourceLoader.GetString("GetSavePathFailed"), savePath);
        }
        #endregion

        #region GoAhead
        /// <summary>
        ///   Looks up a localized string similar to: 前往
        /// </summary>
        public static string GoAhead
        {
            get
            {
                return _resourceLoader.GetString("GoAhead");
            }
        }
        #endregion

        #region Import
        /// <summary>
        ///   Looks up a localized string similar to: 导入
        /// </summary>
        public static string Import
        {
            get
            {
                return _resourceLoader.GetString("Import");
            }
        }
        #endregion

        #region ImportBackup
        /// <summary>
        ///   Looks up a localized string similar to: 导入备份
        /// </summary>
        public static string ImportBackup
        {
            get
            {
                return _resourceLoader.GetString("ImportBackup");
            }
        }
        #endregion

        #region ImportFailed
        /// <summary>
        ///   Looks up a localized string similar to: ⚠导入失败
        /// </summary>
        public static string ImportFailed
        {
            get
            {
                return _resourceLoader.GetString("ImportFailed");
            }
        }
        #endregion

        #region ImportReminderJson
        /// <summary>
        ///   Looks up a localized string similar to: 导入Reminder(JSON)
        /// </summary>
        public static string ImportReminderJson
        {
            get
            {
                return _resourceLoader.GetString("ImportReminderJson");
            }
        }
        #endregion

        #region ImportSuccessful
        /// <summary>
        ///   Looks up a localized string similar to: ✅导入成功
        /// </summary>
        public static string ImportSuccessful
        {
            get
            {
                return _resourceLoader.GetString("ImportSuccessful");
            }
        }
        #endregion

        #region InstallDate
        /// <summary>
        ///   Looks up a localized string similar to: 安装日期
        /// </summary>
        public static string InstallDate
        {
            get
            {
                return _resourceLoader.GetString("InstallDate");
            }
        }
        #endregion

        #region JsonOutputOnly
        /// <summary>
        ///   Looks up a localized string similar to: JSON（仅输出）
        /// </summary>
        public static string JsonOutputOnly
        {
            get
            {
                return _resourceLoader.GetString("JsonOutputOnly");
            }
        }
        #endregion

        #region Label
        /// <summary>
        ///   Looks up a localized string similar to: 标签
        /// </summary>
        public static string Label
        {
            get
            {
                return _resourceLoader.GetString("Label");
            }
        }
        #endregion

        #region LabelEmptyPrompt
        /// <summary>
        ///   Looks up a localized string similar to: 标签不能为空
        /// </summary>
        public static string LabelEmptyPrompt
        {
            get
            {
                return _resourceLoader.GetString("LabelEmptyPrompt");
            }
        }
        #endregion

        #region LabelEmptyPromptDesc
        /// <summary>
        ///   Looks up a localized string similar to: 请在列表中选择一个或多个标签，也可以在下方新建标签🏷
        /// </summary>
        public static string LabelEmptyPromptDesc
        {
            get
            {
                return _resourceLoader.GetString("LabelEmptyPromptDesc");
            }
        }
        #endregion

        #region Language
        /// <summary>
        ///   Looks up a localized string similar to: 语言
        /// </summary>
        public static string Language
        {
            get
            {
                return _resourceLoader.GetString("Language");
            }
        }
        #endregion

        #region LevelStringError
        /// <summary>
        ///   Looks up a localized string similar to: 错误❌：
        /// </summary>
        public static string LevelStringError
        {
            get
            {
                return _resourceLoader.GetString("LevelStringError");
            }
        }
        #endregion

        #region LevelStringInfo
        /// <summary>
        ///   Looks up a localized string similar to: 提示ℹ：
        /// </summary>
        public static string LevelStringInfo
        {
            get
            {
                return _resourceLoader.GetString("LevelStringInfo");
            }
        }
        #endregion

        #region LevelStringWarning
        /// <summary>
        ///   Looks up a localized string similar to: 警告⚠：
        /// </summary>
        public static string LevelStringWarning
        {
            get
            {
                return _resourceLoader.GetString("LevelStringWarning");
            }
        }
        #endregion

        #region LoadingBackups
        /// <summary>
        ///   Looks up a localized string similar to: 正在读取备份文件
        /// </summary>
        public static string LoadingBackups
        {
            get
            {
                return _resourceLoader.GetString("LoadingBackups");
            }
        }
        #endregion

        #region ModifyItem
        /// <summary>
        ///   Looks up a localized string similar to: 修改项
        /// </summary>
        public static string ModifyItem
        {
            get
            {
                return _resourceLoader.GetString("ModifyItem");
            }
        }
        #endregion

        #region NewVersionDetectedTitle
        /// <summary>
        ///   Looks up a localized string similar to: 检测到新版本:{0}
        /// </summary>
        public static string NewVersionDetectedTitle(string version)
        {
            return string.Format(_resourceLoader.GetString("NewVersionDetectedTitle"), version);
        }
        #endregion

        #region NoAccessToSourceFolderToken
        /// <summary>
        ///   Looks up a localized string similar to: 无法访问源文件夹token
        /// </summary>
        public static string NoAccessToSourceFolderToken
        {
            get
            {
                return _resourceLoader.GetString("NoAccessToSourceFolderToken");
            }
        }
        #endregion

        #region NoAccessToTargetFolderToken
        /// <summary>
        ///   Looks up a localized string similar to: 无法访问目标文件夹token
        /// </summary>
        public static string NoAccessToTargetFolderToken
        {
            get
            {
                return _resourceLoader.GetString("NoAccessToTargetFolderToken");
            }
        }
        #endregion

        #region NoBackupTaskDesc
        /// <summary>
        ///   Looks up a localized string similar to: 请先添加备份任务
        /// </summary>
        public static string NoBackupTaskDesc
        {
            get
            {
                return _resourceLoader.GetString("NoBackupTaskDesc");
            }
        }
        #endregion

        #region NoBackupTaskSelectedDesc
        /// <summary>
        ///   Looks up a localized string similar to: 请先选中备份任务再进行备份
        /// </summary>
        public static string NoBackupTaskSelectedDesc
        {
            get
            {
                return _resourceLoader.GetString("NoBackupTaskSelectedDesc");
            }
        }
        #endregion

        #region NoBackupTaskSelectedTitle
        /// <summary>
        ///   Looks up a localized string similar to: 没有选中备份任务
        /// </summary>
        public static string NoBackupTaskSelectedTitle
        {
            get
            {
                return _resourceLoader.GetString("NoBackupTaskSelectedTitle");
            }
        }
        #endregion

        #region NoBackupTaskTitle
        /// <summary>
        ///   Looks up a localized string similar to: 没有备份任务
        /// </summary>
        public static string NoBackupTaskTitle
        {
            get
            {
                return _resourceLoader.GetString("NoBackupTaskTitle");
            }
        }
        #endregion

        #region NoContentDetectedDesc
        /// <summary>
        ///   Looks up a localized string similar to: 请填写有意义的文本内容
        /// </summary>
        public static string NoContentDetectedDesc
        {
            get
            {
                return _resourceLoader.GetString("NoContentDetectedDesc");
            }
        }
        #endregion

        #region NoContentDetectedTitle
        /// <summary>
        ///   Looks up a localized string similar to: 未检测到内容
        /// </summary>
        public static string NoContentDetectedTitle
        {
            get
            {
                return _resourceLoader.GetString("NoContentDetectedTitle");
            }
        }
        #endregion

        #region NoDescriptionIndicator
        /// <summary>
        ///   Looks up a localized string similar to: (无描述)
        /// </summary>
        public static string NoDescriptionIndicator
        {
            get
            {
                return _resourceLoader.GetString("NoDescriptionIndicator");
            }
        }
        #endregion

        #region NoFileChangeDetected
        /// <summary>
        ///   Looks up a localized string similar to: 没有检测到文件变更
        /// </summary>
        public static string NoFileChangeDetected
        {
            get
            {
                return _resourceLoader.GetString("NoFileChangeDetected");
            }
        }
        #endregion

        #region NoReminderIndicator
        /// <summary>
        ///   Looks up a localized string similar to: (无)
        /// </summary>
        public static string NoReminderIndicator
        {
            get
            {
                return _resourceLoader.GetString("NoReminderIndicator");
            }
        }
        #endregion

        #region NoSelectedItemPrompt
        /// <summary>
        ///   Looks up a localized string similar to: 没有选中项
        /// </summary>
        public static string NoSelectedItemPrompt
        {
            get
            {
                return _resourceLoader.GetString("NoSelectedItemPrompt");
            }
        }
        #endregion

        #region NoSelectedItemPromptDesc
        /// <summary>
        ///   Looks up a localized string similar to: 请在这个列表中选择一项进行修改🛠
        /// </summary>
        public static string NoSelectedItemPromptDesc
        {
            get
            {
                return _resourceLoader.GetString("NoSelectedItemPromptDesc");
            }
        }
        #endregion

        #region NoTaskIndicator
        /// <summary>
        ///   Looks up a localized string similar to: (无任务)
        /// </summary>
        public static string NoTaskIndicator
        {
            get
            {
                return _resourceLoader.GetString("NoTaskIndicator");
            }
        }
        #endregion

        #region NoTaskNow
        /// <summary>
        ///   Looks up a localized string similar to: 当前无任务
        /// </summary>
        public static string NoTaskNow
        {
            get
            {
                return _resourceLoader.GetString("NoTaskNow");
            }
        }
        #endregion

        #region NotifyBlockingInfo
        /// <summary>
        ///   Looks up a localized string similar to: {0}(空转次数{1}，共{2}ms)
        /// </summary>
        public static string NotifyBlockingInfo
        {
            get
            {
                return _resourceLoader.GetString("NotifyBlockingInfo");
            }
        }
        #endregion

        #region NotifyDeadlineWithinHours
        /// <summary>
        ///   Looks up a localized string similar to: deadline不足{0}小时
        /// </summary>
        public static string NotifyDeadlineWithinHours
        {
            get
            {
                return _resourceLoader.GetString("NotifyDeadlineWithinHours");
            }
        }
        #endregion

        #region NotifyDescInAddition
        /// <summary>
        ///   Looks up a localized string similar to: 此外，
        /// </summary>
        public static string NotifyDescInAddition
        {
            get
            {
                return _resourceLoader.GetString("NotifyDescInAddition");
            }
        }
        #endregion

        #region NotifyDueInHours
        /// <summary>
        ///   Looks up a localized string similar to: 将在{0}个小时内到期。
        /// </summary>
        public static string NotifyDueInHours
        {
            get
            {
                return _resourceLoader.GetString("NotifyDueInHours");
            }
        }
        #endregion

        #region NotifyMultipleDeadlineWithinHours
        /// <summary>
        ///   Looks up a localized string similar to: {0}项deadline不足{1}小时
        /// </summary>
        public static string NotifyMultipleDeadlineWithinHours
        {
            get
            {
                return _resourceLoader.GetString("NotifyMultipleDeadlineWithinHours");
            }
        }
        #endregion

        #region NotifyMultipleDueInHours
        /// <summary>
        ///   Looks up a localized string similar to: 等，共{0}项将在{1}个小时内到期。
        /// </summary>
        public static string NotifyMultipleDueInHours
        {
            get
            {
                return _resourceLoader.GetString("NotifyMultipleDueInHours");
            }
        }
        #endregion

        #region NotifyOutdatedItems
        /// <summary>
        ///   Looks up a localized string similar to: {0}项已过期，请尽快处理。
        /// </summary>
        public static string NotifyOutdatedItems
        {
            get
            {
                return _resourceLoader.GetString("NotifyOutdatedItems");
            }
        }
        #endregion

        #region NotifyReminderItemMeetDeadline
        /// <summary>
        ///   Looks up a localized string similar to: "{0}"即将到期，是否已经完成？
        /// </summary>
        public static string NotifyReminderItemMeetDeadline
        {
            get
            {
                return _resourceLoader.GetString("NotifyReminderItemMeetDeadline");
            }
        }
        #endregion

        #region NotSelectedIndicator
        /// <summary>
        ///   Looks up a localized string similar to: (未选择)
        /// </summary>
        public static string NotSelectedIndicator
        {
            get
            {
                return _resourceLoader.GetString("NotSelectedIndicator");
            }
        }
        #endregion

        #region OpenFile
        /// <summary>
        ///   Looks up a localized string similar to: 打开文件
        /// </summary>
        public static string OpenFile
        {
            get
            {
                return _resourceLoader.GetString("OpenFile");
            }
        }
        #endregion

        #region OpenFolder
        /// <summary>
        ///   Looks up a localized string similar to: 打开文件夹
        /// </summary>
        public static string OpenFolder
        {
            get
            {
                return _resourceLoader.GetString("OpenFolder");
            }
        }
        #endregion

        #region OperationCanceled
        /// <summary>
        ///   Looks up a localized string similar to: 操作取消
        /// </summary>
        public static string OperationCanceled
        {
            get
            {
                return _resourceLoader.GetString("OperationCanceled");
            }
        }
        #endregion

        #region OutOfDate
        /// <summary>
        ///   Looks up a localized string similar to: 已过期
        /// </summary>
        public static string OutOfDate
        {
            get
            {
                return _resourceLoader.GetString("OutOfDate");
            }
        }
        #endregion

        #region Override
        /// <summary>
        ///   Looks up a localized string similar to: 覆盖
        /// </summary>
        public static string Override
        {
            get
            {
                return _resourceLoader.GetString("Override");
            }
        }
        #endregion

        #region PastTargetTimePrompt
        /// <summary>
        ///   Looks up a localized string similar to: 目标时间已经过了
        /// </summary>
        public static string PastTargetTimePrompt
        {
            get
            {
                return _resourceLoader.GetString("PastTargetTimePrompt");
            }
        }
        #endregion

        #region PastTargetTimePromptDesc
        /// <summary>
        ///   Looks up a localized string similar to: 穿越不了捏😵
        /// </summary>
        public static string PastTargetTimePromptDesc
        {
            get
            {
                return _resourceLoader.GetString("PastTargetTimePromptDesc");
            }
        }
        #endregion

        #region PlainText
        /// <summary>
        ///   Looks up a localized string similar to: 纯文本
        /// </summary>
        public static string PlainText
        {
            get
            {
                return _resourceLoader.GetString("PlainText");
            }
        }
        #endregion

        #region Priority
        /// <summary>
        ///   Looks up a localized string similar to: 优先级
        /// </summary>
        public static string Priority
        {
            get
            {
                return _resourceLoader.GetString("Priority");
            }
        }
        #endregion

        #region PriorityHigh
        /// <summary>
        ///   Looks up a localized string similar to: 🔴(紧急)
        /// </summary>
        public static string PriorityHigh
        {
            get
            {
                return _resourceLoader.GetString("PriorityHigh");
            }
        }
        #endregion

        #region PriorityMiddle
        /// <summary>
        ///   Looks up a localized string similar to: 🟡(优先)
        /// </summary>
        public static string PriorityMiddle
        {
            get
            {
                return _resourceLoader.GetString("PriorityMiddle");
            }
        }
        #endregion

        #region Prompt
        /// <summary>
        ///   Looks up a localized string similar to: 提示
        /// </summary>
        public static string Prompt
        {
            get
            {
                return _resourceLoader.GetString("Prompt");
            }
        }
        #endregion

        #region QuickBackup
        /// <summary>
        ///   Looks up a localized string similar to: 快速备份
        /// </summary>
        public static string QuickBackup
        {
            get
            {
                return _resourceLoader.GetString("QuickBackup");
            }
        }
        #endregion

        #region Record
        /// <summary>
        ///   Looks up a localized string similar to: 记录
        /// </summary>
        public static string Record
        {
            get
            {
                return _resourceLoader.GetString("Record");
            }
        }
        #endregion

        #region Refresh
        /// <summary>
        ///   Looks up a localized string similar to: 刷新
        /// </summary>
        public static string Refresh
        {
            get
            {
                return _resourceLoader.GetString("Refresh");
            }
        }
        #endregion

        #region Reminder
        /// <summary>
        ///   Looks up a localized string similar to: 提醒
        /// </summary>
        public static string Reminder
        {
            get
            {
                return _resourceLoader.GetString("Reminder");
            }
        }
        #endregion

        #region ReminderDataBothDeleted
        /// <summary>
        ///   Looks up a localized string similar to: 🗑内部数据文件和外部数据文件均已清空
        /// </summary>
        public static string ReminderDataBothDeleted
        {
            get
            {
                return _resourceLoader.GetString("ReminderDataBothDeleted");
            }
        }
        #endregion

        #region ReminderDataDeleted
        /// <summary>
        ///   Looks up a localized string similar to: 🗑已删除内部数据文件
        /// </summary>
        public static string ReminderDataDeleted
        {
            get
            {
                return _resourceLoader.GetString("ReminderDataDeleted");
            }
        }
        #endregion

        #region ReminderDescription
        /// <summary>
        ///   Looks up a localized string similar to: 提醒描述
        /// </summary>
        public static string ReminderDescription
        {
            get
            {
                return _resourceLoader.GetString("ReminderDescription");
            }
        }
        #endregion

        #region ReminderDone
        /// <summary>
        ///   Looks up a localized string similar to: 已完成
        /// </summary>
        public static string ReminderDone
        {
            get
            {
                return _resourceLoader.GetString("ReminderDone");
            }
        }
        #endregion

        #region ReminderFolderImportCancel
        /// <summary>
        ///   Looks up a localized string similar to: 取消导入，路径：{0}
        /// </summary>
        public static string ReminderFolderImportCancel(string path)
        {
            return string.Format(_resourceLoader.GetString("ReminderFolderImportCancel"), path);
        }
        #endregion

        #region ReminderFoundBackupFileTitle
        /// <summary>
        ///   Looks up a localized string similar to: 找到"提醒"的数据备份
        /// </summary>
        public static string ReminderFoundBackupFileTitle
        {
            get
            {
                return _resourceLoader.GetString("ReminderFoundBackupFileTitle");
            }
        }
        #endregion

        #region ReminderFoundDataEmptyDesc
        /// <summary>
        ///   Looks up a localized string similar to: 找到数据文件，数据为空，但是找到数据备份。是否使用备份数据恢复？
        /// </summary>
        public static string ReminderFoundDataEmptyDesc
        {
            get
            {
                return _resourceLoader.GetString("ReminderFoundDataEmptyDesc");
            }
        }
        #endregion

        #region ReminderItemsManagement
        /// <summary>
        ///   Looks up a localized string similar to: 管理提醒项
        /// </summary>
        public static string ReminderItemsManagement
        {
            get
            {
                return _resourceLoader.GetString("ReminderItemsManagement");
            }
        }
        #endregion

        #region ReminderLoadFromJsonFile
        /// <summary>
        ///   Looks up a localized string similar to: 从JSON文件中读取数据
        /// </summary>
        public static string ReminderLoadFromJsonFile
        {
            get
            {
                return _resourceLoader.GetString("ReminderLoadFromJsonFile");
            }
        }
        #endregion

        #region ReminderNotFoundDataDesc
        /// <summary>
        ///   Looks up a localized string similar to: 目前无数据文件，但是找到数据备份。是否使用备份数据恢复？
        /// </summary>
        public static string ReminderNotFoundDataDesc
        {
            get
            {
                return _resourceLoader.GetString("ReminderNotFoundDataDesc");
            }
        }
        #endregion

        #region ReminderNotifyDescItemDueInADay
        /// <summary>
        ///   Looks up a localized string similar to: 将在1天之内到期，请留意。
        /// </summary>
        public static string ReminderNotifyDescItemDueInADay
        {
            get
            {
                return _resourceLoader.GetString("ReminderNotifyDescItemDueInADay");
            }
        }
        #endregion

        #region ReminderNotifyDescItemsDueInADay
        /// <summary>
        ///   Looks up a localized string similar to: 共{0}项将在1天之内到期。
        /// </summary>
        public static string ReminderNotifyDescItemsDueInADay
        {
            get
            {
                return _resourceLoader.GetString("ReminderNotifyDescItemsDueInADay");
            }
        }
        #endregion

        #region ReminderNotifyDescMultipleDueInADay
        /// <summary>
        ///   Looks up a localized string similar to: 有{0}项将在1天之内到期。
        /// </summary>
        public static string ReminderNotifyDescMultipleDueInADay
        {
            get
            {
                return _resourceLoader.GetString("ReminderNotifyDescMultipleDueInADay");
            }
        }
        #endregion

        #region ReminderNotifyDescRemaining
        /// <summary>
        ///   Looks up a localized string similar to: 剩余事项提醒
        /// </summary>
        public static string ReminderNotifyDescRemaining
        {
            get
            {
                return _resourceLoader.GetString("ReminderNotifyDescRemaining");
            }
        }
        #endregion

        #region ReminderNotifyTitle
        /// <summary>
        ///   Looks up a localized string similar to: 事项提醒
        /// </summary>
        public static string ReminderNotifyTitle
        {
            get
            {
                return _resourceLoader.GetString("ReminderNotifyTitle");
            }
        }
        #endregion

        #region ReminderOOBEWelcomeDesc
        /// <summary>
        ///   Looks up a localized string similar to: 现在来创建一些提醒项吧
        /// </summary>
        public static string ReminderOOBEWelcomeDesc
        {
            get
            {
                return _resourceLoader.GetString("ReminderOOBEWelcomeDesc");
            }
        }
        #endregion

        #region ReminderOOBEWelcomeTitle
        /// <summary>
        ///   Looks up a localized string similar to: 欢迎使用Reminder👋
        /// </summary>
        public static string ReminderOOBEWelcomeTitle
        {
            get
            {
                return _resourceLoader.GetString("ReminderOOBEWelcomeTitle");
            }
        }
        #endregion

        #region ReminderTodo
        /// <summary>
        ///   Looks up a localized string similar to: 待处理
        /// </summary>
        public static string ReminderTodo
        {
            get
            {
                return _resourceLoader.GetString("ReminderTodo");
            }
        }
        #endregion

        #region ReminderTodoItems
        /// <summary>
        ///   Looks up a localized string similar to: 待办事项
        /// </summary>
        public static string ReminderTodoItems
        {
            get
            {
                return _resourceLoader.GetString("ReminderTodoItems");
            }
        }
        #endregion

        #region RepositoryPage
        /// <summary>
        ///   Looks up a localized string similar to: 项目主页
        /// </summary>
        public static string RepositoryPage
        {
            get
            {
                return _resourceLoader.GetString("RepositoryPage");
            }
        }
        #endregion

        #region Reset
        /// <summary>
        ///   Looks up a localized string similar to: 重置
        /// </summary>
        public static string Reset
        {
            get
            {
                return _resourceLoader.GetString("Reset");
            }
        }
        #endregion

        #region Restore
        /// <summary>
        ///   Looks up a localized string similar to: 恢复
        /// </summary>
        public static string Restore
        {
            get
            {
                return _resourceLoader.GetString("Restore");
            }
        }
        #endregion

        #region Retain
        /// <summary>
        ///   Looks up a localized string similar to: 保留
        /// </summary>
        public static string Retain
        {
            get
            {
                return _resourceLoader.GetString("Retain");
            }
        }
        #endregion

        #region ReviseReminder
        /// <summary>
        ///   Looks up a localized string similar to: 修改提醒
        /// </summary>
        public static string ReviseReminder
        {
            get
            {
                return _resourceLoader.GetString("ReviseReminder");
            }
        }
        #endregion

        #region Save
        /// <summary>
        ///   Looks up a localized string similar to: 保存
        /// </summary>
        public static string Save
        {
            get
            {
                return _resourceLoader.GetString("Save");
            }
        }
        #endregion

        #region SaveData
        /// <summary>
        ///   Looks up a localized string similar to: 保存数据
        /// </summary>
        public static string SaveData
        {
            get
            {
                return _resourceLoader.GetString("SaveData");
            }
        }
        #endregion

        #region SaveList
        /// <summary>
        ///   Looks up a localized string similar to: 保存列表
        /// </summary>
        public static string SaveList
        {
            get
            {
                return _resourceLoader.GetString("SaveList");
            }
        }
        #endregion

        #region SaveReminderDataFile
        /// <summary>
        ///   Looks up a localized string similar to: 保存Reminder
        /// </summary>
        public static string SaveReminderDataFile
        {
            get
            {
                return _resourceLoader.GetString("SaveReminderDataFile");
            }
        }
        #endregion

        #region ScanningPaths
        /// <summary>
        ///   Looks up a localized string similar to: 正在扫描路径
        /// </summary>
        public static string ScanningPaths
        {
            get
            {
                return _resourceLoader.GetString("ScanningPaths");
            }
        }
        #endregion

        #region Search
        /// <summary>
        ///   Looks up a localized string similar to: 搜索
        /// </summary>
        public static string Search
        {
            get
            {
                return _resourceLoader.GetString("Search");
            }
        }
        #endregion

        #region SearchResultSortBy
        /// <summary>
        ///   Looks up a localized string similar to: 搜索排序
        /// </summary>
        public static string SearchResultSortBy
        {
            get
            {
                return _resourceLoader.GetString("SearchResultSortBy");
            }
        }
        #endregion

        #region SelectedFolder
        /// <summary>
        ///   Looks up a localized string similar to: 选取的文件夹:
        /// </summary>
        public static string SelectedFolder
        {
            get
            {
                return _resourceLoader.GetString("SelectedFolder");
            }
        }
        #endregion

        #region SelectTag
        /// <summary>
        ///   Looks up a localized string similar to: 选择标签
        /// </summary>
        public static string SelectTag
        {
            get
            {
                return _resourceLoader.GetString("SelectTag");
            }
        }
        #endregion

        #region Settings
        /// <summary>
        ///   Looks up a localized string similar to: 设置
        /// </summary>
        public static string Settings
        {
            get
            {
                return _resourceLoader.GetString("Settings");
            }
        }
        #endregion

        #region ShowDetailedInfo
        /// <summary>
        ///   Looks up a localized string similar to: 显示详细信息
        /// </summary>
        public static string ShowDetailedInfo
        {
            get
            {
                return _resourceLoader.GetString("ShowDetailedInfo");
            }
        }
        #endregion

        #region ShowNotificationBlockingInfo
        /// <summary>
        ///   Looks up a localized string similar to: 显示通知显示阻塞信息
        /// </summary>
        public static string ShowNotificationBlockingInfo
        {
            get
            {
                return _resourceLoader.GetString("ShowNotificationBlockingInfo");
            }
        }
        #endregion

        #region SidebarOpenState
        /// <summary>
        ///   Looks up a localized string similar to: 侧边栏默认开启状态
        /// </summary>
        public static string SidebarOpenState
        {
            get
            {
                return _resourceLoader.GetString("SidebarOpenState");
            }
        }
        #endregion

        #region SkipCheckUpdateOfVersion
        /// <summary>
        ///   Looks up a localized string similar to: {0}不提示版本{1}的更新
        /// </summary>
        public static string SkipCheckUpdateOfVersion(string hintString, string CheckUpdate)
        {
            return string.Format(_resourceLoader.GetString("SkipCheckUpdateOfVersion"), hintString, CheckUpdate);
        }
        #endregion

        #region SkipThisVersion
        /// <summary>
        ///   Looks up a localized string similar to: 不再提示此版本
        /// </summary>
        public static string SkipThisVersion
        {
            get
            {
                return _resourceLoader.GetString("SkipThisVersion");
            }
        }
        #endregion

        #region StartBackup
        /// <summary>
        ///   Looks up a localized string similar to: 开始备份
        /// </summary>
        public static string StartBackup
        {
            get
            {
                return _resourceLoader.GetString("StartBackup");
            }
        }
        #endregion

        #region Sync
        /// <summary>
        ///   Looks up a localized string similar to: 同步
        /// </summary>
        public static string Sync
        {
            get
            {
                return _resourceLoader.GetString("Sync");
            }
        }
        #endregion

        #region SyncByWindowsOS
        /// <summary>
        ///   Looks up a localized string similar to: 应用设置跨设备同步
        /// </summary>
        public static string SyncByWindowsOS
        {
            get
            {
                return _resourceLoader.GetString("SyncByWindowsOS");
            }
        }
        #endregion

        #region Tag
        /// <summary>
        ///   Looks up a localized string similar to: 标签
        /// </summary>
        public static string Tag
        {
            get
            {
                return _resourceLoader.GetString("Tag");
            }
        }
        #endregion

        #region TaskClearIndicator
        /// <summary>
        ///   Looks up a localized string similar to: 所有任务已完成☕
        /// </summary>
        public static string TaskClearIndicator
        {
            get
            {
                return _resourceLoader.GetString("TaskClearIndicator");
            }
        }
        #endregion

        #region TaskList
        /// <summary>
        ///   Looks up a localized string similar to: 任务列表
        /// </summary>
        public static string TaskList
        {
            get
            {
                return _resourceLoader.GetString("TaskList");
            }
        }
        #endregion

        #region TaskPage
        /// <summary>
        ///   Looks up a localized string similar to: 任务面板
        /// </summary>
        public static string TaskPage
        {
            get
            {
                return _resourceLoader.GetString("TaskPage");
            }
        }
        #endregion

        #region TimeEmptyPrompt
        /// <summary>
        ///   Looks up a localized string similar to: 时间不能为空
        /// </summary>
        public static string TimeEmptyPrompt
        {
            get
            {
                return _resourceLoader.GetString("TimeEmptyPrompt");
            }
        }
        #endregion

        #region TimeEmptyPromptDesc
        /// <summary>
        ///   Looks up a localized string similar to: 还是选一个ddl时间吧⏰
        /// </summary>
        public static string TimeEmptyPromptDesc
        {
            get
            {
                return _resourceLoader.GetString("TimeEmptyPromptDesc");
            }
        }
        #endregion

        #region TimeSelectorSpan
        /// <summary>
        ///   Looks up a localized string similar to: 时间选择器选项间隔（分钟）
        /// </summary>
        public static string TimeSelectorSpan
        {
            get
            {
                return _resourceLoader.GetString("TimeSelectorSpan");
            }
        }
        #endregion

        #region Title
        /// <summary>
        ///   Looks up a localized string similar to: 标题
        /// </summary>
        public static string Title
        {
            get
            {
                return _resourceLoader.GetString("Title");
            }
        }
        #endregion

        #region TitleEmptyPrompt
        /// <summary>
        ///   Looks up a localized string similar to: 标题不能为空
        /// </summary>
        public static string TitleEmptyPrompt
        {
            get
            {
                return _resourceLoader.GetString("TitleEmptyPrompt");
            }
        }
        #endregion

        #region TitleEmptyPromptDesc
        /// <summary>
        ///   Looks up a localized string similar to: 不然就找不到这个项目了😥
        /// </summary>
        public static string TitleEmptyPromptDesc
        {
            get
            {
                return _resourceLoader.GetString("TitleEmptyPromptDesc");
            }
        }
        #endregion

        #region Today
        /// <summary>
        ///   Looks up a localized string similar to: 今天
        /// </summary>
        public static string Today
        {
            get
            {
                return _resourceLoader.GetString("Today");
            }
        }
        #endregion

        #region Tomorrow
        /// <summary>
        ///   Looks up a localized string similar to: 明天
        /// </summary>
        public static string Tomorrow
        {
            get
            {
                return _resourceLoader.GetString("Tomorrow");
            }
        }
        #endregion

        #region TotalBackup
        /// <summary>
        ///   Looks up a localized string similar to: 完整备份
        /// </summary>
        public static string TotalBackup
        {
            get
            {
                return _resourceLoader.GetString("TotalBackup");
            }
        }
        #endregion

        #region TwoDaysLater
        /// <summary>
        ///   Looks up a localized string similar to: 后天
        /// </summary>
        public static string TwoDaysLater
        {
            get
            {
                return _resourceLoader.GetString("TwoDaysLater");
            }
        }
        #endregion

        #region UnableToLoadFolder
        /// <summary>
        ///   Looks up a localized string similar to: 未能读取文件夹 {0}
        /// </summary>
        public static string UnableToLoadFolder(string path)
        {
            return string.Format(_resourceLoader.GetString("UnableToLoadFolder"), path);
        }
        #endregion

        #region UnknownBackupState
        /// <summary>
        ///   Looks up a localized string similar to: 状态未知
        /// </summary>
        public static string UnknownBackupState
        {
            get
            {
                return _resourceLoader.GetString("UnknownBackupState");
            }
        }
        #endregion

        #region Unselected
        /// <summary>
        ///   Looks up a localized string similar to: 暂未选中
        /// </summary>
        public static string Unselected
        {
            get
            {
                return _resourceLoader.GetString("Unselected");
            }
        }
        #endregion

        #region Update
        /// <summary>
        ///   Looks up a localized string similar to: 更新
        /// </summary>
        public static string Update
        {
            get
            {
                return _resourceLoader.GetString("Update");
            }
        }
        #endregion

        #region UpdateStatusTitle
        /// <summary>
        ///   Looks up a localized string similar to: 检测更新状态：
        /// </summary>
        public static string UpdateStatusTitle
        {
            get
            {
                return _resourceLoader.GetString("UpdateStatusTitle");
            }
        }
        #endregion

        #region Version
        /// <summary>
        ///   Looks up a localized string similar to: 版本
        /// </summary>
        public static string Version
        {
            get
            {
                return _resourceLoader.GetString("Version");
            }
        }
        #endregion
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("DotNetPlus.ReswPlus", "2.1.3")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    public class ResourcesExtension: MarkupExtension
    {
        public enum KeyEnum
        {
            __Undefined = 0,
            About,
            Add,
            AddItem,
            Application,
            ApplicationFolder,
            AutoDetectUpdateTitle,
            BackgroundTask,
            BackgroundWakeSpan,
            BackupDone,
            BackupInfoTitle,
            Backuping,
            BackupPath,
            BackupPathExistDesc,
            BackupProcessing,
            BackupRestoring,
            BackupState,
            BackupTask,
            BackupTaskSeq,
            BackupTaskSequenceDesc,
            BackupTitle,
            BasicInfo,
            CalculatingHash,
            Cancel,
            CheckUpdateWhenStartup,
            Clear,
            ClearBackupDataConfirmDesc,
            ClearBackupDataTitle,
            ClearDataListDesc,
            ClearDataListTitle,
            ClearExternalReminderDataDesc,
            ClearExternalReminderDataTitle,
            ClearReminderData,
            ClearReminderDataDialogDesc,
            ClearReminderDataDialogTitle,
            Close,
            ComparingInfo,
            ComparingToFullBackup,
            ComponentSettings,
            Confirm,
            ControlSettings,
            CreatedTime,
            DailyNotification,
            DailyNotificationTime,
            DataSavePath,
            DateEmptyPrompt,
            DateEmptyPromptDesc,
            DeadlineDate,
            DeadlineTime,
            DebugOption,
            Delete,
            DetailedInfo,
            Developer,
            DoNotCheckUpdate,
            DownloadNewVersion,
            DragJsonFileHere,
            DueTime,
            EmptyIndicator,
            EnableNotification,
            EncounterProblem,
            Error,
            ErrorInformation,
            ErrorMessageUnableSaveTo,
            ExportToDownloadFolder,
            ExternalReminderDataAccessible,
            ExternalReminderFolderDataDetectedDesc,
            ExternalReminderFolderDataDetectedTitle,
            ExternalReminderFolderUnavailable,
            ExternalReminderFolderUnspecified,
            FeedbackOnGithub,
            FileNotExist,
            Finished,
            GetBackupPathFailed,
            GetSavePathFailed,
            GoAhead,
            Import,
            ImportBackup,
            ImportFailed,
            ImportReminderJson,
            ImportSuccessful,
            InstallDate,
            JsonOutputOnly,
            Label,
            LabelEmptyPrompt,
            LabelEmptyPromptDesc,
            Language,
            LevelStringError,
            LevelStringInfo,
            LevelStringWarning,
            LoadingBackups,
            ModifyItem,
            NewVersionDetectedTitle,
            NoAccessToSourceFolderToken,
            NoAccessToTargetFolderToken,
            NoBackupTaskDesc,
            NoBackupTaskSelectedDesc,
            NoBackupTaskSelectedTitle,
            NoBackupTaskTitle,
            NoContentDetectedDesc,
            NoContentDetectedTitle,
            NoDescriptionIndicator,
            NoFileChangeDetected,
            NoReminderIndicator,
            NoSelectedItemPrompt,
            NoSelectedItemPromptDesc,
            NoTaskIndicator,
            NoTaskNow,
            NotifyBlockingInfo,
            NotifyDeadlineWithinHours,
            NotifyDescInAddition,
            NotifyDueInHours,
            NotifyMultipleDeadlineWithinHours,
            NotifyMultipleDueInHours,
            NotifyOutdatedItems,
            NotifyReminderItemMeetDeadline,
            NotSelectedIndicator,
            OpenFile,
            OpenFolder,
            OperationCanceled,
            OutOfDate,
            Override,
            PastTargetTimePrompt,
            PastTargetTimePromptDesc,
            PlainText,
            Priority,
            PriorityHigh,
            PriorityMiddle,
            Prompt,
            QuickBackup,
            Record,
            Refresh,
            Reminder,
            ReminderDataBothDeleted,
            ReminderDataDeleted,
            ReminderDescription,
            ReminderDone,
            ReminderFolderImportCancel,
            ReminderFoundBackupFileTitle,
            ReminderFoundDataEmptyDesc,
            ReminderItemsManagement,
            ReminderLoadFromJsonFile,
            ReminderNotFoundDataDesc,
            ReminderNotifyDescItemDueInADay,
            ReminderNotifyDescItemsDueInADay,
            ReminderNotifyDescMultipleDueInADay,
            ReminderNotifyDescRemaining,
            ReminderNotifyTitle,
            ReminderOOBEWelcomeDesc,
            ReminderOOBEWelcomeTitle,
            ReminderTodo,
            ReminderTodoItems,
            RepositoryPage,
            Reset,
            Restore,
            Retain,
            ReviseReminder,
            Save,
            SaveData,
            SaveList,
            SaveReminderDataFile,
            ScanningPaths,
            Search,
            SearchResultSortBy,
            SelectedFolder,
            SelectTag,
            Settings,
            ShowDetailedInfo,
            ShowNotificationBlockingInfo,
            SidebarOpenState,
            SkipCheckUpdateOfVersion,
            SkipThisVersion,
            StartBackup,
            Sync,
            SyncByWindowsOS,
            Tag,
            TaskClearIndicator,
            TaskList,
            TaskPage,
            TimeEmptyPrompt,
            TimeEmptyPromptDesc,
            TimeSelectorSpan,
            Title,
            TitleEmptyPrompt,
            TitleEmptyPromptDesc,
            Today,
            Tomorrow,
            TotalBackup,
            TwoDaysLater,
            UnableToLoadFolder,
            UnknownBackupState,
            Unselected,
            Update,
            UpdateStatusTitle,
            Version,
        }

        private static ResourceLoader _resourceLoader;
        static ResourcesExtension()
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");
        }
        public KeyEnum Key { get; set;}
        public IValueConverter Converter { get; set;}
        public object ConverterParameter { get; set;}
        protected override object ProvideValue()
        {
            string res;
            if(Key == KeyEnum.__Undefined)
            {
                res = "";
            }
            else
            {
                res = _resourceLoader.GetString(Key.ToString());
            }
            return Converter == null ? res : Converter.Convert(res, typeof(String), ConverterParameter, null);
        }
    }
} //Chamberlain_UWP.Strings
