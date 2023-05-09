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

        #region NotSelectedIndicator
        /// <summary>
        ///   Looks up a localized string similar to: （未选择）
        /// </summary>
        public static string NotSelectedIndicator
        {
            get
            {
                return _resourceLoader.GetString("NotSelectedIndicator");
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
            DateEmptyPrompt,
            DateEmptyPromptDesc,
            EmptyIndicator,
            LabelEmptyPrompt,
            LabelEmptyPromptDesc,
            NoDescriptionIndicator,
            NoSelectedItemPrompt,
            NoSelectedItemPromptDesc,
            NotSelectedIndicator,
            PastTargetTimePrompt,
            PastTargetTimePromptDesc,
            TimeEmptyPrompt,
            TimeEmptyPromptDesc,
            TitleEmptyPrompt,
            TitleEmptyPromptDesc,
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
