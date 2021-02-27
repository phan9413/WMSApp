using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace WMSApp.Models.Transfer1
{
    public class OSRQ_Ex : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public int AbsEntry { get; set; }
        public string ItemCode { get; set; }
        public int SnBMDAbs { get; set; }
        public int BinAbs { get; set; }
        public decimal OnHandQty { get; set; }
        public string WhsCode { get; set; }
        public string BinCode { get; set; }
        public string DistNumber { get; set; }
        public DateTime InDate { get; set; }

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

        Color cellColor;
        public Color CellColor
        {
            get => cellColor;
            set
            {
                if (cellColor == value) return;
                cellColor = value;
                OnPropertyChanged(nameof(CellColor));
            }
        }

    }

}
