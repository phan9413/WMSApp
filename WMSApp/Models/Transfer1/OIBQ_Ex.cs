using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace WMSApp.Models.Transfer1
{
    public class OIBQ_Ex : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public int AbsEntry { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int BinAbs { get; set; }
        public decimal OnHandQty { get; set; }
        public string WhsCode { get; set; }
        public string Freezed { get; set; }
        public int FreezeDoc { get; set; }
        public string BinCode { get; set; }


        decimal transferQty;
        public decimal TransferQty
        {
            get => transferQty;
            set
            {
                if (transferQty == value) return;
                transferQty = value;
                OnPropertyChanged(nameof(TransferQty));
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
