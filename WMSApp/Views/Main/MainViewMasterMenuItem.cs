using System;
namespace WMSApp.Views.Main
{    
    public class MainViewMasterMenuItem
    {
        public MainViewMasterMenuItem()
        {
            TargetType = typeof(MainViewMasterMenuItem);
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public Type TargetType { get; set; }
    }
}