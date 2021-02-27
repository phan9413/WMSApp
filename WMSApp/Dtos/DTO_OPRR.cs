using DbClass;
using WMSApp.Models.SAP;

namespace WMSApp.Dtos
{
    public class DTO_OPRR
    {
        public OPRR[] OPRRs { get; set; }
        public PRR1[] PRR1s { get; set; }
        public OPRR_Ex[] OPRR_Exs { get; set; }
        public PRR1_Ex[] PRR1_Exs { get; set; }
        public NNM1[] DocSeries { get; set; }
        public OCRD[] Bp { get; set; }
        public OPDN_Ex[] OPDNs { get; set; } // grood receipt PO / GRN head 
        public PDN1_Ex[] PDN1s { get; set; } // grood receipt PO / GRN line
        public OPCH_Ex[] OPCH_Exs { get; set; } // AP invoice 
        public PCH1_Ex[] PCH1_Exs { get; set; } // AP invoice line

    }
}
