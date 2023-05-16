using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Chamberlain_UWP.Reminder;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Chamberlain_UWP.Settings;
using Windows.Storage.AccessCache;
using Windows.ApplicationModel;
using System.Windows.Input;
using System.Reflection.Context;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Chamberlain_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) => vm.OnNavigatedTo();

        //ViewModel
        SettingsPageViewModel vm = new SettingsPageViewModel();
    }
}