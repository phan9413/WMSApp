using DbClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using WMSApp.Models.Transfer1;

namespace WMSApp.Class
{
    public class zwaTransferDocDetails : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChange(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public Guid LineGuid { get; set; }
        public string ItemCode { get; set; }
        public decimal Qty { get; set; }
        public string FromWhsCode { get; set; }
        public string ToWhsCode { get; set; }
        public string Serial { get; set; }
        public string Batch { get; set; }
        public string SourceDocBaseType { get; set; }
        public int SourceBaseEntry { get; set; }
        public int SourceBaseLine { get; set; }

        public decimal ActualReceiptQty { get; set; } // for keep the actual transfer qty

        /// <summary>
        /// for app data structure
        /// </summary>
        /// 

        string itemName;
        public string ItemName
        {
            get => itemName;
            set
            {
                if (itemName == value) return;
                itemName = value;
                OnPropertyChange(nameof(ItemName));
            }
        }

        public List<zwaTransferDocDetailsBin> FromBins { get; set; } // kept orig from bin for reference
        public List<zwaTransferDocDetailsBin> ToBins { get; set; }
        public List<TransferItemDetailBinM> Picked { get; set; }

        public List<zwaTransferDocDetailsBin> GetList()
        {
            if (Picked == null) return null;
            List<zwaTransferDocDetailsBin> list = new List<zwaTransferDocDetailsBin>();

            //// grouing of batch + bin to prevent duplicate batch  and bin trasfer
            var groupedList = Picked
                .GroupBy(b => new
                {
                    b.Guid,
                    b.LineGuid,                    
                    b.ItemCode,
                    b.ItemName,
                    b.Serial,
                    b.Batch,
                    b.InternalSerialNumber,
                    b.ManufacturerSerialNumber,
                    b.BinCode,
                    b.BinAbs,
                    b.SnBMDAbs,
                    b.WhsCode,
                    b.Direction
                })
                .Select(newBin => new zwaTransferDocDetailsBin
                {
                    Guid = newBin.First().Guid,
                    LineGuid = newBin.First().LineGuid,
                    ItemCode = newBin.First().ItemCode,
                    ItemName = newBin.First().ItemName,
                    Qty = newBin.Sum(x => x.Qty),
                    Serial = newBin.First().Serial,
                    Batch = newBin.First().Batch,
                    InternalSerialNumber = newBin.First().InternalSerialNumber,
                    ManufacturerSerialNumber = newBin.First().ManufacturerSerialNumber,
                    BinCode = newBin.First().BinCode,
                    BinAbs = newBin.First().BinAbs,                    
                    SnBMDAbs = newBin.First().SnBMDAbs,
                    WhsCode = newBin.First().WhsCode,
                    Direction = newBin.First().Direction
                }).ToList();

            if (groupedList == null) return null;
            list.AddRange(groupedList);
            return list;
        }


        Color cellCompleteColor;
        public Color CellCompleteColor
        {
            get => cellCompleteColor;
            set
            {
                if (cellCompleteColor == value) return;
                cellCompleteColor = value;
                OnPropertyChange(nameof(CellCompleteColor));
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
                OnPropertyChange(nameof(ShowList));
            }

        }

        decimal transferQty;
        public decimal TransferQty
        {
            get => transferQty;
            set
            {
                if (transferQty == value) return;
                transferQty = value;
                OnPropertyChange(nameof(TransferQty));
            }
        }

        public OITM SelectedOITM { get; set; }
    }
}
