using DbClass;
using System;
namespace WMSApp.Models.SAP
{
    public class OADM_Ex : OADM, IDisposable
    {
        public string TextDisplay
        {
            get
            {
                return CompnyName;
            }
        }

        public string DetailsDisplay
        {
            get
            {

                return E_Mail;
            }
        }

        public void Dispose()
        {
            //GC.Collect();
        }
    }
}
