using DbClass;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using WMSApp.Models.Transfer1;

namespace WMSApp.Models.SAP
{
    public class WTQ1_Ex : WTQ1, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #region Property binding
        public List<zwaTransferDetails> ItemWhsBinQty { get; set; }

        OITM selectedOITM;
        public OITM SelectedOITM
        {
            get => selectedOITM;
            set
            {
                if (selectedOITM == value) return;
                selectedOITM = value;
                OnPropertyChanged(nameof(SelectedOITM));
            }
        }

        public WTQ1 me;
        public WTQ1 Line
        {
            set
            {
                Quantity = value.OpenQty;
                FromWhsCod = value.FromWhsCod;
                WhsCode = value.WhsCode;
                ItemCode = value.ItemCode;
                Dscription = value.Dscription;
                me = value;
            }
        }

        WTQ1_Ex selectedItem;
        public WTQ1_Ex SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }
        /// <summary>
        /// 20201012T1233 for keeping the transfer line object
        /// </summary>
        public List<TransferItemDetailBinM> TransferItemList { get; set; }

        public decimal RequestQuantity
        {
            get => Quantity; // inherent from WTQ1 object
            set
            {
                if (Quantity == value) return;
                Quantity = value;
                OnPropertyChanged(nameof(RequestQuantity));
            }
        }

        public string RequestFromWarehouse
        {
            get => FromWhsCod; // inherent from WTQ1 object
            set
            {
                if (FromWhsCod == value) return;
                FromWhsCod = value;
                OnPropertyChanged(nameof(RequestFromWarehouse));
            }
        }

        public string RequestToWarehouse
        {
            get => WhsCode; // inherent from WTQ1 object
            set
            {
                if (WhsCode == value) return;
                WhsCode = value;
                OnPropertyChanged(nameof(RequestToWarehouse));
            }
        }

        public string RequestedItemCode
        {
            get => ItemCode; // inherent from WTQ1 object
            set
            {
                if (ItemCode == value) return;
                ItemCode = value;
                OnPropertyChanged(nameof(RequestedItemCode));
            }
        }

        public string RequestedItemName
        {
            get => Dscription; // inherent from WTQ1
            set
            {
                if (Dscription == value) return;
                Dscription = value;
                OnPropertyChanged(nameof(RequestedItemName));
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
                OnPropertyChanged(nameof(TransferQty));
            }
        }

        Color cellCompleteColor;
        public Color CellCompleteColor
        {
            get => cellCompleteColor;
            set
            {
                if (cellCompleteColor == value) return;
                cellCompleteColor = value;
                OnPropertyChanged(nameof(CellCompleteColor));
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
                OnPropertyChanged(nameof(ShowList));
            }
        }
        #endregion
    }
}
