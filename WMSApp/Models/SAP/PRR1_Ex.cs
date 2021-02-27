using DbClass;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WMSApp.Interface;
using WMSApp.Models.Transfer1;
using WMSApp.ViewModels.Transfer1;
using WMSApp.Views.Share;
using Xamarin.Forms;

namespace WMSApp.Models.SAP
{
    public class PRR1_Ex : PRR1, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public Command CmdChangeWhs { get; set; }
        public OPRR_Ex PRR { get; set; } // actual sap GRN document
        public OPDN_Ex GRN { get; set; } // actual sap GRN document
        public OPCH_Ex ApInv { get; set; } // actual sap ap invoice document

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
        public string DocNum => $"{PRR.DocNum}";
        public string CardName => $"{PRR.CardName}";
        public string CardCode => $"{PRR.CardCode}";

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
        public PRR1_Ex()
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

    }
}

