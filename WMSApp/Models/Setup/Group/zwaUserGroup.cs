using Newtonsoft.Json;
using System;
namespace WMSApp.Models.Setup.Group
{
    public class zwaUserGroup
    {
        public int groupId { get; set; }
        public string companyId { get; set; }
        public string groupName { get; set; }
        public string groupDesc { get; set; }
        public DateTime lastModiDate { get; set; }
        public string lastModiUser { get; set; }

        [JsonIgnore]
        public string TextDisplay
        {
            get
            {
                return groupName;
            }
        }

        public string DetailsDisplay
        {
            get
            {
                string details = groupName;
                details += " . " + lastModiDate.ToString("dd/MM/yy");
                return details;
            }
        }
    }
}
