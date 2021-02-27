using WMSApp.Models.Setup.Group;
using DbClass;
using WMSApp.Models.Login;
using WMSApp.Models.SAP;
using System;
using WMSApp.Models.Inventory;
using WMSApp.Models.Share;
using WMSApp.Models.Transfer1;
using WMSApp.ViewModels.Transfer1;

namespace WMSApp.Class
{
    public class Cio // Use to handle information transfer between app and server, work like a bag
    {
        public string request { get; set; }
        public string token { get; set; }
        public string sap_logon_name { get; set; }
        public string sap_logon_pw { get; set; }
        public string phoneRegID { get; set; }
        public string companyName { get; set; }
        public int companyId { get; set; }
        public zwaUser newzwaUser { get; set; }
        public string currentUser { get; set; }
        public string currentUserRole { get; set; } // added 20200626 for indentity admin group         
        public zwaUserGroup1[] currentPermissions { get; set; } // ----------------- current user permission reference, infor from server 
        public zwaUserGroup currentGroup { get; set; }        
        //public OPOR[] Po { get; set; }
        //public POR1[] PoLines { get; set; }     
        public string getPoType { get; set; } //<--- aded by johnny on 20200614        
        // 20200615 
        public zwaRequest dtoRequest { get; set; } // data transfer object for request create ERP doc      
        public zwaGRPO[] dtoGRPO { get; set; } // data transfer object for request create ERP doc
        // 20200616
     //   public ORDR_Ex[] So { get; set; }
        public string getSoType { get; set; } //<--- aded by johnny on 20200616
        //public RDR1_Ex[] SoLines { get; set; }
        //public int soDocEntry { get; set; }
      //  public zwaGRPO[] dtoDeliveryOrder { get; set; }
        // 20200616T0946
        public OWHS[] dtoWhs { get; set; } // for checking the item code whs qty
        // 20200616T0946        
        public zmwRequest dtoDocStatus { get; set; } // for replied the check of the doc status
        public string checkDocGuid { get; set; } // use by App to check the guid creation
        // 20200618T0927
        public OITM[] Items { get; set; } // get list if the items from database
        public string QueryItemCode { get; set; } // get single item of the item code
        public OITM Item { get; set; } // get list if the items from database
        // 20200619
        public OINC_Ex[] InvenCountDocs { get; set; } // return list of the inventory counting open doc
        public string OINCStatus { get; set; } //<--- aded by johnny on 20200616
        public INC1_Ex[] InvenCountDocsLines { get; set; } // return list of the inventory counting open doc lines
        public int OINCDocEntry { get; set; } /// use to query the oinc doc line, based doc entry
        //public OBIN[] LocationBin { get; set; } // return list of the inventory counting open doc lines
        // 20200621T1950
        // add in the user and user group into the app
        public zwaUser[] zwAppUsers { get; set; }
        public OADM_Ex[] oADM_CompanyInfoList { get; set; }
        public zwaUserGroup[] zwaGroupList { get; set; }
        public int newUserGroupTempId { get; set; }
        public zwaUserGroup newUserGroup { get; set; }
        public zwaUserGroup1[] newUserGroupPermission { get; set; }
        public zwaUser[] newUserGroupUsr { get; set; }
        public int groupId { get; set; }
        public zwaUserGroup1[] zwaUserGroupsPermission { get; set; }
        // 20200624T1046
        public OPLN[] PriceList { get; set; } // for app price list selection
        public int ExistingGrPriceListId { get; set; } // for keeping the exiting prices list setup
        public int UpdateGrPriceListId { get; set; } // for keeping the exiting prices list setup
        public string UpdateGrDocSeries { get; set; } /// use to update the goods receipt doc series
        public string UpdateIssueDocSeries { get; set; } // use to update good issue doc series
        public string ExistingGrDocSeries { get; set; }
        public string ExistingGIDocSeries { get; set; }
        public int ExistingGiPriceListId { get; set; } // for keeping the exiting prices list setup // 202006282330
        public int UpdateGiPriceListId { get; set; } // for keeping the exiting prices list setup // 20200628T2330

        // 20200718T1023
        public OBIN[] DtoBins { get; set; } // for query the bins location for warehouse
        public string QueryWhs { get; set; }
        // 20200719T1037
        public zwaItemBin[] dtoItemBins { get; set; }
        // 20200920T1321
        public OWTQ[] TransferRequestList { get; set; }
        public WTQ1[] TransferRequestLine { get; set; }
        public int TransferRequestDocEntry { get; set; }
        // 20200921T1706
        public zwaInventoryRequest[] dtoInventoryRequest { get; set; }
        public zwaInventoryRequestHead dtoInventoryRequestHead { get; set; }
        // 20200923T2044
        //public OWTQ[] RequestTransferDoc { get; set; }
        public string RequestTransferDocFilter { get; set; } // for filter all, open and close
        public DateTime RequestTransferStartDt { get; set; }
        public DateTime RequestTransferEndDt { get; set; }
        // 20200924T1147
        public string QueryItemWhsCode { get; set; }
        public OITW oITW { get; set; }
        public FTS_vw_IMApp_ItemWhsBin[] ItemWhsBinList { get; set; }
        // 20200927
        public BinContent[] dtoBinContents { get; set; }
        public StockTransactionLog[] dtoStockTransLogs { get; set; }
        public DateTime QueryStartDate { get; set; }
        public DateTime QueryEndDate { get; set; }
        public CommonStockInfo[] CommonStockInfos { get; set; } // 20201002
        // 20201011 for transfer 2 
        public string TransferItemCode { get; set; }
        public string TransferQueryCode { get; set; }
        public string TransferWhsCode { get; set; }
        public OITM TransferFoundItem { get; set; }
        public OBIN TransferFoundBin { get; set; }
        public OSRN TransferFoundSerial { get; set; }
        public OBTN TransferFoundBatch { get; set; }
        public OSBQ TransferBinSerialAccumulator { get; set; }
        public OBBQ TransferBinBatchAccumulator { get; set; }
        public OIBQ TransferBinAccumulator { get; set; }
        public OSRI TransferOSRI { get; set; } // 20201016T1428
        public string TransSerialCode { get; set; }
        public OBTQ TransferBatch { get; set; } // 20201016T1633
        public int TransBatchAbs { get; set; }
        public OSBQ_Ex[] TransferBinContentSerial { get; set; } // 20201018
        public OSRQ_Ex[] TransferContentSerial { get; set; }
        public OBBQ_Ex[] TransferBinContentBatch { get; set; } // 20201019T
        public OBTQ_Ex[] TransferContentBatch { get; set; } // 20201018T1139
        public OIBQ_Ex[] TransferBinItems { get; set; } // 20201019T1946      
        public zwaHoldRequest TransferHoldRequest { get; set; }
        public zwaTransferDocHeader TransferDocHeader { get; set; }
        public zwaTransferDocDetails[] TransferDocDetails { get; set; }
        public zwaTransferDocDetailsBin[] TransferDocDetailsBins { get; set; }
        public OBIN[] WarehouseBin { get; set; }       
        public int STAHoldRequestId { get; set; } // 20201031T1547 return the inserted row id for show in app
        public zwaHoldRequest[] dtozwaHoldRequests { get; set; } // 20201031T1909
        public zwaTransferDocDetails[] dtozmwTransferDocDetails { get; set; }// 20201101T1111
        public zwaTransferDocDetailsBin[] dtozwaTransferDocDetailsBin { get; set; } // 20201101T1111
        public Guid TransferDocRequestGuid { get; set; } // 2020111906
        public Guid TransferDocRequestGuidLine {get; set;}
        public int[] PoDocEntries { get; set; }
        public zmwDocHeaderField dtozmwDocHeaderField { get; set; }
        public int QueryDocEntry { get; set; }
        public int QueryDocLineNum { get; set; }
        public string QueryDistNum { get; set; }
        public Cio() { }
    }
}
