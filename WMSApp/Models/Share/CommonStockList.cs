using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using Xamarin.Forms.Internals;

namespace WMSApp.Models.Share
{
    public class CommonStockInfo : INotifyPropertyChanged
    {
        public int SysNumber { get; set; }
        public string DistNumber { get; set; }
        public string MnfSerial { get; set; }
        public string LotNumber { get; set; }
        public DateTime InDate { get; set; }
        public string Status { get; set; }
        public int AbsEntry { get; set; }
        public int BinAbs { get; set; }
        public int SnBMDAbs { get; set; }
        public string ItemCode { get; set; }
        public string itemName { get; set; }
        public string WhsCode { get; set; }
        public decimal OnHandQty { get; set; }
        public string BinCode { get; set; }

        // App deisplay and opr
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        bool isChecked;
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                if (isChecked == value) return;
                isChecked = value;
                OnPropertyChanged(nameof(IsChecked));

                SelectedColr = (isChecked == true) ? SelectedColr = Color.Wheat : Color.White;
            }
        }

        Color selectedColr;
        public Color SelectedColr
        {
            get => selectedColr;
            set
            {
                if (selectedColr == value) return;
                selectedColr = value;
                OnPropertyChanged(nameof(SelectedColr));
            }
        }

        #region keeping to warehouse information 
        string toWhs;
        public string ToWhs
        {
            get => toWhs;
            set
            {
                if (toWhs == value) return;
                toWhs = value;
                OnPropertyChanged(nameof(ToWhs));
            }
        }

        string toBinCode;
        public string ToBinCode
        {
            get => toBinCode;
            set
            {
                if (toBinCode == value) return;
                toBinCode = value;
                OnPropertyChanged(nameof(ToBinCode));
            }
        }

        int toBinAbs;
        public int ToBinAbs
        {
            get => toBinAbs;
            set
            {
                if (toBinAbs == value) return;
                toBinAbs = value;
                OnPropertyChanged(nameof(ToBinAbs));
            }
        }

        #endregion
        #region for bin selction view
        public string FromWhs
        {
            get => WhsCode; 
            set
            {
                if (WhsCode == value) return;
                WhsCode = value;
                OnPropertyChanged(nameof(FromWhs));
            }
        }

        public decimal Qty
        {
            get => OnHandQty;
            set
            {
                if (OnHandQty == value) return;
                OnHandQty = value;
                OnPropertyChanged(nameof(Qty));
            }
        }

        #endregion
    }
}
