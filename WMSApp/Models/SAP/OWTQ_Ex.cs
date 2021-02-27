using DbClass;

namespace WMSApp.Models.SAP
{
    public class OWTQ_Ex : OWTQ
    {
        public OWTQ RequestDocument { get; set; }
        public string Text
        {
            get => $"Transfer Request# {RequestDocument?.DocNum}";
        }

        public string Details
        {
            get
            {
                if (RequestDocument == null) return string.Empty;

                var details = $"{RequestDocument.DocDate:dd/MM/yyyy}\n";
                details += RequestDocument.DocStatus.Equals("O") ? "Open" : "Closed";
                return details;
            }
            
        }
    }
}
