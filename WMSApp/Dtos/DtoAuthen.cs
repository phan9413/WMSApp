using WMSApp.Class;
using WMSApp.Models.Setup.Group;

namespace WMSApp.Dtos
{
    public class DtoAuthen
    {
        public string sap_logon_name { get; set; }
        public string currentUser { get; set; }
        public BearerToken bearerToken { get; set; }
        public zwaUserGroup currentGroup { get; set; }
        public zwaUserGroup1[] currentPermissions { get; set; }
    }
}
