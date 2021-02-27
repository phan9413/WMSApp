using DbClass;
using System.ComponentModel;

namespace WMSApp.Models.SAP
{
    public class ReturnCommonHead_Ex : ODLN , INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string pname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pname));

        public string DocType { get; set; } // for ODLN / ORRR or 

        public string Text => $"{DocNum} . {CardName}";

        public string Details => $"{DocDate:dd-MM-yyyy} . {CardCode} . {GetDocStatus()}";

        string GetDocStatus()
        {
            switch (DocStatus)
            {
                case "O": return "Open";
                case "C": return "Closed";
            }
            return string.Empty;
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

        public ReturnCommonHead_Ex()
        {
        }
    }
}
