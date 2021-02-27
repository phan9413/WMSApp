using DbClass;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.GRPO;
using WMSApp.Models.Transfer1;
using WMSApp.ViewModels.Transfer1;
using WMSApp.Views.Share;
using Xamarin.Forms;

namespace WMSApp.Models.SAP
{
    public class ReturnCommonLine_Ex : DLN1, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public Command CmdChangeWhs { get; set; }
        public ReturnCommonHead_Ex Head { get; set; }

        /// <summary>
        /// Use to save the bin binding with serial or batch   
        /// use by grpo 
        /// </summary>
        public List<OBIN_Ex> Bins { get; set; }  // Use to save the bin binding with serial or batch  
        public List<string> Serials { get; set; } // keeping the serial string list
        public List<Batch> Batches { get; set; } // keeping the batch infor
        public List<zwaTransferDocDetailsBin> SerialsWithBins { get; set; } // keept serial wit bin selection
        public List<zwaTransferDocDetailsBin> BatchWithBins { get; set; } // keept serial wit bin selection

        /// <summary>
        /// For DO, GR operation
        /// </summary>
        /// 
        public List<OSBQ_Ex> SerialInBin { get; set; }
        public List<OSRQ_Ex> SerialsWhs { get; set; }
        public List<OBBQ_Ex> BatchesInBin { get; set; }
        public List<OBTQ_Ex> BatchesInWhs { get; set; }
        public List<OIBQ_Ex> ItemQtyInBin { get; set; }

        public decimal entryQuantity { get; set; }
        public decimal EntryQuantity
        {
            get => entryQuantity;
            set
            {
                try
                {
                    if (entryQuantity == value) return;
                    entryQuantity = value;
                    cellBackGroundColor = (entryQuantity == 0) ? Color.White : Color.LightGreen;

                    if (entryQuantity == 0)
                    {
                        showList = string.Empty;
                        Bins = null;
                        Serials = null;
                        Batches = null;
                        SerialsWithBins = null;
                        BatchWithBins = null;
                        ItemWhsCode = WhsCode;
                    }

                    receiptQuantityDisplay = $"{direction} : {entryQuantity:N}";
                    OnPropertyChanged(nameof(EntryQuantity));
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
        public string DocNum => $"{Head.DocNum}";
        public string CardName => $"{Head.CardName}";
        public string CardCode => $"{Head.CardCode}";

        readonly string _SelectBaseWarehouse = "_SelectBaseWarehouse20201113T1043_R";

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
        public ReturnCommonLine_Ex()
        {
            cellBackGroundColor = Color.White;
            EntryQuantity = 0;
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
                $"Select an {direction} warehouse {ItemCodeDisplay}\n{ItemNameDisplay}", Color.Gray));
        }

        /// <summary>
        /// Return list of the bin content for b1 creation
        /// </summary>
        /// <param name="groupGuid"></param>
        /// <returns></returns>
        public List<zwaItemBin> GetList_GR(Guid groupGuid, Guid lineGuid)
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
                           Quantity = newItem.Sum(x => x.Quantity),
                           BinCode = newItem.First().BinCode,
                           BinAbsEntry = newItem.First().BinAbsEntry,
                           BatchNumber = newItem.First().BatchNumber,
                           SerialNumber = newItem.First().SerialNumber,
                           TransType = newItem.First().TransType,
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
        public List<zwaItemBin> GetList(Guid groupingGuid, Guid lineGuid)
        {
            try
            {
                const string _TransType = "BIN";
                var returnList = new List<zwaItemBin>();


                //public int Id { get; set; }
                //public Guid Guid { get; set; }
                //public string ItemCode { get; set; }
                //public decimal Quantity { get; set; }
                //public string BinCode { get; set; }
                //public int BinAbsEntry { get; set; }
                //public string BatchNumber { get; set; }
                //public string SerialNumber { get; set; }
                //public string TransType { get; set; }
                //public string BatchAttr1 { get; set; }
                //public string BatchAttr2 { get; set; }
                //public Guid LineGuid { get; set; }
                //public DateTime BatchAdmissionDate { get; set; } = DateTime.Now;
                //public DateTime BatchExpiredDate { get; set; } = DateTime.Now;
                //public DateTime TransDateTime { get; set; }

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
                        LineGuid = lineGuid,
                        TransDateTime = DateTime.Now,
                        BatchAdmissionDate = DateTime.Now,
                        BatchExpiredDate = DateTime.Now
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
                        LineGuid = lineGuid,
                        TransDateTime = DateTime.Now,
                        BatchAdmissionDate = DateTime.Now,
                        BatchExpiredDate = DateTime.Now
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
                           BatchAttr1 = x.Attribute1,
                           BatchAttr2 = x.Attribute2,

                           TransDateTime = DateTime.Now,
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
                          
                          BatchAttr1 = x.BatchProp.Attribute1,
                          BatchAttr2 = x.BatchProp.Attribute2,

                          TransDateTime = DateTime.Now,
                          BatchAdmissionDate = DateTime.Now,
                          BatchExpiredDate = DateTime.Now,
                          LineGuid = lineGuid
                      })));
                }

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
                         LineGuid = lineGuid,

                         BatchAdmissionDate = DateTime.Now,
                         BatchExpiredDate = DateTime.Now
                     }));
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
                           Quantity = newItem.Sum(x => x.Quantity),
                           BinCode = newItem.First().BinCode,
                           BinAbsEntry = newItem.First().BinAbsEntry,
                           BatchNumber = newItem.First().BatchNumber,
                           SerialNumber = newItem.First().SerialNumber,
                           TransType = newItem.First().TransType,
                           TransDateTime = newItem.First().TransDateTime,
                           BatchAttr1 = newItem.First().BatchAttr1,
                           BatchAttr2 = newItem.First().BatchAttr2,
                           BatchAdmissionDate = newItem.First().BatchAdmissionDate,
                           BatchExpiredDate = newItem.First().BatchExpiredDate,
                           LineGuid = newItem.First().LineGuid,
                       }).ToList();

                return groupedList; // return the massaged item bin records
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                return null;
            }
        }
    }
}
