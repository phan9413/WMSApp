using System;
using System.ComponentModel;

namespace WMSApp.Models.Transfer1
{
    public class TransferItemDetailBinM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public int Id { get; set; }
        public Guid Guid { get; set; }
        public Guid LineGuid { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }

        decimal qty;
        public decimal Qty
        {
            get => qty;
            set
            {
                qty = value;
                OnPropertyChanged(nameof(Qty));
            }
        }

        public string Serial { get; set; }
        public string Batch { get; set; }
        public string InternalSerialNumber { get; set; }
        public string ManufacturerSerialNumber { get; set; }
        public int BinAbs { get; set; }
        public int SnBMDAbs { get; set; }
        public string WhsCode { get; set; }
        public string Direction { get; set; }

        /// <summary>
        /// for screen display
        /// </summary>
        public string BinCode { get; set; }
        public string DistNum
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Serial) && !string.IsNullOrWhiteSpace(Batch))
                {
                    return $"Batch# {Batch}";
                }

                if (!string.IsNullOrWhiteSpace(Serial) && string.IsNullOrWhiteSpace(Batch))
                {
                    return $"Serial# {Serial}";
                }
                return "";
            }
        }

        public string CheckDistNum
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Serial) && !string.IsNullOrWhiteSpace(Batch))
                {
                    return $"{Batch}";
                }

                if (!string.IsNullOrWhiteSpace(Serial) && string.IsNullOrWhiteSpace(Batch))
                {
                    return $"{Serial}";
                }
                return "";
            }
        }

        public string Whs
        {
            get
            {
                if (Direction.Equals("FROM")) return $"Warehouse {WhsCode} ->";
                if (Direction.Equals("TO")) return $"Warehouse {WhsCode} <-";
                return WhsCode;
            }
        }
    }        
}
