using System;
namespace WMSApp.Models.Setup.Group
{
    public class zwaUserGroup1
    {
        public int id { get; set; }
        public int screenId { get; set; }
        public int groupId { get; set; }
        public string companyId { get; set; }
        public int parentId { get; set; }
        public string title { get; set; }
        public string dscrptn { get; set; }
        public string authorised { get; set; }
        public DateTime lastModiDate { get; set; }
        public string lastModiUser { get; set; }
        public int isFunctionCtrl { get; set; }
        public int isCompulsory { get; set; }
        public decimal ctrlLimit { get; set; } // 20200423T1223 for specify the control limit of the permission (if any), johnny
        public string appName { get; set; } // 20200507T1900 Add in to differential the app name permission
    }
}
