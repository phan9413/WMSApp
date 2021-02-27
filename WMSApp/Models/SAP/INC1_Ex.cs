using DbClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using WMSApp.Models.GRPO;

namespace WMSApp.Models.SAP
{
    public class INC1_Ex : INC1, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));
        //public INC1 Line { get; set; }
        public string BinCode { get; set; }
        public string ItemCodeDisplay => ItemCode;
        public string ItemDescDisplay => ItemDesc;
        public string WarehouseDisplay => BinCode; // GetItemWarehouseAddress();


        public List<Batch> Batches { get; set; }
        public List<string> Serials { get; set; }
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

        decimal countedQty;
        public decimal CountedQty
        {
            get => countedQty;
            set
            {
                if (countedQty == value) return;
                countedQty = value;
                OnPropertyChanged(nameof(CountedQty));
                OnPropertyChanged(nameof(CountedQtyDisplay));
                OnPropertyChanged(nameof(CellBackGroundColor));
            }
        }

        public Color CellBackGroundColor
        {
            get
            {
                if (countedQty > 0) return Color.LightGreen;
                return Color.White;
            }
        }

        public string CountedQtyDisplay => $"Counted: {countedQty:N}";


        /// <summary>
        /// The constructor
        /// </summary>
        public INC1_Ex() { CountedQty = 0; }

        public List<zwaItemBin> GetList(Guid groupGuid)
        {
            try
            {
                //         public int Id { get; set; }
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

                var resultList = new List<zwaItemBin>();
                if (Serials != null)
                {
                    Serials.ForEach(x => resultList.Add(new zwaItemBin
                    {
                        Guid = groupGuid,
                        ItemCode = ItemCode,
                        Quantity = 1,                        
                        BinCode = BinCode,
                        BinAbsEntry = BinEntry,
                       // BatchNumber = string.Empty,
                        SerialNumber = x,
                        TransType = string.Empty,
                        TransDateTime = DateTime.Now,
                        //BatchAttr1 = string.Empty,
                        //BatchAttr2 = string.Empty,
                        BatchAdmissionDate = DateTime.Now,
                        BatchExpiredDate = DateTime.Now
                    })); ;
                }

                if (Batches != null)
                {
                    Batches.ForEach(x => resultList.Add(new zwaItemBin
                    {
                        Guid = groupGuid,
                        ItemCode = ItemCode,
                        Quantity = x.Qty,
                        BinCode = BinCode,
                        BinAbsEntry = BinEntry,
                        BatchNumber = x.DistNumber,
                        //SerialNumber = string.Empty,
                        TransType = string.Empty,
                        TransDateTime = DateTime.Now,
                        BatchAttr1 = string.Empty,
                        BatchAttr2 = string.Empty,
                        BatchAdmissionDate = DateTime.Now,
                        BatchExpiredDate = DateTime.Now
                    })); ;
                }
                return resultList;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                return null;
            }
        }

        /// <summary>
        /// try to get the warehouse name with 4 level of structure
        /// </summary>
        /// <returns></returns>
        //string GetItemWarehouseAddress()
        //{
        //    if (App.BinLocations == null) return string.Empty;

        //    var binLoc = App.BinLocations.Where(x => x.AbsEntry == BinEntry).FirstOrDefault();
        //    if (binLoc == null) return WhsCode;
        //    return binLoc.BinCode;
        //}
    }
}
