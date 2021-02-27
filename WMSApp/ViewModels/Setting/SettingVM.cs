using DbClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.Setting;
using WMSApp.Models.Setting.AppUser;
using WMSApp.Views.Setting.AppUserGroup;
using Xamarin.Forms;

namespace WMSApp.ViewModels.Setting
{
    public class SettingVM : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));

        SettingMenuItem selectedItem;
        public SettingMenuItem SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    NavigationToItemMenu(selectedItem);

                    selectedItem = null;
                    OnPropertyChanged(nameof(SelectedItem));

                }
            }
        }

        List<SettingMenuItem> itemsSource;
        public ObservableCollection<SettingMenuItem> ItemsSource { get; set; }

        INavigation Navigation;

        /// <summary>
        /// The constructor
        /// </summary>
        public SettingVM(INavigation navigation)
        {

            Navigation = navigation;
            SetupItemMenus();
            ResetListView();
        }

        /// <summary>
        /// Setup Item Menus
        /// </summary>
        void SetupItemMenus()
        {
            itemsSource = new List<SettingMenuItem>();
            itemsSource.Add(
                new SettingMenuItem
                {
                    Text = "Goods Receipt Price List Setup",
                    Details = "Setup default price list for goods recepit intergration",
                    ViewId = SettingViewEnum.GrPriceListSetupView
                });

            // 20200627T2324
            // add in the price list selection update for GI
            itemsSource.Add(
               new SettingMenuItem
               {
                   Text = "Goods Issues Price List Setup",
                   Details = "Setup default price list for app goods issues intergration",
                   ViewId = SettingViewEnum.GiPriceListSetupView
               });


            itemsSource.Add(
               new SettingMenuItem
               {
                   Text = "Goods Receipt Doc Series Setup",
                   Details = "Setup default documents series for app goods receipt intergration",
                   ViewId = SettingViewEnum.AppLGR_DocSeries
               });

            itemsSource.Add(
             new SettingMenuItem
             {
                 Text = "Goods Issues Doc Series Setup",
                 Details = "Setup default documents series for app goods issues intergration",
                 ViewId = SettingViewEnum.AppLGI_DocSeries
             });




            itemsSource.Add(
               new SettingMenuItem
               {
                   Text = "App User Group",
                   Details = "Create and maintaine the app user group and app permission",
                   ViewId = SettingViewEnum.AppUserGroupView
               });

            itemsSource.Add(
               new SettingMenuItem
               {
                   Text = "App User",
                   Details = "Create and maintaine the app user",
                   ViewId = SettingViewEnum.AppUserView
               });

            //itemsSource.Add(
            //   new SettingMenuItem
            //   {
            //       Text = "License",
            //       Details = "Check App license details and information",
            //       ViewId = SettingViewEnum.AppLicenseView
            //   });
        }

        /// <summary>
        /// Assign the setting list into the listview on user screen
        /// </summary>
        void ResetListView()
        {
            if (itemsSource == null) return;
            ItemsSource = new ObservableCollection<SettingMenuItem>(itemsSource);
            OnPropertyChanged(nameof(ItemsSource));
        }

        /// <summary>
        ///  based on user selection
        ///  navigation the page to dedicated page
        /// </summary>
        /// <param name="selected"></param>

        void NavigationToItemMenu(SettingMenuItem selected)
        {
            switch (selected.ViewId)
            {
                case SettingViewEnum.GrPriceListSetupView:
                    {
                        PromptSelectPriceList();
                        break;
                    }

                case SettingViewEnum.GiPriceListSetupView:
                    {
                        PromptGI_SelectPriceList();
                        break;
                    }
                case SettingViewEnum.AppUserGroupView:
                    {
                        Navigation.PushAsync(new AppGroupListView());
                        break;
                    }
                case SettingViewEnum.AppUserView:
                    {
                        Navigation.PushAsync(new AppUserListView());
                        break;
                    }
                case SettingViewEnum.AppLicenseView:
                    {
                        break;
                    }
                case SettingViewEnum.AppLGR_DocSeries:
                    {
                        PromptDocSeriesUserInput("GR");
                        break;
                    }
                case SettingViewEnum.AppLGI_DocSeries:
                    {
                        PromptDocSeriesUserInput("GI");
                        break;
                    }
            }
        }

        /// <summary>
        /// Prompt Doc Series User Input
        /// </summary>
        /// <param name="transType"></param>
        async void PromptDocSeriesUserInput(string transType)
        {
            try
            {
                var cioRequest = new Cio // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    request = (transType.Equals("GI")) ? "GetGoodsIssuesDocSeries" : "GetGoodsReceiptDocSeries"
                };

                var content = string.Empty;
                var lastErrorMessage = string.Empty;
                var isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Items");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrorMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    var badRequest = JsonConvert.DeserializeObject<BRequest>(content);  // < -- set the global variable                    
                    DisplayAlert($"{badRequest?.Message}\n{lastErrorMessage}");
                    return;
                }

                var cio = JsonConvert.DeserializeObject<Cio>(content);
                if (cio == null)
                {
                    DisplayAlert("Temp error in read server replied, please try again later. Thanks");
                    return;
                }

                string existingDocSeries = (transType.Equals("GI")) ? cio.ExistingGIDocSeries : cio.ExistingGrDocSeries;

                string promptTitle = $"Input {transType} Doc Series\nExisting Doc Series is : {existingDocSeries}";
                string result = await new Dialog().DisplayPromptAsync(promptTitle, "",
                    "Save", "Cancel", "", -1, Keyboard.Default, existingDocSeries);

                //if (result.Length == 0) return; // commented to allow empty value to be reset
                if (result.ToLower().Equals("cancel")) return;
                if (result.ToLower().Equals(existingDocSeries.ToLower())) return; // if there are the sames then return

                // update to the server
                UpdateDocSeries(transType, result);
            }
            catch (Exception excep)
            {
                DisplayAlert(excep.ToString());
            }
        }

        /// <summary>
        /// Update Doc Series
        /// base on given doc trans type
        /// </summary>
        /// <param name="transType"></param>
        /// <param name="docSeries"></param>
        async void UpdateDocSeries(string transType, string docSeries)
        {
            try
            {
                /// get list of the price list and existing price from the server
                var cioRequest = new Cio // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                };

                if (transType.Equals("GI")) // for GI
                {
                    cioRequest.request = "UpdateGoodsIssuesDocSeries";
                    cioRequest.UpdateIssueDocSeries = docSeries;
                    // UpdateGrDocSeries
                }
                else // for GR
                {
                    cioRequest.request = "UpdateGoodsReceiptDocSeries";
                    cioRequest.UpdateGrDocSeries = docSeries;
                    // UpdateIssueDocSeries
                }

                var content = string.Empty;
                var lastErrorMessage = string.Empty;
                var isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Items");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrorMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    var brequest = JsonConvert.DeserializeObject<BRequest>(content);
                    DisplayAlert($"{brequest.Message}{lastErrorMessage}");
                    return;
                }

                DisplayToast($"{transType} Doc Series updated.");
            }
            catch (Exception excep)
            {
                DisplayAlert(excep.ToString());
            }
        }

        /// <summary>
        /// Prompt price list for user selected. and perform update
        /// </summary>
        async void PromptSelectPriceList()
        {
            try
            {
                #region Query to server and check the gather information
                /// get list of the price list and existing price from the server
                var cioRequest = new Cio // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    request = "GetGoodsReceiptPriceList"
                };

                var content = string.Empty;
                var lastErrorMessage = string.Empty;
                var isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Items");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrorMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    var badRequest = JsonConvert.DeserializeObject<BRequest>(content);  // < -- set the global variable                    
                    DisplayAlert($"{badRequest?.Message}\n{lastErrorMessage}");
                    return;
                }

                var cio = JsonConvert.DeserializeObject<Cio>(content);
                if (cio == null)
                {
                    DisplayAlert("Temp error reading price list from server, please try again later. [x0]");
                    return;
                }

                var priceList = cio.PriceList;
                if (priceList == null)
                {
                    DisplayAlert("Temp error reading price list from server, please try again later.[x1]");
                    return;
                }

                if (priceList.Length == 0)
                {
                    DisplayAlert("Temp error reading price list from server, please try again later. [x2]");
                    return;
                }
                #endregion

                #region Preparation of price list for user select
                var displayList = priceList.Select(x => x.ListName).Distinct().ToArray();
                var existingListId = cio.ExistingGrPriceListId;
                OPLN existingPriceLst = priceList.Where(x => x.ListNum == existingListId).FirstOrDefault();

                string displayTitle = "Choose a price list\n";
                if (existingPriceLst != null)
                {
                    displayTitle += "Current price list -> " + existingPriceLst.ListName;
                }
                #endregion

                #region wait user selection and perform the update into the server
                var selectedPriceList = await new Dialog().DisplayActionSheet(displayTitle, "cancel", null, displayList);
                if (selectedPriceList.ToLower().Equals("cancel"))
                {
                    return;
                }

                if (existingPriceLst == null)
                {
                    goto PerformUpdate;
                }

                var confirm = await new Dialog().DisplayAlert("Confirm update",
                    $"Change {existingPriceLst.ListName} to {selectedPriceList} ?", "Confirm", "Cancel");

                if (!confirm) return;
                
                PerformUpdate:
                OPLN selected = priceList.Where(x => x.ListName.Equals(selectedPriceList)).FirstOrDefault();
                if (selected != null) UpdateGrPriceList(selected.ListNum);
                else
                {
                    DisplayAlert($"Error reading the selected price list " +
                        $"{selectedPriceList} property, Please try again later");
                    return;
                }

                return;
                #endregion
            }
            catch (Exception excep)
            {
                DisplayAlert(excep.ToString());
            }
        }

        /// <summary>
        /// Update the selected price list id to the server
        /// </summary>
        /// <param name="selectedPriceListId"></param>
        async void UpdateGrPriceList(int selectedPriceListId)
        {
            try
            {
                /// get list of the price list and existing price from the server
                var cioRequest = new Cio // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    request = "UpdateGoodsReceiptPriceListId",
                    UpdateGrPriceListId = selectedPriceListId
                };

                var content = string.Empty;
                var lastErrorMessage = string.Empty;
                var isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Items");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrorMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    var brequest = JsonConvert.DeserializeObject<BRequest>(content);
                    DisplayAlert($"{brequest.Message}{lastErrorMessage}");
                    return;
                }

                DisplayToast("GR Price list updated.");
            }
            catch (Exception excep)
            {
                DisplayAlert(excep.ToString());
            }
        }

        /// <summary>
        /// Prompt price list for user selected. and perform update
        /// </summary>
        async void PromptGI_SelectPriceList()
        {
            try
            {
                #region Query to server and check the gather information
                /// get list of the price list and existing price from the server
                var cioRequest = new Cio // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    request = "GetGoodsIssuesPriceList"
                };

                var content = string.Empty;
                var lastErrorMessage = string.Empty;
                var isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Items");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrorMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    var badRequest = JsonConvert.DeserializeObject<BRequest>(content);  // < -- set the global variable                    
                    DisplayAlert($"{badRequest?.Message}\n{lastErrorMessage}");
                    return;
                }

                var cio = JsonConvert.DeserializeObject<Cio>(content);
                if (cio == null)
                {
                    DisplayAlert("Temp error reading price list from server, please try again later. [x0]");
                    return;
                }

                var priceList = cio.PriceList;
                if (priceList == null)
                {
                    DisplayAlert("Temp error reading price list from server, please try again later.[x1]");
                    return;
                }

                if (priceList.Length == 0)
                {
                    DisplayAlert("Temp error reading price list from server, please try again later. [x2]");
                    return;
                }
                #endregion

                #region Preparation of price list for user select
                var displayList = priceList.Select(x => x.ListName).Distinct().ToArray();

                var existingListId = cio.ExistingGiPriceListId;
                OPLN existingPriceLst = priceList.Where(x => x.ListNum == existingListId).FirstOrDefault();

                string displayTitle = "Choose a price list\n";
                if (existingPriceLst != null)
                {
                    displayTitle += "Current price list -> " + existingPriceLst.ListName;
                }
                #endregion

                #region wait user selection and perform the update into the server
                var selectedPriceList = await new Dialog().DisplayActionSheet(displayTitle, "cancel", null, displayList);
                if (selectedPriceList.ToLower().Equals("cancel"))
                {
                    return;
                }

                if (existingPriceLst == null)
                {
                    goto PerformUpdate;
                }

                var confirm = await new Dialog().DisplayAlert("Confirm update",
                    $"Change {existingPriceLst.ListName} to {selectedPriceList} ?", "Confirm", "Cancel");

                if (!confirm) return;

                PerformUpdate:
                OPLN selected = priceList.Where(x => x.ListName.Equals(selectedPriceList)).FirstOrDefault();
                if (selected != null) UpdateGiPriceList(selected.ListNum);
                else
                {
                    DisplayAlert($"Error reading the selected price list " +
                        $"{selectedPriceList} property, Please try again later");
                    return;
                }

                return;
                #endregion
            }
            catch (Exception excep)
            {
                DisplayAlert(excep.ToString());
            }
        }

        /// <summary>
        /// Update the selected price list id to the server
        /// </summary>
        /// <param name="selectedPriceListId"></param>
        async void UpdateGiPriceList(int selectedPriceListId)
        {
            try
            {
                /// get list of the price list and existing price from the server
                var cioRequest = new Cio // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    request = "UpdateGoodsIssuesPriceListId",
                    UpdateGiPriceListId = selectedPriceListId
                };

                var content = string.Empty;
                var lastErrorMessage = string.Empty;
                var isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Items");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrorMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    var brequest = JsonConvert.DeserializeObject<BRequest>(content);
                    DisplayAlert($"{brequest.Message}{lastErrorMessage}");
                    return;
                }

                DisplayToast("GI Price list updated.");
            }
            catch (Exception excep)
            {
                DisplayAlert(excep.ToString());
            }
        }

        void DisplayToast(string message)
        {
            if (message.Length == 0) return;
            var itoast = DependencyService.Get<IToastMessage>();
            itoast?.LongAlert(message);
        }

        /// <summary>
        /// Display alert on to user screen
        /// </summary>
        /// <param name="message"></param>
        async void DisplayAlert(string message)
        {
            if (message.Length == 0) return;
            await new Dialog().DisplayAlert("Alert", message, "OK");
        }

        /// <summary>
        /// Dispose code
        /// </summary>
        public void Dispose()
        {
            //GC.Collect();
        }
    }
}
