using DbClass;
using System.ComponentModel;

namespace WMSApp.Models.SAP
{
    public class OPRR_Ex : OPRR, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public OPRR PRR { get; set; }

        public string Text => $"Request# {DocNum} . {CardName}";
        public string Details => $"{DocDate:dd-MM-yyyy} . {CardCode} . {GetDocStatus()}";

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
            switch (DocStatus)
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

        public OPRR_Ex()
        {

        }
    }
}
