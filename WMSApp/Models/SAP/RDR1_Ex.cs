using DbClass;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.GRPO;
using WMSApp.Models.Transfer1;
using WMSApp.ViewModels.Transfer1;
using WMSApp.Views.Share;
using Xamarin.Forms;
namespace WMSApp.Models.SAP
{
    public class RDR1_Ex : RDR1, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public Command CmdChangeWhs { get; set; }
        public ORDR_Ex SO { get; set; } // actual sap GRN document

        /// <summary>
        /// Use to save the bin binding with serial or batch   
        /// use by grpo 
        /// </summary>
        //public List<OBIN_Ex> Bins { get; set; }  // Use to save the bin binding with serial or batch  
        //public List<string> Serials { get; set; } // keeping the serial string list
        //public List<Batch> Batches { get; set; } // keeping the batch infor
        //public List<zwaTransferDocDetailsBin> SerialsWithBins { get; set; } // keept serial wit bin selection
        //public List<zwaTransferDocDetailsBin> BatchWithBins { get; set; } // keept serial wit bin selection

        /// <summary>
        /// For DO operation
        /// </summary>
        /// 
        public List<OSBQ_Ex> SerialInBin { get; set; }
        public List<OSRQ_Ex> SerialsWhs { get; set; }
        public List<OBBQ_Ex> BatchesInBin { get; set; }
        public List<OBTQ_Ex> BatchesInWhs { get; set; }
        public List<OIBQ_Ex> ItemQtyInBin { get; set; }

        public decimal exitQuantity { get; set; }
        public decimal ExitQuantity
        {
            get => exitQuantity;
            set
            {
                try
                {
                    if (exitQuantity == value) return;
                    exitQuantity = value;

                    cellBackGroundColor = (exitQuantity == 0) ? Color.White : Color.LightGreen;

                    if (exitQuantity == 0)
                    {
                        showList = string.Empty;
                        SerialInBin = null;
                        SerialsWhs = null;
                        BatchesInBin = null;
                        BatchesInWhs = null;
                        ItemQtyInBin = null;
                        itemWhsCode = string.Empty;
                    }

                    receiptQuantityDisplay = $"Exit: {exitQuantity:N}";

                    OnPropertyChanged(nameof(ExitQuantity));
                    OnPropertyChanged(nameof(ReceiptQuantityDisplay));
                    OnPropertyChanged(nameof(ShowList));
                    OnPropertyChanged(nameof(CellBackGroundColor));
                }
                catch (Exception excep)
                {
                    Console.WriteLine(excep.ToString());
                }
            }
        }

        string receiptQuantityDisplay;
        public string ReceiptQuantityDisplay
        {
            get => receiptQuantityDisplay;
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

        public string ItemCodeDisplay => ItemCode;
        public string ItemNameDisplay => Dscription;
        public string OpenQuantityDisplay => $"Open Qty: {OpenQty:N}";
        public string DocNum => $"{SO.DocNum}";
        public string CardName => $"{SO.CardName}";
        public string CardCode => $"{SO.CardCode}";

        readonly string _SelectBaseWarehouse = "_SelectBaseWarehouse20201113T1043_SO";

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
        public RDR1_Ex()
        {
            cellBackGroundColor = Color.White;
            ExitQuantity = 0;
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
                WhsCode = selectedWhs.WhsCode;
                baseWarehouse = selectedWhs.WhsCode;
                DependencyService.Get<IToastMessage>()?.ShortAlert("Line warehouse changed");
            });

            Navigation.PushPopupAsync(new PickWarehousePopUpView(_SelectBaseWarehouse,
                $"Select an entry warehouse {ItemCodeDisplay}\n{ItemNameDisplay}", Color.Gray));
        }

        /// <summary>
        /// Return list of the bin content for b1 creation
        /// </summary>
        /// <param name="groupGuid"></param>
        /// <returns></returns>
        public List<zwaItemBin> GetList(Guid groupGuid, Guid lineGuid)
        {
            try
            {
                #region reference
                // structure reference
                //SerialInBin = null;
                //SerialsWhs = null;
                //BatchesInBin = null;
                //BatchesInWhs = null;
                //ItemQtyInBin = null;

                //public int Id { get; set; }
                //public Guid Guid { get; set; }
                //public string ItemCode { get; set; }
                //public decimal Quantity { get; set; }
                //public string BinCode { get; set; }
                //public int BinAbsEntry { get; set; }
                //public string BatchNumber { get; set; }
                //public string SerialNumber { get; set; }
                //public string TransType { get; set; }
                //public DateTime TransDateTime { get; set; }
                //public string BatchAttr1 { get; set; }
                //public string BatchAttr2 { get; set; }
                //public DateTime BatchAdmissionDate { get; set; } = DateTime.Now;
                //public DateTime BatchExpiredDate { get; set; } = DateTime.Now;
                #endregion

                const string _TransType = "BIN";
                var returnList = new List<zwaItemBin>();
                if (SerialInBin != null)
                {
                    SerialInBin.ForEach(x =>
                    {
                        returnList.Add(new zwaItemBin
                        {
                            Guid = groupGuid,
                            ItemCode = ItemCode,
                            Quantity = 1,
                            BinCode = x.BinCode,
                            BinAbsEntry = x.BinAbs,
                            SerialNumber = x.DistNumber,
                            TransType = _TransType,
                            TransDateTime = DateTime.Now,
                            BatchAdmissionDate = DateTime.Now,
                            BatchExpiredDate = DateTime.Now, 
                            LineGuid = lineGuid
                        });
                    });
                }

                if (SerialsWhs != null)
                {
                    SerialsWhs.ForEach(x =>
                    {
                        returnList.Add(new zwaItemBin
                        {
                            Guid = groupGuid,
                            ItemCode = ItemCode,
                            Quantity = 1,
                            BinCode = string.Empty,
                            BinAbsEntry = -1,
                            SerialNumber = x.DistNumber,
                            TransDateTime = DateTime.Now,
                            BatchAdmissionDate = DateTime.Now,
                            BatchExpiredDate = DateTime.Now,
                            LineGuid = lineGuid
                        });
                    });
                }

                if (BatchesInBin != null)
                {
                    BatchesInBin.ForEach(x =>
                   {
                       returnList.Add(new zwaItemBin
                       {
                           Guid = groupGuid,
                           ItemCode = ItemCode,
                           Quantity = x.TransferBatchQty,
                           BinCode = x.BinCode,
                           BinAbsEntry = x.BinAbs,
                           BatchNumber = x.DistNumber,
                           TransType = _TransType,
                           TransDateTime = DateTime.Now,
                           BatchAdmissionDate = DateTime.Now,
                           BatchExpiredDate = DateTime.Now,
                           LineGuid = lineGuid
                       });
                   });
                }

                if (BatchesInWhs != null)
                {
                    BatchesInWhs.ForEach(x =>
                   {
                       returnList.Add(new zwaItemBin
                       {
                           Guid = groupGuid,
                           ItemCode = ItemCode,
                           Quantity = x.TransferBatchQty,
                           BinCode = string.Empty,
                           BinAbsEntry = -1,
                           BatchNumber = x.DistNumber,
                           TransDateTime = DateTime.Now,
                           BatchAdmissionDate = DateTime.Now,
                           BatchExpiredDate = DateTime.Now,
                           LineGuid = lineGuid
                       });
                   });
                }

                if (ItemQtyInBin != null)
                {
                    ItemQtyInBin.ForEach(x =>
                   {
                       returnList.Add(new zwaItemBin
                       {
                           Guid = groupGuid,
                           ItemCode = ItemCode,
                           Quantity = x.TransferQty,
                           BinCode = x.BinCode,
                           BinAbsEntry = x.BinAbs,
                           TransDateTime = DateTime.Now,
                           BatchAdmissionDate = DateTime.Now,
                           BatchExpiredDate = DateTime.Now, 
                           TransType = _TransType,
                           LineGuid = lineGuid
                       });
                   });
                }

                // grouping all the data 
                var groupedList = returnList
                    .GroupBy(g => new
                       {
                           g.Guid,
                           g.ItemCode,                           
                           g.BinCode,
                           g.BinAbsEntry,
                           g.BatchNumber,
                           g.SerialNumber,
                           g.TransType,
                           g.TransDateTime,
                           g.BatchAttr1,
                           g.BatchAttr2,
                           g.BatchAdmissionDate,
                           g.BatchExpiredDate, 
                           g.LineGuid
                       })
                       .Select(newItem => new zwaItemBin
                       {
                           Guid = newItem.First().Guid,
                           ItemCode = newItem.First().ItemCode,
                           Quantity = newItem.Sum(x=>x.Quantity),
                           BinCode = newItem.First().BinCode,
                           BinAbsEntry = newItem.First().BinAbsEntry,
                           BatchNumber = newItem.First().BatchNumber,
                           SerialNumber =  newItem.First().SerialNumber,
                           TransType =  newItem.First().TransType,
                           TransDateTime = newItem.First().TransDateTime,
                           BatchAttr1 = newItem.First().BatchAttr1,
                           BatchAttr2 = newItem.First().BatchAttr2,
                           BatchAdmissionDate = newItem.First().BatchAdmissionDate,
                           BatchExpiredDate = newItem.First().BatchExpiredDate,
                           LineGuid = newItem.First().LineGuid                           
                       }).ToList();

                return groupedList; // return the massaged item bin records
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                return null;
            }
        }


        /// <summary>
        /// Return the grpo item bin information
        /// </summary>
        /// <returns></returns>
        //public List<zwaItemBin> GetList(Guid groupingGuid)
        //{
        //    try
        //    {
        //        const string _TransType = "BIN";
        //        var returnList = new List<zwaItemBin>();

        //        // serial no bin ******************************
        //        if (Serials != null && Serials.Count > 0)
        //        {
        //            Serials.ForEach(x =>
        //            returnList.Add(new zwaItemBin
        //            {
        //                Guid = groupingGuid,
        //                ItemCode = this.ItemCodeDisplay,
        //                Quantity = 1,
        //                BinCode = string.Empty,
        //                BinAbsEntry = -1,
        //                BatchNumber = string.Empty,
        //                SerialNumber = x,
        //                TransType = string.Empty,
        //                TransDateTime = DateTime.Now
        //            }));
        //        }

        //        // serial with bin ******************************
        //        if (SerialsWithBins != null && SerialsWithBins.Count > 0)
        //        {
        //            SerialsWithBins.ForEach(x =>
        //            returnList.Add(new zwaItemBin
        //            {
        //                Guid = groupingGuid,
        //                ItemCode = this.ItemCodeDisplay,
        //                Quantity = 1,
        //                BinCode = x.BinCode,
        //                BinAbsEntry = x.BinAbs,
        //                BatchNumber = string.Empty,
        //                SerialNumber = x.Serial,
        //                TransType = _TransType,
        //                TransDateTime = DateTime.Now
        //            }));
        //        }

        //        // batch no bin ******************************
        //        if (Batches != null && Batches.Count > 0)
        //        {
        //            Batches.ForEach(x =>
        //               returnList.Add(new zwaItemBin
        //               {
        //                   Guid = groupingGuid,
        //                   ItemCode = this.ItemCodeDisplay,
        //                   Quantity = x.Qty,
        //                   BinCode = string.Empty,
        //                   BinAbsEntry = -1,
        //                   BatchNumber = x.DistNumber,
        //                   SerialNumber = string.Empty,
        //                   TransType = string.Empty,
        //                   TransDateTime = DateTime.Now,

        //                   BatchAttr1 = x.Attribute1,
        //                   BatchAttr2 = x.Attribute2,
        //                   BatchAdmissionDate = x.Admissiondate,
        //                   BatchExpiredDate = x.Expireddate
        //               }));
        //        }

        //        // batch with bin 
        //        if (BatchWithBins != null && BatchWithBins.Count > 0)
        //        {
        //            BatchWithBins.ForEach(x => x.Bins.ForEach(b =>
        //              returnList.Add(new zwaItemBin
        //              {
        //                  Guid = groupingGuid,
        //                  ItemCode = this.ItemCodeDisplay,
        //                  Quantity = b.BatchTransQty,
        //                  BinCode = b.oBIN.BinCode,
        //                  BinAbsEntry = b.oBIN.AbsEntry,
        //                  BatchNumber = b.BatchNumber,
        //                  SerialNumber = string.Empty,
        //                  TransType = _TransType,
        //                  TransDateTime = DateTime.Now,
        //                  BatchAttr1 = x.BatchProp.Attribute1,
        //                  BatchAttr2 = x.BatchProp.Attribute2,
        //                  BatchAdmissionDate = x.BatchProp.Admissiondate,
        //                  BatchExpiredDate = x.BatchProp.Expireddate
        //              })));
        //        }

        //        // qty no bin 
        //        //if (this.receiptQuantity > 0)
        //        //{
        //        //    returnList.Add(new zwaItemBin
        //        //    {
        //        //        Guid = groupingGuid,
        //        //        ItemCode = this.ItemCodeDisplay,
        //        //        Quantity = receiptQuantity,
        //        //        BinCode = string.Empty,
        //        //        BinAbsEntry = -1,
        //        //        BatchNumber = string.Empty,
        //        //        SerialNumber = string.Empty,
        //        //        TransType = string.Empty,
        //        //        TransDateTime = DateTime.Now
        //        //    });
        //        //}

        //        // qty with bin 
        //        if (Bins != null && Bins.Count > 0)
        //        {
        //            Bins.ForEach(x =>
        //             returnList.Add(new zwaItemBin
        //             {
        //                 Guid = groupingGuid,
        //                 ItemCode = this.ItemCodeDisplay,
        //                 Quantity = x.BinQty,
        //                 BinCode = x.oBIN.BinCode,
        //                 BinAbsEntry = x.oBIN.AbsEntry,
        //                 BatchNumber = string.Empty,
        //                 SerialNumber = string.Empty,
        //                 TransType = _TransType,
        //                 TransDateTime = DateTime.Now
        //             }));
        //        }

        //        return returnList;
        //    }
        //    catch (Exception excep)
        //    {
        //        Console.WriteLine(excep.ToString());
        //        return null;
        //    }
        //}
    }
}


/// <summary>
/// start a task to capture the general entries warehouse
/// </summary>
/// <returns></returns>
//public async void PromptWarehouseSelectionAsync()
//{
//    try
//    {
//        UserDialogs.Instance.ShowLoading("A moment please ...");
//        if (App.Warehouses == null) return;
//        var whsList = App.Warehouses.Select(x => x.WhsCode).Distinct().ToArray();
//        var selectedWhs = await new Dialog().DisplayActionSheet("Select a warehouse", "Cancel", null, whsList);

//        if (string.IsNullOrWhiteSpace(selectedWhs))
//        {
//            return;
//        }

//        if (selectedWhs.ToLower().Equals("cancel")) /// if then exit the screen
//        {
//            return;
//        }

//        /// 20200719T1221
//        /// check selected warehouse contain no bin setup
//        /// if no then direct exist, no need to load bin location from server for bin receipt
//        var appListWhs = App.Warehouses.Where(x => x.WhsCode.Equals(selectedWhs)).FirstOrDefault();
//        if (appListWhs != null && appListWhs.BinActivat.Equals("N"))
//        {
//            ItemWhsCode = selectedWhs;
//            BaseWarehouse = selectedWhs;
//            Bins = null;
//            return;
//        }

//        // check whs with bin activated
//        var cioRequest = new Cio() // load the data from web server 
//        {
//            sap_logon_name = App.waUser,
//            sap_logon_pw = App.waPw,
//            token = App.waToken,
//            phoneRegID = App.waToken,
//            request = "GetWarehouseBins",
//            QueryWhs = selectedWhs
//        };

//        string content, lastErrMessage;
//        bool isSuccess = false;
//        using (var httpClient = new HttpClientWapi())
//        {
//            content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Warehouses");
//            isSuccess = httpClient.isSuccessStatusCode;
//            lastErrMessage = httpClient.lastErrorDesc;
//        }

//        if (!isSuccess)
//        {
//            ItemWhsCode = selectedWhs;
//            BaseWarehouse = selectedWhs;
//            Bins = null;
//            return;
//        }

//        var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
//        if (repliedCio == null)
//        {
//            ItemWhsCode = selectedWhs;
//            BaseWarehouse = selectedWhs;
//            Bins = null;
//            return;
//        }

//        App.Bins = repliedCio.DtoBins;

//        // start bins selection -> manual entry / scan by camera / scan by hand held
//        // enter qty

//        if (App.Bins == null)
//        {
//            ItemWhsCode = selectedWhs;
//            BaseWarehouse = selectedWhs;
//            Bins = null;
//            return;
//        }

//        if (App.Bins.Length == 0)
//        {
//            ItemWhsCode = selectedWhs;
//            BaseWarehouse = selectedWhs;
//            Bins = null;
//            return;
//        }

//        // to indentified the base warehouse from bin
//        BaseWarehouse = selectedWhs;

//        // else 
//        const string repliedAddress = nameof(POR1_Ex);
//        MessagingCenter.Subscribe(this, repliedAddress, (List<OBIN_Ex> receivedValue) =>
//        {
//            MessagingCenter.Unsubscribe<List<OBIN_Ex>>(this, repliedAddress);
//            Bins = receivedValue;
//            UpdateReceiptQty();
//            UpdateItemWhs();
//        });

//        await Navigation.PushAsync(
//            new CaptureTextView($"GRPO - Add bin for {this.ItemCodeDisplay}", "Bin",
//            repliedAddress, Bins, this.POLine.OpenQty, true));
//    }
//    catch (System.Exception)
//    {
//    }
//    finally
//    {
//        UserDialogs.Instance.HideLoading();
//    }
//}

/// <summary>
/// HandlerExisting warehouse 
/// </summary>
/// <param name="existingWhs"></param>
//public async void PromptBinEntry(OWHS selectedWhs)
//{
//    try
//    {
//        if (selectedWhs == null)
//        {
//            return;
//        }

//        // check whs with bin activated
//        var cioRequest = new Cio() // load the data from web server 
//        {
//            sap_logon_name = App.waUser,
//            sap_logon_pw = App.waPw,
//            token = App.waToken,
//            phoneRegID = App.waToken,
//            request = "GetWarehouseBins",
//            QueryWhs = selectedWhs.WhsCode
//        };

//        string content, lastErrMessage;
//        bool isSuccess = false;
//        using (var httpClient = new HttpClientWapi())
//        {
//            content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Warehouses");
//            isSuccess = httpClient.isSuccessStatusCode;
//            lastErrMessage = httpClient.lastErrorDesc;
//        }

//        if (!isSuccess)
//        {
//            ItemWhsCode = selectedWhs.WhsCode;
//            BaseWarehouse = selectedWhs.WhsCode;
//            Bins = null;
//            return;
//        }

//        var repliedCio = JsonConvert.DeserializeObject<Cio>(content);
//        if (repliedCio == null)
//        {
//            ItemWhsCode = selectedWhs.WhsCode;
//            BaseWarehouse = selectedWhs.WhsCode;
//            Bins = null;
//            return;
//        }

//        App.Bins = repliedCio.DtoBins;

//        // start bins selection -> manual entry / scan by camera / scan by hand held
//        // enter qty

//        if (App.Bins == null)
//        {
//            ItemWhsCode = selectedWhs.WhsCode;
//            BaseWarehouse = selectedWhs.WhsCode;
//            Bins = null;
//            return;
//        }
//        if (App.Bins.Length == 0)
//        {
//            ItemWhsCode = selectedWhs.WhsCode;
//            BaseWarehouse = selectedWhs.WhsCode;
//            Bins = null;
//            return;
//        }

//        // to indentified the base warehouse from bin
//        BaseWarehouse = selectedWhs.WhsCode;

//        // else 
//        const string repliedAddress = nameof(POR1_Ex);
//        MessagingCenter.Subscribe(this, repliedAddress, (List<OBIN_Ex> receivedValue) =>
//        {
//            MessagingCenter.Unsubscribe<List<OBIN_Ex>>(this, repliedAddress);
//            Bins = receivedValue;
//            UpdateReceiptQty();
//            UpdateItemWhs();
//        });

//        await Navigation.PushAsync(
//            new CaptureTextView($"GRPO - Add bin for {this.ItemCodeDisplay}",
//            "Bin", repliedAddress, Bins, POLine.OpenQty, true));
//    }
//    catch (System.Exception)
//    {
//    }
//    finally
//    {
//        UserDialogs.Instance.HideLoading();
//    }
//}

/// <summary>
/// Update the receipt qty after bin qty entries
/// </summary>
//void UpdateReceiptQty()
//{
//    if (Bins == null) ReceiptQuantity = 0;
//    ReceiptQuantity = Bins.Sum(x => x.BinQty);
//}

/// <summary>
/// Update the bin on screen
/// </summary>
//void UpdateItemWhs()
//{
//    if (Bins == null) return;
//    var whs = string.Empty;
//    var direction = (this.direction.Equals("in")) ? "<-" : "->";

//    foreach (var bin in Bins)
//    {
//        whs += $"{bin.oBIN.BinCode} {direction} {bin.BinQty}\n";
//    }

//    if (whs.Length == 0) return;
//    ItemWhsCode = whs;
//}

