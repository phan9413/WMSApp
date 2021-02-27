using DbClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Models.GRPO;
using WMSApp.Models.SAP;
using WMSApp.Models.Transfer1;
using WMSApp.ViewModels.Transfer1;
using Xamarin.Forms;


namespace WMSApp.Models.GIGR
{
    public class OITM_Ex : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string proName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
        public string Guid { get; set; } = string.Empty;
        //public Command CmdRemoveItem { get; set; }
        public OITM Item { get; set; }
        public List<OBIN_Ex> Bins { get; set; }
        /// <summary>
        /// for GI process 
        /// </summary>
        public List<OSBQ_Ex> SerialsInBin { get; set; } // to kept the serial in bin
        public List<OSRQ_Ex> Serials { get; set; } // kept list of the serials
        public List<OBBQ_Ex> BatchInBin { get; set; } // kept the batch bin 
        public List<OBTQ_Ex> Batches { get; set; } // kept the batch 
        public List<OIBQ_Ex> ItemQtyBins { get; set; } // kept the qty in bin

        /// <summary>
        /// For GR ---------------------------------------------------------
        /// </summary>
        public List<zwaTransferDocDetailsBin> GRSerialToBins { get; set; } // kept for gr serial allocated to bin
        public List<string> GRSerialList { get; set; }
        public List<zwaTransferDocDetailsBin> GRBatchInBins { get; set; } // kept the batch in bin
        public List<Batch> GRBatch { get; set; } // kept the batch list
        public List<OBIN_Ex> GRQtyBin { get; set; } // keep the gr item qty in bin
        // ----------------------------------------------------------------------

        public string itemWhsCode;
        public string ItemWhsCode
        {
            get => $"{TransType} {itemWhsCode}";
            set
            {
                if (itemWhsCode == value) return;
                itemWhsCode = value;
                OnPropertyChanged(nameof(ItemWhsCode));
            }
        }
        decimal transQty;
        public decimal TransQty
        {
            get => transQty;
            set
            {
                if (transQty != value)
                {
                    transQty = value;
                }

                OnPropertyChanged(nameof(TransQty));
                OnPropertyChanged(nameof(TransQuantityDisplay));
                OnPropertyChanged(nameof(CellColor));
                OnPropertyChanged(nameof(ItemWhsCode));
            }
        }
        public Color CellColor
        {
            get
            {
                if (transQty == 0) return Color.White;
                return Color.LightGreen;
            }
        }
        public string ItemCodeDisplay => Item.ItemCode;
        public string ItemNameDisplay => Item.ItemName;
        public string TransQuantityDisplay => $"{TransType} {TransQty:N}";

        string transType;
        public string TransType
        {
            get => (transType.Equals("GI")) ? "Out" : "In";
            set
            {
                if (transType != value)
                {
                    transType = value;
                    OnPropertyChanged(nameof(TransType));
                }
            }
        }

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

        string baseWarehouse;
        public string BaseWarehouse
        {
            get => baseWarehouse;
            set
            {
                if (baseWarehouse != value)
                {
                    baseWarehouse = value;
                    OnPropertyChanged(nameof(BaseWarehouse));
                }
            }
        }

        //public INavigation Navigation { get; set; }

        /// <summary>
        /// The constructor
        /// </summary>
        public OITM_Ex()
        {
            //CmdRemoveItem = new Command(() =>
            //{
            //    MessagingCenter.Send<OITM_Ex>(this, "removeItem");
            //});
        }

        /// <summary>
        /// Use to formating the list from varous source to return the list of the 
        /// </summary>
        /// <returns></returns>
        public List<zwaItemBin> GetList(Guid groupingGuid, Guid lineGuid)
        {
            try
            {
                List<zwaItemBin> returnList = new List<zwaItemBin>();
                if (transType.Equals("GI"))
                {
                    #region Reference
                    // group all entry and get sum of qty of same item 
                    //// grouing of batch + bin to prevent duplicate batch  and bin trasfer
                    ///// reference 
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
                    #endregion end referece

                    #region GI reading
                    if (SerialsInBin != null)
                    {
                        SerialsInBin.ForEach(x =>
                        {
                            returnList.Add(new zwaItemBin
                            {
                                Guid = groupingGuid,
                                ItemCode = x.ItemCode,
                                Quantity = 1,
                                BinCode = x.BinCode,
                                BinAbsEntry = x.BinAbs,                                
                                SerialNumber = x.DistNumber,
                                TransType = transType,
                                TransDateTime = DateTime.Now, 
                                LineGuid = lineGuid
                            });
                        });
                    }

                    if (Serials != null)
                    {
                        Serials.ForEach(x =>
                        {
                            returnList.Add(new zwaItemBin
                            {
                                Guid = groupingGuid,
                                ItemCode = x.ItemCode,
                                Quantity = 1,
                                SerialNumber = x.DistNumber,
                                TransType = transType,
                                TransDateTime = DateTime.Now,
                                BinCode = null,
                                BinAbsEntry = -1,
                                LineGuid = lineGuid
                            });
                        });
                    }

                    if (BatchInBin != null)
                    {
                        BatchInBin.ForEach(x =>
                        {
                            returnList.Add(new zwaItemBin
                            {
                                Guid = groupingGuid,
                                ItemCode = x.ItemCode,
                                Quantity = x.TransferBatchQty,
                                BatchNumber = x.DistNumber,
                                BinAbsEntry = x.BinAbs,
                                BinCode = x.BinCode,
                                TransType = transType,
                                TransDateTime = DateTime.Now,
                                LineGuid = lineGuid
                            });
                        });
                    }

                    if (Batches != null)
                    {
                        Batches.ForEach(x =>
                        {
                            returnList.Add(new zwaItemBin
                            {
                                Guid = groupingGuid,
                                ItemCode = x.ItemCode,
                                Quantity = x.TransferBatchQty,
                                BatchNumber = x.DistNumber,
                                TransType = transType,
                                TransDateTime = DateTime.Now,
                                BinCode = null,
                                BinAbsEntry = -1,
                                LineGuid = lineGuid
                            });
                        });
                    }

                    if (ItemQtyBins != null)
                    {
                        ItemQtyBins.ForEach(x =>
                        {
                            returnList.Add(new zwaItemBin
                            {
                                Guid = groupingGuid,
                                ItemCode = x.ItemCode,
                                Quantity = x.TransferQty,
                                BinCode = x.BinCode,
                                BinAbsEntry = x.BinAbs,
                                TransType = transType,
                                TransDateTime = DateTime.Now,
                                LineGuid = lineGuid
                            });
                        });
                    }
                    var groupedList1 = returnList
                   .GroupBy(b => new
                   {
                       b.Guid,
                       b.ItemCode,
                       b.BinCode,
                       b.BinAbsEntry,
                       b.BatchNumber,
                       b.SerialNumber,
                       b.TransType,
                       b.TransDateTime,
                       b.BatchAttr1,
                       b.BatchAttr2,
                       b.BatchAdmissionDate,
                       b.BatchExpiredDate,
                       b.LineGuid
                   })
                   .Select(newLine => new zwaItemBin
                   {
                       Guid = newLine.First().Guid,
                       ItemCode = newLine.First().ItemCode,
                       Quantity = newLine.Sum(x => x.Quantity),
                       BinCode = newLine.First().BinCode,
                       BinAbsEntry = newLine.First().BinAbsEntry,
                       BatchNumber = newLine.First().BatchNumber,
                       SerialNumber = newLine.First().SerialNumber,
                       TransType = newLine.First().TransType,
                       TransDateTime = newLine.First().TransDateTime,
                       BatchAttr1 = newLine.First().BatchAttr1,
                       BatchAttr2 = newLine.First().BatchAttr2,
                       BatchAdmissionDate = newLine.First().BatchAdmissionDate,
                       BatchExpiredDate = newLine.First().BatchExpiredDate,
                       LineGuid = newLine.First().LineGuid
                   }).ToList();

                    #endregion
                    return groupedList1;
                }

                #region GR process
                // handle the GR item
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

                if (GRSerialToBins != null)
                {
                    GRSerialToBins.ForEach(x =>
                    {
                        returnList.Add(new zwaItemBin
                        {
                            Guid = groupingGuid,
                            ItemCode = x.ItemCode,
                            Quantity = 1,
                            BinCode = x.BinCode,
                            BinAbsEntry = x.BinAbs,
                            BatchNumber = null,
                            SerialNumber = x.Serial,
                            TransType = transType,
                            TransDateTime = DateTime.Now, 
                            LineGuid = lineGuid
                        });
                    });
                }

                if (GRSerialList != null)
                {
                    GRSerialList.ForEach(x =>
                    {
                        returnList.Add(new zwaItemBin
                        {
                            Guid = groupingGuid,
                            ItemCode = this.Item.ItemCode,
                            Quantity = 1,
                            SerialNumber = x,
                            TransType = transType,
                            TransDateTime = DateTime.Now,
                            BinCode = null,
                            BinAbsEntry = -1,
                            LineGuid = lineGuid
                        });
                    });
                }

                if (GRBatchInBins != null)
                {
                    GRBatchInBins.ForEach(x =>
                    {
                        x.Bins.ForEach(b =>
                        {
                            returnList.Add(new zwaItemBin
                            {
                                Guid = groupingGuid,
                                ItemCode = this.Item.ItemCode,
                                Quantity = b.BatchTransQty,
                                BatchNumber = b.BatchNumber,
                                BatchAdmissionDate = x.BatchProp.Admissiondate,
                                BatchAttr1 = x.BatchProp.Attribute1,
                                BatchAttr2 = x.BatchProp.Attribute2,
                                BatchExpiredDate = x.BatchProp.Expireddate,
                                BinCode = b.oBIN.BinCode,
                                BinAbsEntry = b.oBIN.AbsEntry,
                                TransType = transType,
                                TransDateTime = DateTime.Now,
                                LineGuid = lineGuid
                            });
                        });
                    });
                }

                if (GRBatch != null)
                {
                    GRBatch.ForEach(x =>
                    {
                        returnList.Add(new zwaItemBin
                        {
                            Guid = groupingGuid,
                            ItemCode = this.Item.ItemCode,
                            Quantity = x.Qty,
                            BatchNumber = x.DistNumber,
                            BatchAdmissionDate = x.Admissiondate,
                            BatchAttr1 = x.Attribute1,
                            BatchAttr2 = x.Attribute2,
                            BatchExpiredDate = x.Expireddate,
                            TransType = transType,
                            TransDateTime = DateTime.Now,
                            BinCode = null,
                            BinAbsEntry = -1,
                            LineGuid = lineGuid
                        });
                    });
                }

                if (GRQtyBin != null)
                {
                    GRQtyBin.ForEach(x =>
                    {
                        returnList.Add(new zwaItemBin
                        {
                            Guid = groupingGuid,
                            ItemCode = this.Item.ItemCode,
                            Quantity = x.BinQty,
                            BinCode = x.oBIN.BinCode,
                            BinAbsEntry = x.oBIN.AbsEntry,
                            TransType = transType,
                            TransDateTime = DateTime.Now,
                            LineGuid = lineGuid
                        });
                    });
                }
                #endregion

                var groupedList = returnList
                    .GroupBy(b => new
                    {
                        b.Guid,
                        b.ItemCode,
                        b.BinCode,
                        b.BinAbsEntry,
                        b.BatchNumber,
                        b.SerialNumber,
                        b.TransType,
                        b.TransDateTime,
                        b.BatchAttr1,
                        b.BatchAttr2,
                        b.BatchAdmissionDate,
                        b.BatchExpiredDate,
                        b.LineGuid
                    })
                    .Select(newLine => new zwaItemBin
                    {
                        Guid = newLine.First().Guid,
                        ItemCode = newLine.First().ItemCode,
                        Quantity = newLine.Sum(x => x.Quantity),                        
                        BinCode = newLine.First().BinCode,
                        BinAbsEntry = newLine.First().BinAbsEntry,
                        BatchNumber = newLine.First().BatchNumber,
                        SerialNumber = newLine.First().SerialNumber,
                        TransType = newLine.First().TransType,
                        TransDateTime = newLine.First().TransDateTime,
                        BatchAttr1 = newLine.First().BatchAttr1,
                        BatchAttr2 = newLine.First().BatchAttr2,
                        BatchAdmissionDate = newLine.First().BatchAdmissionDate,
                        BatchExpiredDate = newLine.First().BatchExpiredDate,
                        LineGuid = newLine.First().LineGuid
                    }).ToList();

                return groupedList;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                return null;
            }
        }

        /// <summary>
        /// to reset the cell
        /// </summary>
        public void Reeset()
        {
            SerialsInBin = null;
            Serials = null;
            BatchInBin = null;
            Batches = null;
            ItemQtyBins = null;
            GRSerialToBins = null;
            GRSerialList = null;
            GRBatchInBins = null;
            GRBatch = null;
            GRQtyBin = null;
            TransQty = 0;
            ItemWhsCode = string.Empty;
            TransType = string.Empty;
            ShowList = string.Empty;
        }

        /// <summary>
        /// start a task to capture the general entries warehouse
        /// </summary>
        /// <returns></returns>
        //public async void PromptWarehouseSelectionAsync(OWHS selectedWhs, string returnAddress)
        //{
        //    try
        //    {
        //        UserDialogs.Instance.ShowLoading("A moment please ...");

        //        /// 20200719T1221
        //        /// check selected warehouse contain no bin setup
        //        /// if no then direct exist, no need to load bin location from server for bin receipt                

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

        //        BaseWarehouse = selectedWhs.WhsCode;
        //        // else 
        //        const string repliedAddress = nameof(OITM_Ex);
        //        MessagingCenter.Subscribe(this, repliedAddress, (List<OBIN_Ex> receivedValue) =>
        //        {
        //            MessagingCenter.Unsubscribe<List<OBIN_Ex>>(this, repliedAddress);
        //            Bins = receivedValue;                    
        //            PrepareItemWhsDisplay();
        //            UpdateReceiptQty();

        //            MessagingCenter.Send<OITM_Ex>(this, returnAddress); // return the VM for add into the list view
        //        });

        //        await Navigation.PushAsync(
        //            new CaptureTextView($"{TransType} - bin for {this.ItemCodeDisplay}", 
        //            "Bin", repliedAddress, Bins, -1, false));
        //    }
        //    catch (Exception excep)
        //    {
        //        Console.WriteLine($"Debug - {TransType} PromptWarehouseSelectionAsync, {excep}");
        //    }
        //    finally
        //    {
        //        UserDialogs.Instance.HideLoading();
        //    }
        //}

        /// <summary>
        /// prepare to display the bin whs code 
        /// </summary>
        //void PrepareItemWhsDisplay()
        //{
        //    if (Bins == null) return;
        //    var whsDisplay = string.Empty;
        //    foreach (var bin in Bins)
        //    {
        //        whsDisplay += $"{bin.oBIN.BinCode} -> {bin.BinQty}\n";
        //    }

        //    if (whsDisplay.Length == 0) return;
        //    itemWhsCode = whsDisplay;
        //}

        /// <summary>
        /// Update the receipt qty after bin qty entries
        /// </summary>
        //void UpdateReceiptQty()
        //{
        //    if (Bins == null) TransQty = 0;
        //    TransQty = Bins.Sum(x => x.BinQty);
        //}
    }
}
