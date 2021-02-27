using DbClass;
using System.ComponentModel;

namespace WMSApp.Models.SAP
{
    public class OPOR_Ex : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public OPOR PO { get; set; }
        public string Text => $"{PO?.DocNum} . {PO?.CardName}";
        public string Details => $"{PO?.DocDate:dd-MM-yyyy} . {PO?.CardCode} . {GetDocStatus()}";

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

        string GetDocStatus()
        {
            if (PO == null) return string.Empty;
            switch (PO.DocStatus)
            {
                case "O": return "Open";
                case "C": return "Closed";
                default: return string.Empty;
            }
        }

        int lineCount;
        public int LineCount
        {
            get => lineCount;
            set
            {
                if (lineCount == value) return;
                lineCount = value;
                OnPropertyChanged(nameof(LineCount));
            }
        }

        public OPOR_Ex()
        {

        }
    }
}