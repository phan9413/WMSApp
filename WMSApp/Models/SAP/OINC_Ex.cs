using DbClass;
namespace WMSApp.Models.SAP
{
    public class OINC_Ex : OINC
    {        
        public string Text => $"Doc# {DocNum} . {CountDate:dd/MM/yyyy}".ToUpper();        
        public string Details => $"{Remarks}\n{Ref2}".ToUpper();                    

    }
}
