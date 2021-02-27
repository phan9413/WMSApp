using DbClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WMSApp.ViewModels.Transfer1
{
    public class OSBQ_Ex : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        ///  properties combine OSBQ + ORSN + OBIN 
        /// </summary>
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

    }
}
