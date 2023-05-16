using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Storage;
using Windows.System.UserProfile;

namespace Chamberlain_UWP.Settings
{
    internal static class LanguageHelper
    {
        static string currentLanguage = "auto";

        /// <summary>
        /// 当前设置的语言（包括auto），不接受设置为不支持的语言
        /// </summary>
        public static string CurrentLanguage
        {
            get => currentLanguage;
            set
            {
                currentLanguage = value;
                SetLanguage(value);
            }
        }

        /// <summary>
        /// 当前显示的语言
        /// </summary>
        public static string DisplayLanguage
        {
            get
            {
                if (CurrentLanguage == "auto") return GetSystemLanguage();
                else return CurrentLanguage;
            }
        }

        /// <summary>
        /// 有效项为第一项
        /// </summary>
        public static readonly IList<string> LangCode = new List<string>()
        {
            "zh-cn zh zh-Hans zh-hans-cn zh-sg zh-hans-sg",
            "en-us en en-au en-ca en-gb en-ie en-in en-nz en-sg en-za en-bz en-hk en-id en-jm en-kz en-mt en-my en-ph en-pk en-tt en-vn en-zw en-053 en-021 en-029 en-011 en-018 en-014",
        };

        /// <summary>
        /// 语言列表，用于显示。语言顺序要同 LangCode
        /// </summary>
        public static readonly IList<string> SupportLang = new List<string>()
        {
            "zh-cn 中文","en English","auto"
        };

        /// <summary>
        /// 获取目前软件兼容的系统显示语言。如果没有则返回en-us
        /// </summary>
        /// <returns>目前软件正在显示的的语言</returns>
        public static string GetSystemLanguage()
        {
            var languages = GlobalizationPreferences.Languages;

            //如果用户有语言设置，则根据用户设置设置语言
            if (languages.Count > 0)
            {
                for (int i = 0; i < LangCode.Count; i++)
                {
                    if (LangCode[i].ToLower().Contains(languages[0].ToLower()))
                    {
                        string temp = LangCode[i].ToLower();
                        string[] tempArr = temp.Split(' ');

                        return tempArr[0];
                    }
                }
            }

            //如果没找到，则返回英语
            return "en-us";
        }

        /// <summary>
        /// 获取语言的标准形式，如果没找到则返回en-us
        /// </summary>
        /// <param name="lang">语言代码</param>
        static string GetStandardLanguage(string lang)
        {
            for (int i = 0; i < LangCode.Count; i++)
            {
                if (LangCode[i].ToLower().Contains(lang.ToLower()))
                {
                    string temp = LangCode[i].ToLower();
                    string[] tempArr = temp.Split(' ');

                    return tempArr[0]; //返回标准形式
                }
            }
            return "en-us"; //没找到，返回en-us
        }

        /// <summary>
        /// 设置语言，不接受不支持的语言。可以设为auto，如果参数为空，则会自动读取设置。
        /// </summary>
        /// <param name="lang"></param>
        public static void SetLanguage(string lang = null)
        {
            //空参数判断
            if (string.IsNullOrEmpty(lang))
            {
                lang = SettingsConfig.Language ?? "auto";
            }

            //没有设置语言或自动检测语言
            if (lang == "auto")
            {
                currentLanguage = "auto";
                ApplicationLanguages.PrimaryLanguageOverride = GetSystemLanguage();
            }
            else
            {
                //设置了特定语言
                currentLanguage = GetStandardLanguage(lang); //设置内部变量为标准语言形式
                ApplicationLanguages.PrimaryLanguageOverride = currentLanguage;
            }

            //保存设置
            SettingsConfig.Language = currentLanguage;
        }
    }
}
