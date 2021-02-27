using System;
using System.ComponentModel;
using WMSApp.Class;
using WMSApp.Models.Setup.Group;
using Xamarin.Forms;
namespace WMSApp.Models.Setting
{
    public enum EnumPermissionFieldId
    {
        SalesPriceListSelection = 23,
        SalesPriceEdit = 24,
        SalesDiscountByValue = 25,
        SalesDiscountByPercent = 26
    }

    public enum MenuItemType
    {
        Quotation = 3,
        Order = 4,
        Delivery = 5,
        Return = 6,
        Invoice = 7,
        CreditMemo = 8,
        BusinessPartnerMaster = 10,
        BpNewLead = 11,
        IncomingPayments = 13,
        ItemMaster = 15,
        Setup = 17,
        GPS_tracking = 18,
        Synchonization_Db = 21,
        VersionLicense = 22,
        Logout,
        defaultVal,
    }

    /// <summary>
    /// Definition of the item for grouping later
    /// </summary>
    public class Item : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int ItemId { get; set; }
        public string Title { get; set; }
        string desc;
        public string Desc
        {
            get
            {
                return desc;
            }
            set
            {
                if (desc != value)
                {
                    desc = value;
                    OnPropertyChanged(nameof(Desc));
                }
            }
        }

        public MenuItemType Id { get; set; }

        // for cater the item used with switch
        bool _isSwitchOn;
        public bool isSwitchOn
        {
            get
            {
                return _isSwitchOn;
            }
            set
            {
                if (_isSwitchOn != value)
                {
                    _isSwitchOn = value;
                    if (Tag != null)
                    {
                        ((zwaUserGroup1)Tag).authorised = (_isSwitchOn) ? "1" : "0";
                        MessagingCenter.Send(this, "UpdateHeaderSwitchOn");  //<--- send to VM to modified the header itemGroup

                        HandlerSpecialPermission();
                        OnPropertyChanged(nameof(isSwitchOn));
                    }
                }
            }
        }

        object _Tag;
        public object Tag // holding source converting object
        {
            get
            {
                return _Tag;
            }
            set
            {
                if (_Tag != value)
                {
                    _Tag = value;
                    if (_Tag != null)
                    {
                        _isSwitchOn = (((zwaUserGroup1)_Tag).authorised == "1") ? true : false;
                    }
                }
            }
        }

        /// <summary>
        /// Handle the on property changed event
        /// </summary>
        /// <param name="propertyName"></param>
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Dispose code
        /// </summary>
        public void Dispose() { } //GC.Collect();

        /// <summary>
        /// Capture Permitted Control Limit from the class inner routine
        /// </summary>
        public async void CapturePermittedControlLimit()
        {
            try
            {
                var selectField = (zwaUserGroup1)Tag;

                string updatedValue = await new Dialog().DisplayPromptAsync($"Input {Title} limit",
                    $"Numeric Only [any values from 0-9], Input zero (0) as no limit, Last limit : {selectField.ctrlLimit}",
                    "Confirm",
                    "Cancel",
                    "",
                    -1,
                    Keyboard.Numeric,
                    $"{selectField.ctrlLimit}");

                //$"Input {Title} limit"
                //, $"Numeric Only [any values from 0-9], Input zero (0) as no limit, Last limit : {selectField.ctrlLimit}"
                //, ""
                //, "Confirm"
                //, "Cancel"
                //, ""
                //, -1
                //, Keyboard.Numeric);


                #region Validate input
                if (updatedValue == null)
                {
                    goto ResetDesc;
                }

                if (updatedValue.Equals(""))
                {
                    goto ResetDesc;
                }

                if (!IsNumeric(updatedValue))
                {
                    DisplayAlert("Alert", "String value not allowed, Please try again. Thanks", "OK");
                    goto ResetDesc;
                }

                decimal value = Convert.ToDecimal(updatedValue);
                if (selectField.ctrlLimit == value)
                {
                    goto ResetDesc;
                }

                if (value < 0)
                {
                    DisplayAlert("Alert", "Negative value not allowed, Please try again. Thanks", "OK");
                    goto ResetDesc;
                }
                #endregion

                ((zwaUserGroup1)Tag).ctrlLimit = value;

                string formatedDesc = string.Format("{0:0.0000}", selectField.ctrlLimit); // thousand separator and 4 decimal place
                Desc = (selectField.ctrlLimit >= 0) ? $"{selectField.dscrptn}, Limited : {formatedDesc}" : selectField.dscrptn;


            // deselect the selected item
            ResetDesc:
                OnPropertyChanged(nameof(Desc));
            }
            catch (Exception excep)
            {
                DisplayAlert("Alert", $"{excep}", "OK");
            }
        }

        /// <summary>
        /// Handler special permission field
        /// </summary>
        void HandlerSpecialPermission()
        {
            if (_isSwitchOn)
            {
                var permission = (zwaUserGroup1)Tag;
                if (permission.screenId == (int)EnumPermissionFieldId.SalesDiscountByPercent ||
                    permission.screenId == (int)EnumPermissionFieldId.SalesDiscountByValue)
                {
                    CapturePermittedControlLimit();
                }
                return;
            }
        }

        /// <summary>
        /// Display the dialog onto the screen
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="btnStr"></param>
        async void DisplayAlert(string title, string message, string accept) =>
            await new Dialog().DisplayAlert(title, message, accept);


        /// <summary>
        ///  check input is numeric
        /// </summary>
        /// <param name="numbericString"></param>
        /// <returns></returns>
        bool IsNumeric(string numbericString) =>
            decimal.TryParse(numbericString, out decimal decimalVal);
    }
}
