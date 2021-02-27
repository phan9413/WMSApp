using System;
using System.Collections.Generic;
using System.Text;

namespace WMSApp.Class
{
    public class BearerToken // Handle and store the bearer token from the web server
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }
}
