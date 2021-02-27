using Acr.UserDialogs;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Extensions;
using System;
using System.ComponentModel;
using System.Dynamic;
using WMSApp.Class;
using WMSApp.Dtos;
using WMSApp.Interface;
using Xamarin.Forms;

namespace WMSApp.ViewModels.RequestSummary
{
    public class RequestSummaryVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Command CmdAction { get; set; }
        public Command CmdShowDetails { get; set; }
        string _Picture { get; set; } = string.Empty;
        public string Picture
        {
            get => _Picture;
            set
            {
                if (_Picture == value) return;
                _Picture = value;
                OnPropertyChanged(nameof(Picture));
            }
        }

        string _Result { get; set; } = string.Empty;
        public string Result
        {
            get => _Result;
            set
            {
                if (_Result == value) return;
                _Result = value;
                OnPropertyChanged(nameof(Result));
            }
        }

        string _Message { get; set; } = string.Empty;
        public string Message
        {
            get => _Message;
            set
            {
                if (_Message == value) return;
                _Message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        bool _IsOKVisible { get; set; } = false;
        public bool IsOKVisible
        {
            get => _IsOKVisible;
            set
            {
                if (_IsOKVisible == value) return;
                _IsOKVisible = value;
                OnPropertyChanged(nameof(IsOKVisible));
            }
        }

        bool _IsResetVisible { get; set; } = false;
        public bool IsResetVisible
        {
            get => _IsResetVisible;
            set
            {
                if (_IsResetVisible == value) return;
                _IsResetVisible = value;
                OnPropertyChanged(nameof(IsResetVisible));
            }
        }

        INavigation Navigation { get; set; } = null;
        zwaRequest Request { get; set; } = null;

        /// <summary>
        /// constructor
        /// </summary>
        public RequestSummaryVM(INavigation navigation, zwaRequest request)
        {
            Request = request;
            bool isSuccess = (Request.status.ToLower().Equals("success"));
            if (isSuccess)
            {
                _Message = $"{Request.request} Doc# {Request.sapDocNumber} successfully.";
            }
            else
            {
                _Message = $"{Request.request}\n{Request.lastErrorMessage}\n\n" +
                    $"Please correct the issue, and press reset to try post again, Thanks. ";
            }

            _Picture = isSuccess ? "tick512_green" : "cross512_reb";
            _Result = isSuccess ? "Success" : "Fail";
            _IsOKVisible = isSuccess;
            _IsResetVisible = !_IsOKVisible;
            Navigation = navigation;


        }

        void InitCmd()
        {
            CmdAction = new Command<string>((string action) =>
            {
                switch (action)
                {
                    case "close":
                        {
                            Navigation.PopPopupAsync();
                            break;
                        }
                    case "ok":
                        {
                            Navigation.PopPopupAsync();
                            break;
                        }
                    case "reset":
                        {
                            ResetRequest();
                            Navigation.PopPopupAsync();
                            break;
                        }
                }
            });

            CmdShowDetails = new Command(HandlerShowDetails);
        }

        async void ResetRequest()
        {
            try
            {
                //var requestBag = new Cio
                //{
                //    sap_logon_name = App.waUser,
                //    sap_logon_pw = App.waPw,
                //    token = App.waToken,
                //    phoneRegID = App.waToken,
                //    request = "ResetRequestTried",
                //    checkDocGuid = $"{Request.guid}"
                //};

                dynamic request_ = new ExpandoObject();
                request_.request = "ResetRequestTried";
                request_.checkDocGuid = $"{Request.guid}";

                // send over server to create request
                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(request_, "DocStatus");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert($"{content}\n{lastErrMessage}");
                    return;
                }

                // activate the checking
                App.RequestList.Add(Request);
                App.IsContinueRequestChecking = true;

                ShowToast("Request reset, posting validation restarted.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                ShowAlert(Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        void HandlerShowDetails()
        {
            LoadRequestDetails();
        }

        async void LoadRequestDetails()
        {
            try
            {
                dynamic request_ = new ExpandoObject();
                request_.request = "GetRequestDetails";
                request_.checkDocGuid = $"{Request.guid}";

                // send over server to create request
                string content, lastErrMessage;
                bool isSuccess = false;
                using (var httpClient = new HttpClientWapi())
                {
                    content = await httpClient.RequestSvrAsync_mgt(request_, "DocStatus");
                    isSuccess = httpClient.isSuccessStatusCode;
                    lastErrMessage = httpClient.lastErrorDesc;
                }

                if (!isSuccess)
                {
                    ShowAlert($"{content}\n{lastErrMessage}");
                    return;
                }

                var requestDetails = JsonConvert.DeserializeObject<DTO_Request>(content);
                if (requestDetails == null) return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                ShowAlert(Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        async void ShowAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "Ok");

        void ShowToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);

    }
}
