﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Media.Protection.PlayReady;
using Windows.UI.Xaml.Controls;

namespace Chamberlain_UWP.Settings
{
    internal class Release
    {
        [JsonPropertyName("tag_name")]
        public string Tag { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("body")]
        public string Message { get; set; }
    }

    internal class ReleaseManager
    {
        private readonly HttpClient client = new HttpClient();
        internal async Task<Release> GetLastest() //风险性方法，考虑try
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Chamberlain Updater");

            var streamTask = client.GetStreamAsync("https://api.github.com/repos/huuhghhgyg/Chamberlain_UWP/releases/latest");
            var release = await JsonSerializer.DeserializeAsync<Release>(await streamTask);
            return release;
        }

        /// <summary>
        /// 移除Release返回的描述中阻碍阅读的markdown符号
        /// </summary>
        /// <param name="msg">Release返回的描述</param>
        /// <returns></returns>
        internal string RemoveMdSymbol(string msg)
        {
            StringBuilder sb = new StringBuilder(msg);
            sb.Replace("## ", ""); //移除二级标题标记
            sb.Replace("# ", ""); //移除一级标题标记
            sb.Replace("*", ""); //移除重点标记

            return sb.ToString();
        }

        /// <summary>
        /// 与在线版本比较, true：本地版本最新；false：在线版本最新（默认4位版本号）
        /// </summary>
        /// <param name="release"></param>
        /// <returns>true：本地版本最新；false：在线版本最新</returns>
        internal bool VersionCompare(Release release)
        {
            PackageVersion packageVersion = Package.Current.Id.Version;
            int[] local = { packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision }; //本地版本序列

            string version = release.Tag.Trim('v'); //将release的tag改为可识别的版本号(去除标签前的v)

            int len = version.Split('.').Count(); //版本号长度
            if (len < 4) for (int i = 0; i < 4 - len; i++) version += ".0"; //补齐版本号

            string[] online = version.Split('.'); //在线版本序列

            for (int i = 0; i < 4; i++)
            {
                if (local[i] < int.Parse(online[i])) //本地版本小
                {
                    return false; //检测到在线版本更新，立即返回
                }
                else if (local[i] > int.Parse(online[i])) return true; //本地版本更高，直接返回true
            }

            return true; //本地为最新版本
        }
    }
}
