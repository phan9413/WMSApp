using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace WMSApp.Models.SAP
{
    public class FTS_vw_IMApp_ItemWhsBin : INotifyPropertyChanged
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int BinAbs { get; set; }
        public decimal OnHandQty { get; set; }
        public string WhsCode { get; set; }
        public string BinCode { get; set; }
        public string BatchSerial { get; set; }

        public int ToBinAbs { get; set; } // 20200925 in applicatio usage
        public List<FTS_vw_IMApp_ItemWhsBin> ToWhsAbsList; // 20200927 for application usage

        bool isChecked; 
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                if (isChecked == value) return;
                isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        // for screen display
        decimal transQty;
        public decimal TransferQty
        {
            get => transQty; 
            set
            {
                if (transQty == value) return;
                transQty = value;
                OnPropertyChanged(nameof(TransferQty));
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));
     }
}
