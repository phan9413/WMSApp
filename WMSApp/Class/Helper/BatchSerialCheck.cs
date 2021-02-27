using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DbClass;
using Newtonsoft.Json;
using WMSApp.Models.Transfer1;

namespace WMSApp.Class.Helper
{
    public class BatchSerialCheck
    {
        readonly string controller = "BatchSerial";
        public BatchSerialCheck() { }

        public async Task<OSRI> IsSerialExist(string serialNum)
        {
            const string request = "IsSerialNumExist";
            string content = await IsDistNumExist(serialNum, request, controller);
            if (string.IsNullOrWhiteSpace(content)) return null;

            var orsi = JsonConvert.DeserializeObject<OSRI>(content);
            return orsi;
        }

        public async Task<OBTN> IsBatchExist(string batchNum)
        {
            const string request = "IsBatchNumExist";
            string content = await IsDistNumExist(batchNum, request, controller);
            if (string.IsNullOrWhiteSpace(content)) return null;

            var obtn = JsonConvert.DeserializeObject<OBTN>(content);
            return obtn;
        }

        async Task<string> IsDistNumExist(string distNumber, string request, string controller)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A moment ...");
                // query the item from server
                var requestBag = new Cio
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = request,                    
                    QueryDistNum = distNumber
                };

                // send over server to create request
                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(requestBag, controller);
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    DisplayAlert(content);
                    return null;
                }

                return content;
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
                return null;
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        async void DisplayAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "OK");
    }
}
