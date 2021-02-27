using Android.Content.PM;
using System;
using WMSApp.Droid;
using WMSApp.Interface;
using Xamarin.Forms;

[assembly: Dependency(typeof(IAppVersionDroid))]
namespace WMSApp.Droid
{
    class IAppVersionDroid : IAppVersion
    {

        PackageInfo _appInfo;
        public IAppVersionDroid()
        {
            var context = Android.App.Application.Context;
            _appInfo = context.PackageManager.GetPackageInfo(context.PackageName, 0);
        }

        [Obsolete]
        public string Version
        {
            get
            {
                return _appInfo.VersionName + "." + _appInfo.VersionCode.ToString();
            }
        }
    }
}