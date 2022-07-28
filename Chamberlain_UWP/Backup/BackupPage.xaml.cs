using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Chamberlain_UWP.Backup
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BackupPage : Page
    {
        public BackupPage()
        {
            this.InitializeComponent();
        }
        public BackupPageViewModel ViewModel { get => BackupData.ViewModel; }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!BackupData.Loaded)
            {
                //读取数据
                Task loadDataFromJson = Task.Run(async () => await ViewModel.Manager.LoadData()); //等待异步方法
                loadDataFromJson.Wait();
                ViewModel.RefershData();
            }
        }
    }

    public static class BackupData
    {
        public static bool Loaded = false;
        public static BackupPageViewModel ViewModel = new BackupPageViewModel();
    }
}
