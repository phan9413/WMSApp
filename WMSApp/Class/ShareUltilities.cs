using System;
using System.IO;
namespace WMSApp.Class
{
    public class ShareUltilities
    {
        public readonly string QUERY_VERIFIED_SAPUSER = @"mobile_ws/m";
        public readonly string QUERY_FILEUPLOAD = @"mobile_fu/m/j";
        public readonly string QUERY_APPSETUP = @"mobile_ws/cs"; // 20200320
        public readonly string QUERY_APPSETUP_USR = @"mobile_ws/us"; // 20200324
        public readonly string QUERY_APPSETUP_GRP = @"mobile_ws/gs"; // 20200326
        public readonly string QUERY_APPSYN = @"mobile_ws/sync"; // 20200411T2320
        public readonly string APP_JSON = "application/json";
        public readonly string UPDATE_RESP_MONITOR = @"ResponseMonitor"; // 20210122T1223

        public readonly string QUERY_GPRO  = "";

        //public string WEBAPI_HOST = "http://ftsap.com:42330/";// 20200110 use hosted server at puchong office 
        //                                                      //public string WEBAPI_HOST = "http://192.168.0.16:42330/"; // fastrack puchong office, local laptop  

        public string WEBAPI_HOST = "http://192.168.137.1:20719/";

        readonly string defaultWEBAPI = "http://216.81.183.227:20719/"; //"http://192.168.137.1:20719/";

        public string lastErrDesc { get; set; } ="";

        /// <summary>
        /// Constructor to call load App web api configuration from text file
        /// </summary>
        public ShareUltilities() // <-- trigger every time a class being invoke
        {
            LoadTextFile();
        }

        /// <summary>
        /// Load the last saved web api server address
        /// from inner app storage @ mobile phone
        /// </summary>
        void LoadTextFile()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string filename = Path.Combine(path, "spserver.txt");
                if (File.Exists(filename)) //<--- if file exist then delete the file
                {
                    using (var streamReader = new StreamReader(filename))
                    {
                        string content = streamReader.ReadToEnd();
                        WEBAPI_HOST = content.Trim(); // read into the app for stand by link 
                    }
                }
                else
                {
                    WEBAPI_HOST = defaultWEBAPI;
                   // WEBAPI_HOST = "http://ftsap.com:42330/";
                }
            }
            catch (Exception excep)
            {
                lastErrDesc = excep.ToString();
            }
        }

    }
}
