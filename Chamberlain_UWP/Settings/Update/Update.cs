using Chamberlain_UWP.Backup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Chamberlain_UWP.Settings.Update
{
    internal class Update
    {
        internal static async void CheckUpdate() //检查更新
        {
            if (SettingsConfig.CheckUpdate != "false") //允许检查更新
            {
                Release release;
                ReleaseManager manager = new ReleaseManager();
                try
                {
                    release = await manager.GetLastest();
                    if (release.Tag != SettingsConfig.CheckUpdate && !manager.VersionCompare(release))
                    {
                        //有更新版本，弹窗提示
                        ContentDialog updateDialog = new ContentDialog
                        {
                            Title = $"检测到新版本:{release.Name}",
                            Content = manager.RemoveMdSymbol(release.Message),
                            PrimaryButtonText = "前往下载",
                            SecondaryButtonText = "不再提示此版本",
                            CloseButtonText = "关闭",
                            DefaultButton = ContentDialogButton.Primary
                        };
                        ContentDialogResult result = await updateDialog.ShowAsync();

                        if (result == ContentDialogResult.Primary)
                        {
                            //访问release页
                            var releaseUri = new Uri(@"https://github.com/huuhghhgyg/Chamberlain_UWP/releases");
                            await Windows.System.Launcher.LaunchUriAsync(releaseUri);
                        }
                        else if (result == ContentDialogResult.Secondary)
                        {
                            //不提示指定版本
                            SettingsConfig.CheckUpdate = release.Tag;
                        }
                    }
                }
                catch { } //忽略网络报错
            }
        }
    }
}
