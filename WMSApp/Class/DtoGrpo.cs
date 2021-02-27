using DbClass;
using WMSApp.Models.SAP;

namespace WMSApp.Class
{
    public class DtoGrpo
    {
        public OPOR[] OPORs { get; set; }
        public POR1[] POR1s { get; set; }
        public OPOR_Ex[] OPOR_Exs { get; set; }
        public POR1_Ex[] POR1_Exs { get; set; }
        public NNM1[] GrpoDocSeries { get; set; }
        public OCRD[] Vendors { get; set; }
    }
}
