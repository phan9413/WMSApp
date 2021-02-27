namespace WMSApp.Models.Login
{
    /// <summary>
    /// Use in asp net core token request
    /// 20200506T1212
    /// </summary>
    public class AuthenticationUser
    {
        public string userIdName { get; set; }
        public string password { get; set; }
        public string companyId { get; set; }

    }
    public class zwaUserModel
    {
        public string userIdName { get; set; }
        public string password { get; set; }
        public string companyId { get; set; }
    }
}
