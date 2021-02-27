using DbClass;
using WMSApp.Class;
using WMSApp.Models.SAP;
using WMSApp.Models.Share;

namespace WMSApp.Dtos
{
    public class DTO_Request
    {
        public zwaRequest Request { get; set; }
        public zwaGRPO[] Grpo { get; set; }
        public zwaItemBin[] ItemBin { get; set; }
        public zmwDocHeaderField DocHeaderField { get; set; }
        public zwaTransferDocDetails[] TransferDocDetails { get; set; }
        public zwaTransferDocDetailsBin[] TransferDocDetailsBin { get; set; }
        public zwaTransferDocHeader TransferDocHeader { get; set; }
        public zwaHoldRequest HoldRequest { get; set; }
        public zwaInventoryRequest InventoryRequest { get; set; }
        public zwaInventoryRequestHead InventoryRequestHead { get; set; }
        public OCRD[] Bp { get; set; } = null;
    }
}
