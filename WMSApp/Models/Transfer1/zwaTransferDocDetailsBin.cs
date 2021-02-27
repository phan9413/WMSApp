using DbClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using WMSApp.Models.GRPO;
using WMSApp.Models.SAP;
using Xamarin.Forms.Internals;
using ZXing.Client.Result;

namespace WMSApp.Class
{
    public class zwaTransferDocDetailsBin : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public int Id { get; set; }
        public Guid Guid { get; set; }
        public Guid LineGuid { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public string Serial { get; set; }
        public string Batch { get; set; }
        public string InternalSerialNumber { get; set; }
        public string ManufacturerSerialNumber { get; set; }
        public int BinAbs { get; set; }
        public string BinCode { get; set; }
        public int SnBMDAbs { get; set; }
        public string WhsCode { get; set; }
        public string Direction { get; set; }

        // for serial to bin selection
        public OBIN SelectedBin { get; set; }
        public List<OBIN_Ex> SelectedBins { get; set; } 

        string selectedBinCode;
        public string SelectedBinCode
        {
            get => selectedBinCode;
            set
            {
                if (selectedBinCode == value) return;
                selectedBinCode = value;
                OnPropertyChanged(nameof(SelectedBinCode));
            }
        }

        // 20201113T2155
        // add in the batch property here here for easy query
        public Batch BatchProp  { get; set; }

        // add to use to show on screen 
        bool isChecked;
        public bool IsChecked
        {
            get => isChecked;
            set
            {
               // if (isChecked == value) return;
                isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        // for keeping the bins 
        List<OBIN_Ex> bins;
        public List<OBIN_Ex> Bins
        {
            get => bins;
            set
            {
                if (bins == value) return;
                bins = value;

                if (bins == null) return;
                string displayBin = string.Empty;
                foreach (var b in bins)
                {
                    displayBin += $"-> {b.oBIN.BinCode}, Qty {b.BatchTransQty:N}\n";
                }
                BinListDisplay = displayBin;
                OnPropertyChanged(nameof(Bins));
            }
        }

        string binListDisplay;
        public string BinListDisplay
        {
            get => binListDisplay;
            set
            {
                if (binListDisplay == value) return;
                binListDisplay = value;
                OnPropertyChanged(nameof(BinListDisplay));
            }
        }

        /// <summary>
        /// Reset the bin
        /// </summary>
        public void Reset()
        {
            SelectedBinCode = string.Empty;
            BinListDisplay = string.Empty;
            Bins = null;
            SelectedBin = null;

            ReceiptQty = 0;
            Qty = 0;
            ReceiptColor = Color.White;
        }

        decimal receiptQty;
        public decimal ReceiptQty
        {
            get => receiptQty; 
            set
            {
                if (receiptQty == value) return;
                receiptQty = value;
                OnPropertyChanged(nameof(ReceiptQty));
            }
        }

        Color receiptColor;
        public Color ReceiptColor
        {
            get => receiptColor;
            set
            {
                if (receiptColor == value) return;
                receiptColor = value;
                OnPropertyChanged(nameof(ReceiptColor));
            }
        }

        /// <summary>
        /// for control the display for grpo and tranfer line
        /// </summary>
        bool fromWhsVisible;
        public bool FromWhsVisible
        {
            get => fromWhsVisible;
            set
            {
                if (fromWhsVisible == value) return;
                fromWhsVisible = value;
                OnPropertyChanged(nameof(FromWhsVisible));
            }
        }
    }
}
