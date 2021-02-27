using System;
using System.Collections.Generic;
using System.Text;

namespace WMSApp.Models.Setting
{
    public enum SettingViewEnum
    {
        GrPriceListSetupView,
        GiPriceListSetupView,
        AppUserGroupView,
        AppUserView,
        AppLicenseView,
        AppLGI_DocSeries,
        AppLGR_DocSeries,
    }

    public class SettingMenuItem
    {
        public string Text { get; set; }
        public string Details { get; set; }
        public SettingViewEnum ViewId { get; set; }
    }
}
