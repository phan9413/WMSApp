using System;
using System.ComponentModel;
using System.Drawing;
namespace WMSApp.Models.Transfer1
{
    public class OBTQ_Ex : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public int AbsEntry { get; set; }
        public string ItemCode { get; set; }
        public int SnBMDAbs { get; set; }
        public int BinAbs { get; set; }
        public decimal Quantity { get; set; }
        public string WhsCode { get; set; }
        public string BinCode { get; set; }
        public string DistNumber { get; set; }
        public DateTime InDate { get; set; }

        decimal transferBatchQty;

        public decimal TransferBatchQty
        {
            get => transferBatchQty;
            set
            {
                if (transferBatchQty == value) return;
                transferBatchQty = value;
                OnPropertyChanged(nameof(TransferBatchQty));
            }
        }

        Color selectedColor;
        public Color SelectedColor
        {
            get => selectedColor;
            set
            {
                if (selectedColor == value) return;
                selectedColor = value;
                OnPropertyChanged(nameof(SelectedColor));
            }
        }


        /// <summary>
        /// For display 
        /// </summary>
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
