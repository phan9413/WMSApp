using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WMSApp.Interface;
using Xamarin.Forms;

namespace WMSApp.Class.Helper
{
    public class RequestCheckHelper
    {
        public zwaRequest Requests { get; set; } = null;
        public string ReturnAddress { get; set; } = string.Empty;
        int LoopLimit { get; set; }  = 180;
        int CurrentLoopCount { get; set; } = 0;
        bool IsStopTimmer { get; set; } = false;
        List<zwaRequest> RemoveList = new List<zwaRequest>();

        readonly string _FAIL = "fail";
        readonly string _SUCCESS = "success";
        readonly string _ONHOLD = "onhold";
        readonly string _ALERT_TITLE = "Alert";
        readonly string _ALERT_CONFIRMED = "OK";
        readonly string _CONTROLLER = "DocStatus";
        readonly string _CTRL_REQUEST = "CheckRequestDocStatus";
        readonly string _NEXT_VIEW = "RequestSummaryView";

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestCheckHelper() { }   

        /// <summary>
        /// Use by the app start checking on every 180 second
        /// </summary>
        public void StartCheckStatus()
        {
            if (App.RequestList == null)
            {
                App.RequestList = new List<zwaRequest>();
                return;
            }

            ClearRemoveList();

            App.RequestList.ForEach(x =>
            {
                Requests = x;
                CheckDocCreationStatus_Single(x);
            });

            // stop the timer
            App.IsContinueRequestChecking = (App.RequestList.Count > 0);           
        }

        public void ClearRemoveList()
        {
            if (RemoveList.Count > 0)
            {
                RemoveList.ForEach(x =>
                {
                    App.RequestList.Remove(x);
                });
                RemoveList.Clear();
            }
        }

        /// <summary>
        /// Task to check doc statuc and send nontification
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        async void CheckDocCreationStatus_Single(zwaRequest request)
        {
            try
            {
                var requestBag = new Cio
                {
                    sap_logon_name = App.waUser,
                    sap_logon_pw = App.waPw,
                    token = App.waToken,
                    phoneRegID = App.waToken,
                    request = _CTRL_REQUEST,
                    checkDocGuid = $"{request.guid}"
                };

                // send over server to create request
                string content, lastErrMessage;
                bool isSuccess = false;

                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(requestBag, _CONTROLLER);
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    RemoveList.Add(request);                    

                    ShowAlert($"{content}\n{lastErrMessage}");                    
                    UserDialogs.Instance.HideLoading();
                    return;
                }

                var repliedRequest = JsonConvert.DeserializeObject<zwaRequest>(content);
                if (repliedRequest == null) // may be yet update
                {                    
                    return;
                }

                if (!string.IsNullOrWhiteSpace(repliedRequest.lastErrorMessage))
                {
                    var json = GetJson(repliedRequest);
                    var message1 = $"{repliedRequest.request}\n{repliedRequest.lastErrorMessage}";

                    SendNotification("Error", message1, _NEXT_VIEW, json);                    
                    ShowAlert("Error", message1, "OK");

                    RemoveList.Add(request);
                    UserDialogs.Instance.HideLoading();
                    return;
                }

                // handler posting success
                if (repliedRequest.status.ToLower().Equals(_SUCCESS))
                {
                    var json = GetJson(repliedRequest);
                    var message = $"{repliedRequest.request} #{repliedRequest.sapDocNumber}";
                    
                    SendNotification("Success", message, _NEXT_VIEW, json);
                    ShowAlert("Success", message, "OK");

                    MessagingCenter.Send("", Requests.popScreenAddress);

                    UserDialogs.Instance.HideLoading();
                    RemoveList.Add(request);                               
                }
            }
            catch (Exception excep)
            {
                ShowAlert($"{excep}");
                IsStopTimmer = true;
            }
        }

        string GetJson(zwaRequest request) => JsonConvert.SerializeObject(request);

        async void ShowAlert(string message) => await new Dialog().DisplayAlert(_ALERT_TITLE, message, _ALERT_CONFIRMED);

        async void ShowAlert(string title, string message, string confirmButton) => await new Dialog().DisplayAlert(title, message, confirmButton);

        void SendNotification(string title, string message, string nextView, string data) => 
            DependencyService.Get<INotificationManager>().SendNotification(title, message, nextView, data);
    }
}

//public void StartChecking()
//{
//    Device.StartTimer(TimeSpan.FromSeconds(1), () =>
//    {
//        if (CurrentLoopCount == LoopLimit) // longer loop 180 times / 3 minute
//        {
//            IsStopTimmer = true;
//            ShowAlert($"There is issue reading the replied for {Requests.request} from server.\n" +
//                "please double check the system on doc creation.\nBefore try to post from App again, Thanks.");                    
//        }

//        if (IsStopTimmer)
//        {                    
//            RepliedMessage(_FAIL);
//            return false; // stop the timer
//        }

//        CheckDocCreationStatus();
//        return true; // return true to continue
//    });
//}


//async void CheckDocCreationStatus()
//{
//    if (this.Requests == null)
//    {
//        CurrentLoopCount = LoopLimit;
//        return;
//    }

//    try
//    {               
//        var requestBag = new Cio
//        {
//            sap_logon_name = App.waUser,
//            sap_logon_pw = App.waPw,
//            token = App.waToken,
//            phoneRegID = App.waToken,
//            request = _CTRL_REQUEST,
//            checkDocGuid = $"{this.Requests.guid}"
//        };

//        // send over server to create request
//        string content, lastErrMessage;
//        bool isSuccess = false;

//        using (var httpClient = new HttpClientWapi())
//        {
//            content = await httpClient.RequestSvrAsync_mgt(requestBag, _CONTROLLER);
//            isSuccess = httpClient.isSuccessStatusCode;
//            lastErrMessage = httpClient.lastErrorDesc;
//        }

//        if (!isSuccess)
//        {
//            ShowAlert($"{content}\n{lastErrMessage}");
//            CurrentLoopCount++;
//            return;
//        }

//        var repliedRequest = JsonConvert.DeserializeObject<zwaRequest>(content);
//        if (repliedRequest == null) // may be yet update
//        {
//            CurrentLoopCount++;
//            return;
//        }

//        //if (!string.IsNullOrWhiteSpace(repliedRequest.lastErrorMessage))
//        //{                   
//        //    CurrentLoopCount++; // increase the check loop
//        //    return;
//        //}

//        // handler posting fail
//        if (repliedRequest.status.ToLower().Equals(_ONHOLD) && 
//            !string.IsNullOrWhiteSpace(repliedRequest.lastErrorMessage) && 
//            repliedRequest.tried >= 3)
//        {
//            var message = $"{repliedRequest.request}\n{repliedRequest.lastErrorMessage}";
//            ShowAlert(message);
//            IsStopTimmer = true;
//            RepliedMessage(_FAIL);
//            return;
//        }

//        // handler posting success
//        if (repliedRequest.status.ToLower().Equals(_SUCCESS))
//        {
//            var message = $"{repliedRequest.request} #{repliedRequest.sapDocNumber}\nCreated successfully";
//            ShowAlert(message);
//            IsStopTimmer = true;
//            RepliedMessage(_SUCCESS);

//        }
//    }
//    catch (Exception excep)
//    {
//        ShowAlert($"{excep}");
//        IsStopTimmer = true;
//    }
//}

//void RepliedMessage(string message) => MessagingCenter.Send(message, ReturnAddress);
