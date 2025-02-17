﻿using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace WMSApp.Models.Login
{
    public class zwaUser : INotifyPropertyChanged
    {
        public int sysId { get; set; } // generated by sql database
        public string companyId { get; set; } // get detaild from web api
        public string userIdName { get; set; } // enter by admini
        public string password { get; set; } // enter by admin
        public string sapId { get; set; } // generated by DIAPI in middleware
        public string displayName { get; set; } // enter by the admin
        public DateTime lastModiDate { get; set; } // system update
        public string lastModiUser { get; set; } // system update
        public string locked { get; set; } // enter by admin
        public int Roles { get; set; }  // get details from web api
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string RoleDesc { get; set; }
        public int groupId { get; set; }
        public string groupName { get; set; }

        // Added by johnny, 20200421T1036 for create and check / ignore the creation of the ERP user
        public int createERPSalesEmp { get; set; }


        // for group user selection check box 20200327T2255
        bool _isSelected;
        [JsonIgnore]
        public bool isSelected
        {
            get => _isSelected; 
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("isSelected");
                }
            }
        }

        [JsonIgnore]
        public bool isVisible { get; set; } = true;

        [JsonIgnore]
        public string TextDisplay
        {
            get
            {
                return userIdName + " . " + displayName + " . " + GetLockedDisplay();
            }
        }

        [JsonIgnore]
        public string DetailsDisplay
        {
            get
            {
                string displayStr = (RoleDesc != null && RoleDesc.Length > 0) ? RoleDesc + " . " : "";
                displayStr += (groupName != null) ? groupName + " . " : "";
                displayStr += (lastModiDate != null) ? "Last modified : " + lastModiDate.ToString("dd/MM/yy") : "";


                return displayStr;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Handle the on property changed, value update to screen
        /// </summary>
        /// <param name="propertyname"></param>
        public void OnPropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        string GetLockedDisplay()
        {
            return (locked.ToLower().Equals("y")) ? "Locked" : "Active";
        }
    }
}
