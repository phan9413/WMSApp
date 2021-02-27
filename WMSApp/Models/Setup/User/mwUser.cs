using System;
namespace WMSApp.Models.Setup.User
{
    public class mwUser
    {
        public int id { get; set; }
        public string salesCode { get; set; }
        public string salesPersonName { get; set; }
        public string password { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string assigned_token { get; set; }
        public DateTime lastLogon { get; set; }
    }
}
