using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.SAP;
using WMSApp.Views.Share;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Share
{
    public class CaptureTextVM : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string PropName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropName));
        public void Dispose() { }// => GC.Collect();
        public string CurrentCaptureMode { get; set; }
        public string CapturePageTitle { get; set; }
        public Command CmdStartCamera { get; set; }
        public Command CmdStartManualEnter { get; set; }
        public Command CmdClose { get; set; }
        public Command CmdSelectBinCode { get; set; }
        public Command CmdSearchBinCode { get; set; }
        OBIN_Ex selectedItem;
        public OBIN_Ex SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    // handle the selected item 
                    HandlerCaptureResult(selectedItem.oBIN.BinCode);
                    selectedItem = null;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }
        List<OBIN_Ex> EnteredBin;
        public ObservableCollection<OBIN_Ex> ItemsSource { get; set; }

        string itemInfo;
        public string ItemInfo
        {
            get => itemInfo;
            set
            {
                if (itemInfo != value)
                {
                    itemInfo = value;
                    OnPropertyChanged(nameof(ItemInfo));
                }
            }
        }

        INavigation Navigation;
        string RepliedCallerMcAddress;
        string DataType;
        decimal ControlSumOfQty;//, Remaining;
        bool IsCheckQty = false;
            
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="navigation"></param>
        /// <param name="pageTitle"></param>
        /// <param name="dataType"></param>
        /// <param name="repliedMcAddress"></param>
        /// <param name="bins"></param>
        public CaptureTextVM(INavigation navigation, string pageTitle, 
            string dataType, string repliedMcAddress, List<OBIN_Ex> bins, 
            decimal controlSumOfQty, bool isCheckQty)
        {
            InitCmd();

            Navigation = navigation;
            RepliedCallerMcAddress = repliedMcAddress;
            CapturePageTitle = pageTitle;
            DataType = dataType;
            EnteredBin = bins;
            ControlSumOfQty = controlSumOfQty;
            //Remaining = controlSumOfQty;
            IsCheckQty = isCheckQty;

            if (isCheckQty)            
            {
                ItemInfo = $"Total Needed {ControlSumOfQty:N}";  //, Remaining {Remaining:N}";
            }            
            
            if (bins != null)
            {
                ItemsSource = new ObservableCollection<OBIN_Ex>(EnteredBin);
            }
        }

        /// <summary>
        /// Handle the remo
        /// </summary>
        /// <param name="selected"></param>
        public void RemoveBinListObject(OBIN_Ex selected)
        {
            if (EnteredBin == null) return;
            if (EnteredBin.Count == 0) return;

            //Remaining += selected.BinQty;
            ItemInfo = $"Total Needed {ControlSumOfQty:N}";

            EnteredBin.Remove(selected);
            ResetList();
        }

        /// <summary>
        /// Handler the input from the multiple source
        /// </summary>
        /// <param name="result"></param>
        public async void HandlerCaptureResult(string result)
        {
            if (App.Bins == null)
            {
                DisplayToast($"There is no bin configured for this warehouse, please try again later (x00). Thanks");
                return;
            }

            if (App.Bins.Length == 0)
            {
                DisplayToast($"There is no bin configured for this warehouse, please try again later (x01). Thanks");
                return;
            }

            if (result.ToLower().Equals("cancel"))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                return;
            }

            result = result.ToUpper();
            var existingBin = App.Bins.Where(x => x.BinCode.Equals(result)).FirstOrDefault();
            if (existingBin == null)
            {
                DisplayToast($"{DataType} -> '{result}' no found in configuration, please try again later. Thanks");
                return;
            }

            string binQty = await new Dialog()
                .DisplayPromptAsync($"Input {DataType} {result} Qty", "", 
                "OK", "Cancel", "", -1, Keyboard.Numeric, "");

            if (string.IsNullOrWhiteSpace(binQty))
            {
                return;
            }

            bool isDecimal = decimal.TryParse(binQty, out decimal mValue);
            if (!isDecimal)
            {
                DisplayAlert("Alert", $"Invalid {DataType} Qty, please try again later. Thanks", "Ok");
                return;
            }

            if (mValue < 0)
            {
                DisplayAlert("Alert", $"Invalid {DataType} Qty < zero, please try again later. Thanks", "Ok");
                return;
            }

            if (EnteredBin == null) EnteredBin = new List<OBIN_Ex>();
            int locId = EnteredBin.IndexOf(EnteredBin.Where(x => x.oBIN.BinCode.Equals(result)).FirstOrDefault());
            if (locId <= -1) // detected as new entry
            {
                EnteredBin.Add(
                   new OBIN_Ex
                   {
                       oBIN = existingBin,
                       BinQty = mValue,
                       Text = existingBin.BinCode,
                       Detail = $"Receipt -> {mValue:N}"
                   });
                DisplayToast($"{existingBin.BinCode} add in receipt -> {mValue:N}");
                ResetList();
                return;
            }

            // update the bin list with location
            EnteredBin[locId].oBIN = existingBin;
            EnteredBin[locId].BinQty = mValue;
            EnteredBin[locId].Text = existingBin.BinCode;
            EnteredBin[locId].Detail = $"Receipt -> {mValue:N}";
            EnteredBin[locId].RaiseOnPropertyChanged();
            DisplayToast($"{existingBin.BinCode} add in receipt -> {mValue:N}");
            ResetList();
        }

        /// <summary>
        /// Init the cmd
        /// </summary>
        void InitCmd()
        {
            CmdStartCamera = new Command(() =>
            {
                // start camra scan and return 
                const string address = nameof(CaptureTextVM);
                MessagingCenter.Subscribe(this, address, (string result) =>
                {
                    MessagingCenter.Unsubscribe<string>(this, address);
                    HandlerCaptureResult(result);
                });
                Navigation.PushAsync(new CameraScanView(address));
            });

            CmdStartManualEnter = new Command(async () =>
            {
                string binCode = await new Dialog()
                                .DisplayPromptAsync($"Input {DataType}", "", "OK", "Cancel", "", -1, Keyboard.Default, "");

                HandlerCaptureResult(binCode);
            });

            CmdClose = new Command(SaveAndClose);
            CmdSelectBinCode = new Command(PromptBinCodeSelect);
            CmdSearchBinCode = new Command(SearchBinCode);
        }

        /// <summary>
        /// Save and close the bin item whs collection
        /// </summary>
        public void SaveAndClose()
        {
            if (EnteredBin == null) EnteredBin = new List<OBIN_Ex>();
            var mValue = EnteredBin.Sum(x => x.BinQty);

            // 20200721
            // check the overall qty when save
            if (IsCheckQty && mValue > ControlSumOfQty)
            {
                DisplayAlert("Alert", $"Invalid {DataType} sum input {mValue:N} > needed {ControlSumOfQty:N}," +
                    $" please remove some bin entry row, and try again later. Thanks", "Ok");
                return;
            }

            MessagingCenter.Send<List<OBIN_Ex>>(EnteredBin, RepliedCallerMcAddress);
            Navigation.PopAsync();
        }

        /// <summary>
        /// Reset the list  for user screen
        /// </summary>
        void ResetList()
        {
            if (EnteredBin == null) return;
            ItemsSource = new ObservableCollection<OBIN_Ex>(EnteredBin);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        /// Show a general list view 
        /// for user to select and return a selected string
        /// </summary>
        void SearchBinCode ()
        {
            string returnAddress = "SearchBinCode";
            MessagingCenter.Subscribe(this, returnAddress, (string binCode) =>
            {
                MessagingCenter.Unsubscribe<string>(this, returnAddress);
                if (string.IsNullOrWhiteSpace(binCode))
                {
                    return;
                }

                if (binCode.Equals("cancel"))
                {
                    return;
                }

                HandlerCaptureResult(binCode);
                return;
            });

            var binList = App.Bins.Select(x => x.BinCode).Distinct().ToArray();
            Navigation.PushAsync(new GeneralListView("Search Bin Code", returnAddress, binList));
        }

        /// <summary>
        /// Prompt a list of bin code for user selection and qty
        /// </summary>
        async void PromptBinCodeSelect()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment please ...");
                if (App.Bins == null)
                {
                    DisplayAlert("Alert", $"There is no bin configured for this warehouse, please try again later(x00). Thanks", "Ok");
                    return;
                }

                if (App.Bins.Length == 0)
                {
                    DisplayAlert("Alert", $"There is no bin configured for this warehouse, please try again later (x01). Thanks", "Ok");
                    return;
                }

                var binList = App.Bins.Select(x => x.BinCode).Distinct().ToArray();
                var selectedBinCode = await new Dialog().DisplayActionSheet("Select a bin", "cancel", null, binList);

                if (selectedBinCode.ToLower().Equals("cancel"))
                {
                    return;
                }

                HandlerCaptureResult(selectedBinCode);
            }
            catch (Exception)
            {   
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }


        /// <summary>
        /// Display alrt simulation in VM
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="accept"></param>
        async void DisplayAlert(string title, string message, string accept) => 
            await new Dialog().DisplayAlert(title, message, accept);        

        /// <summary>
        /// Show slilent message on user screen
        /// </summary>
        /// <param name="message"></param>
        void DisplayToast(string message) => 
            DependencyService.Get<IToastMessage>()?.ShortAlert(message); 
    }
}
