using DbClass;
using System.ComponentModel;
using System.Drawing;

namespace WMSApp.Models.SAP
{
    public class OBIN_Ex : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string PropertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        public OBIN oBIN { get; set; }

        string text;
        public string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        string detail;
        public string Detail
        {
            get => detail;
            set
            {
                if (detail != value)
                {
                    detail = value;
                    OnPropertyChanged(nameof(Detail));
                }
            }
        }

        decimal binQty;
        public decimal BinQty
        {
            get => binQty;
            set
            {
                if (binQty != value)
                {
                    binQty = value;
                    OnPropertyChanged(nameof(BinQty));
                }
            }
        }

        /// <summary>
        /// Binding with batch information
        /// </summary>
        string batchNumber; 
        public string BatchNumber
        {
            get => batchNumber;
            set
            {
                if (batchNumber == value) return;
                batchNumber = value;
                OnPropertyChanged(nameof(BatchNumber));
            }
        }

        /// <summary>
        /// binding with batch qty
        /// </summary>
        decimal batchTransQty;
        public decimal BatchTransQty
        {
            get => batchTransQty;
            set
            {
                if (batchTransQty == value) return;
                batchTransQty = value;
                OnPropertyChanged(nameof(BatchTransQty));
            }
        }

        /// <summary>
        /// for capture the serial and bin binding
        /// 20201109
        /// </summary>
        string bindedSerialNumber;
        public string BindedSerialNumber
        {
            get => bindedSerialNumber;
            set
            {
                if (bindedSerialNumber == value) return;
                bindedSerialNumber = value;
                OnPropertyChanged(nameof(BindedSerialNumber));
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
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bin"></param>
        public OBIN_Ex() {  }

        /// <summary>
        /// for raise property changes
        /// </summary>
        public void RaiseOnPropertyChanged()
        {
            OnPropertyChanged(nameof(Text));
            OnPropertyChanged(nameof(Detail));
        }
    }
}
