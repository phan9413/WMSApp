using Acr.UserDialogs;
using DbClass;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.GRPO;
using WMSApp.Views.Share;
using Xamarin.Forms;

namespace WMSApp.Models.SAP
{
    public class POR1_Ex : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public Command CmdChangeWhs { get; set; }
        public POR1 POLine { get; set; } // actual sap po line
        public OPOR_Ex PO { get; set; } // actual sap PO document

        /// <summary>
        /// Use to save the bin binding with serial or batch         
        /// </summary>
        public List<OBIN_Ex> Bins { get; set; }  // Use to save the bin binding with serial or batch  
        public List<string> Serials { get; set; } // keeping the serial string list
        public List<Batch> Batches { get; set; } // keeping the batch infor
        public List<zwaTransferDocDetailsBin> SerialsWithBins { get; set; } // keept serial wit bin selection
        public List<zwaTransferDocDetailsBin> BatchWithBins { get; set; } // keept serial wit bin selection

        decimal receiptQuantity;
        public decimal ReceiptQuantity
        {
            get => receiptQuantity;
            set
            {
                if (receiptQuantity == value) return;
                receiptQuantity = value;

                if (CellBackGroundColor == null) 
                    CellBackGroundColor = new Color();

                CellBackGroundColor = (receiptQuantity == 0) ? Color.White : Color.LightGreen;
                ReceiptQuantityDisplay = $"Receipt: {receiptQuantity:N}";

                if (receiptQuantity == 0)
                {
                    ShowList = string.Empty;
                    Bins = null;
                    Serials = null;
                    Batches = null;
                    SerialsWithBins = null;
                    BatchWithBins = null;
                    ItemWhsCode = string.Empty;
                }

                OnPropertyChanged(nameof(ReceiptQuantity));                                
            }
        }

        string receiptQuantityDisplay;
        public string ReceiptQuantityDisplay
        {
            get => receiptQuantityDisplay; // $"Receipt: {receiptQuantity:N}";
            set
            {
                if (receiptQuantityDisplay == value) return;
                receiptQuantityDisplay = value;
                OnPropertyChanged(nameof(ReceiptQuantityDisplay));
            }
        }

        Color cellBackGroundColor;
        public Color CellBackGroundColor
        {
            get => cellBackGroundColor;
            set
            {
                if (cellBackGroundColor == value) return;
                cellBackGroundColor = value;
                OnPropertyChanged(nameof(CellBackGroundColor));
            }
        }

        string itemWhsCode;
        public string ItemWhsCode
        {
            get => itemWhsCode;
            set
            {
                if (itemWhsCode == value) return;
                itemWhsCode = value;
                OnPropertyChanged(nameof(ItemWhsCode));
            }
        }

        string baseWarehouse;
        public string BaseWarehouse
        {
            get => baseWarehouse;
            set
            {
                if (baseWarehouse == value) return;
                baseWarehouse = value;
                OnPropertyChanged(nameof(BaseWarehouse));
            }
        }

        string direction;
        public string Direction
        {
            get => direction;
            set
            {
                if (direction == value) return;
                direction = value;
                OnPropertyChanged(nameof(Direction));
            }
        }

        public string ItemCodeDisplay => POLine?.ItemCode;
        public string ItemNameDisplay => POLine?.Dscription;
        public string OpenQuantityDisplay => $"Open Qty: {POLine?.OpenQty:N}";
        public string DocNum => $"{PO?.PO.DocNum}";
        public string CardName => $"{PO?.PO.CardName}";
        public string CardCode => $"{PO?.PO.CardCode}";

        readonly string _SelectBaseWarehouse = "_SelectBaseWarehouse20201113T1043";

        /// <summary>
        /// Show the collected string in list
        /// </summary>
        string showList;
        public string ShowList
        {
            get => showList;
            set
            {
                if (showList == value) return;
                showList = value;
                OnPropertyChanged(nameof(ShowList));
            }
        }

        public INavigation Navigation { get; set; }

        /// <summary>
        /// Construtor for this model
        /// </summary>
        public POR1_Ex()
        {
            cellBackGroundColor = Color.White;
            ReceiptQuantity = 0;
            //CmdChangeWhs = new Command(PromptWarehouseSelectionAsync);
            CmdChangeWhs = new Command(ChangeWhs);
        }

        /// <summary>
        /// lead to change warehouse and reset the entry
        /// </summary>
        void ChangeWhs()
        {
            MessagingCenter.Subscribe<OWHS>(this, _SelectBaseWarehouse, (OWHS selectedWhs) =>
            {
                MessagingCenter.Unsubscribe<OWHS>(this, _SelectBaseWarehouse);

                if (selectedWhs == null) return;
                POLine.WhsCode = selectedWhs.WhsCode;
                baseWarehouse = selectedWhs.WhsCode;
                DependencyService.Get<IToastMessage>()?.ShortAlert("Line warehouse changed");
            });

            Navigation.PushPopupAsync(new PickWarehousePopUpView(_SelectBaseWarehouse,
                $"Select an entry warehouse {ItemCodeDisplay}\n{ItemNameDisplay}", Color.Gray));
        }

        /// <summary>
        /// start a task to capture the general entries warehouse
        /// </summary>
        /// <returns></returns>
        public async void PromptWarehouseSelectionAsync()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment please ...");
                if (App.Warehouses == null) return;
                var whsList = App.Warehouses.Select(x => x.WhsCode).Distinct().ToArray();
                var selectedWhs = await new Dialog().DisplayActionSheet("Select a warehouse", "Cancel", null, whsList);

                if (string.IsNullOrWhiteSpace(selectedWhs))
                {
                    return;
                }

                if (selectedWhs.ToLower().Equals("cancel")) /// if then exit the screen
                {
                    return;
                }

                /// 20200719T1221
                /// check selected warehouse contain no bin setup
                /// if no then direct exist, no need to load bin location from server for bin receipt
                var appListWhs = App.Warehouses.Where(x => x.WhsCode.Equals(selectedWhs)).FirstOrDefault();
                if (appListWhs != null && appListWhs.BinActivat.Equals("N"))
                {
                    ItemWhsCode = selectedWhs;
                    BaseWarehouse = selectedWhs;
                    Bins = null;
                    return;
                }

                // check whs with bin activated
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetWarehouseBins",
                    QueryWhs = selectedWhs
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Warehouses");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ItemWhsCode = selectedWhs;
                    BaseWarehouse = selectedWhs;
                    Bins = null;
                    return;
                }

                var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
                if (repliedCio == null)
                {
                    ItemWhsCode = selectedWhs;
                    BaseWarehouse = selectedWhs;
                    Bins = null;
                    return;
                }

                App.Bins = repliedCio.DtoBins;

                // start bins selection -> manual entry / scan by camera / scan by hand held
                // enter qty

                if (App.Bins == null)
                {
                    ItemWhsCode = selectedWhs;
                    BaseWarehouse = selectedWhs;
                    Bins = null;
                    return;
                }

                if (App.Bins.Length == 0)
                {
                    ItemWhsCode = selectedWhs;
                    BaseWarehouse = selectedWhs;
                    Bins = null;
                    return;
                }

                // to indentified the base warehouse from bin
                BaseWarehouse = selectedWhs;

                // else 
                const string repliedAddress = nameof(POR1_Ex);
                MessagingCenter.Subscribe(this, repliedAddress, (List<OBIN_Ex> receivedValue) =>
                {
                    MessagingCenter.Unsubscribe<List<OBIN_Ex>>(this, repliedAddress);
                    Bins = receivedValue;
                    UpdateReceiptQty();
                    UpdateItemWhs();
                });

                await Navigation.PushAsync(
                    new CaptureTextView($"GRPO - Add bin for {this.ItemCodeDisplay}", "Bin",
                    repliedAddress, Bins, this.POLine.OpenQty, true));
            }
            catch (System.Exception)
            {
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// HandlerExisting warehouse 
        /// </summary>
        /// <param name="existingWhs"></param>
        public async void PromptBinEntry(OWHS selectedWhs)
        {
            try
            {
                if (selectedWhs == null)
                {
                    return;
                }

                // check whs with bin activated
                var cioRequest = new Cio() // load the data from web server 
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = "GetWarehouseBins",
                    QueryWhs = selectedWhs.WhsCode
                };

                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Warehouses");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ItemWhsCode = selectedWhs.WhsCode;
                    BaseWarehouse = selectedWhs.WhsCode;
                    Bins = null;
                    return;
                }

                var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
                if (repliedCio == null)
                {
                    ItemWhsCode = selectedWhs.WhsCode;
                    BaseWarehouse = selectedWhs.WhsCode;
                    Bins = null;
                    return;
                }

                App.Bins = repliedCio.DtoBins;

                // start bins selection -> manual entry / scan by camera / scan by hand held
                // enter qty

                if (App.Bins == null)
                {
                    ItemWhsCode = selectedWhs.WhsCode;
                    BaseWarehouse = selectedWhs.WhsCode;
                    Bins = null;
                    return;
                }
                if (App.Bins.Length == 0)
                {
                    ItemWhsCode = selectedWhs.WhsCode;
                    BaseWarehouse = selectedWhs.WhsCode;
                    Bins = null;
                    return;
                }

                // to indentified the base warehouse from bin
                BaseWarehouse = selectedWhs.WhsCode;

                // else 
                const string repliedAddress = nameof(POR1_Ex);
                MessagingCenter.Subscribe(this, repliedAddress, (List<OBIN_Ex> receivedValue) =>
                {
                    MessagingCenter.Unsubscribe<List<OBIN_Ex>>(this, repliedAddress);
                    Bins = receivedValue;
                    UpdateReceiptQty();
                    UpdateItemWhs();
                });

                await Navigation.PushAsync(
                    new CaptureTextView($"GRPO - Add bin for {this.ItemCodeDisplay}",
                    "Bin", repliedAddress, Bins, POLine.OpenQty, true));
            }
            catch (System.Exception)
            {
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Update the receipt qty after bin qty entries
        /// </summary>
        void UpdateReceiptQty()
        {
            if (Bins == null) ReceiptQuantity = 0;
            ReceiptQuantity = Bins.Sum(x => x.BinQty);
        }

        /// <summary>
        /// Update the bin on screen
        /// </summary>
        void UpdateItemWhs()
        {
            if (Bins == null) return;
            var whs = string.Empty;
            var direction = (this.direction.Equals("in")) ? "<-" : "->";

            foreach (var bin in Bins)
            {
                whs += $"{bin.oBIN.BinCode} {direction} {bin.BinQty}\n";
            }

            if (whs.Length == 0) return;
            ItemWhsCode = whs;
        }

        /// <summary>
        /// Return the grpo item bin information
        /// </summary>
        /// <returns></returns>
        public List<zwaItemBin> GetList(Guid groupingGuid, Guid lineGuid)
        {
            try
            {
                const string _TransType = "BIN";
                var returnList = new List<zwaItemBin>();

                // serial no bin ******************************
                if (Serials != null && Serials.Count > 0)
                {
                    Serials.ForEach(x =>
                    returnList.Add(new zwaItemBin
                    {
                        Guid = groupingGuid,
                        ItemCode = this.ItemCodeDisplay,
                        Quantity = 1,
                        BinCode = string.Empty,
                        BinAbsEntry = -1,
                        BatchNumber = string.Empty,
                        SerialNumber = x,
                        TransType = string.Empty,
                        TransDateTime = DateTime.Now,
                        LineGuid = lineGuid
                    }));
                }

                // serial with bin ******************************
                if (SerialsWithBins != null && SerialsWithBins.Count > 0)
                {
                    SerialsWithBins.ForEach(x =>
                    returnList.Add(new zwaItemBin
                    {
                        Guid = groupingGuid,
                        ItemCode = this.ItemCodeDisplay,
                        Quantity = 1,
                        BinCode = x.BinCode,
                        BinAbsEntry = x.BinAbs,
                        BatchNumber = string.Empty,
                        SerialNumber = x.Serial,
                        TransType = _TransType,
                        TransDateTime = DateTime.Now,
                        LineGuid = lineGuid
                    }));
                }

                // batch no bin ******************************
                if (Batches != null && Batches.Count > 0)
                {
                    Batches.ForEach(x =>
                       returnList.Add(new zwaItemBin
                       {
                           Guid = groupingGuid,
                           ItemCode = this.ItemCodeDisplay,
                           Quantity = x.Qty,
                           BinCode = string.Empty,
                           BinAbsEntry = -1,
                           BatchNumber = x.DistNumber,
                           SerialNumber = string.Empty,
                           TransType = string.Empty,
                           TransDateTime = DateTime.Now,

                           BatchAttr1 = x.Attribute1,
                           BatchAttr2 = x.Attribute2,
                           BatchAdmissionDate = x.Admissiondate,
                           BatchExpiredDate = x.Expireddate,
                           LineGuid = lineGuid
                       }));
                }

                // batch with bin 
                if (BatchWithBins != null && BatchWithBins.Count > 0)
                {
                    BatchWithBins.ForEach(x => x.Bins.ForEach(b =>
                      returnList.Add(new zwaItemBin
                      {
                          Guid = groupingGuid,
                          ItemCode = this.ItemCodeDisplay,
                          Quantity = b.BatchTransQty,
                          BinCode = b.oBIN.BinCode,
                          BinAbsEntry = b.oBIN.AbsEntry,
                          BatchNumber = b.BatchNumber,
                          SerialNumber = string.Empty,
                          TransType = _TransType,
                          TransDateTime = DateTime.Now,
                          BatchAttr1 = x.BatchProp.Attribute1,
                          BatchAttr2 = x.BatchProp.Attribute2,
                          BatchAdmissionDate = x.BatchProp.Admissiondate,
                          BatchExpiredDate = x.BatchProp.Expireddate,
                          LineGuid = lineGuid
                      })));
                }

                // qty no bin 
                //if (this.receiptQuantity > 0)
                //{
                //    returnList.Add(new zwaItemBin
                //    {
                //        Guid = groupingGuid,
                //        ItemCode = this.ItemCodeDisplay,
                //        Quantity = receiptQuantity,
                //        BinCode = string.Empty,
                //        BinAbsEntry = -1,
                //        BatchNumber = string.Empty,
                //        SerialNumber = string.Empty,
                //        TransType = string.Empty,
                //        TransDateTime = DateTime.Now
                //    });
                //}

                // qty with bin 
                if (Bins != null && Bins.Count > 0)
                {
                    Bins.ForEach(x =>
                     returnList.Add(new zwaItemBin
                     {
                         Guid = groupingGuid,
                         ItemCode = this.ItemCodeDisplay,
                         Quantity = x.BinQty,
                         BinCode = x.oBIN.BinCode,
                         BinAbsEntry = x.oBIN.AbsEntry,
                         BatchNumber = string.Empty,
                         SerialNumber = string.Empty,
                         TransType = _TransType,
                         TransDateTime = DateTime.Now,
                         LineGuid = lineGuid
                     }));
                }
                return returnList;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                return null;
            }
        }
    }
}
