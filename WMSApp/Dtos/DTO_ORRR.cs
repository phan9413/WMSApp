using DbClass;
using WMSApp.Models.SAP;

namespace WMSApp.Dtos
{
    public class DTO_ORRR
    {
        public ORRR[] ORRRs { get; set; }
        public RRR1[] RRR1ss { get; set; }
        public ORRR_Ex[] ORRR_Exs { get; set; }
        public RRR1_Ex[] RRR1_Exs { get; set; }
        public NNM1[] DocSeries { get; set; }
        public OCRD[] Bp { get; set; }

        public ODLN_Ex[] ODLN_Exs { get; set; }
        public DLN1_Ex[] DLN1_Exs { get; set; }
        public OINV_Ex[] OINV_Exs { get; set; } // for ar invoice
        public INV1_Ex[] INV1_Exs { get; set; } // for ar invoice line
    }
}
