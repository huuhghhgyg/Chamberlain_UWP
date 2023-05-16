using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chamberlain_UWP.Settings
{
    internal static class ResourceLoader
    {
        public static string GetString(string value) => Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView().GetString(value);
    }
}
