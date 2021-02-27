using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.GRPO;
using WMSApp.Views.Share;
using Xamarin.Forms;
using ZXing;

namespace WMSApp.ViewModels.Share
{
    public class CollectBatchesVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public Command OnQRCodeScannerCmd { get; set; }
        public Command CancelCmd { get; set; }
        public Command SaveCmd { get; set; }
        public Command TorchCmd { get; set; }
        public Command DeleteItemCmd { get; set; }
        public Command CmdAdd { get; set; }

        string status;
        public string Status
        {
            get => status;
            set
            {
                if (status == value) return;
                status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        string selectedItem;
        public string SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                OnPropertyChanged(SelectedItem);
            }
        }

        bool isTorchOn;
        public bool IsTorchOn
        {
            get => isTorchOn;
            set
            {
                if (isTorchOn == value) return;
                isTorchOn = value;
                OnPropertyChanged(nameof(IsTorchOn));
            }
        }

        Result result;
        public Result Result
        {
            get => result;
            set
            {
                if (result == value) return;
                result = value;
                OnPropertyChanged(nameof(Result));
            }
        }

        string qrcode;
        public string QRcode
        {
            get => qrcode;
            set
            {
                if (qrcode == value) return;
                qrcode = value;
                OnPropertyChanged(nameof(QRcode));
            }
        }

        bool isAnalyzing = true;
        public bool IsAnalyzing
        {
            get => isAnalyzing;
            set
            {
                if (isAnalyzing == value) return;
                isAnalyzing = value;
                OnPropertyChanged(nameof(IsAnalyzing));
            }
        }

        bool isScanning = true;
        public bool IsScanning
        {
            get => isScanning;
            set
            {
                if (isScanning == value) return;
                isScanning = value;
                OnPropertyChanged(nameof(IsScanning));
            }
        }

        bool isCameraExpaneded;
        public bool IsCameraExpaneded
        {
            get => isCameraExpaneded; 
            set
            {
                if (isCameraExpaneded == value) return;
                isCameraExpaneded = value;
                OnPropertyChanged(nameof(IsCameraExpaneded));
            }
        }

        bool isShowOtherProperty;
        public bool IsShowOtherProperty
        {
            get => isShowOtherProperty;
            set
            {
                if (isShowOtherProperty == value) return;
                isShowOtherProperty = value;
                //OnPropertyChanged(nameof(IsShowOtherProperty));
            }
        }

        List<Batch> qrCodeList { get; set; } = new List<Batch>();
        public ObservableCollection<Batch> QRCodeList { get; set; }

        INavigation navigation;       
        string returnAddress;
        readonly string _CollectBatchPopUpView = "_CollectBatchPopUpView20201009T1600";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Navigation"></param>
        /// <param name="ReturnAddress"></param>
        public CollectBatchesVM(INavigation Navigation, string ReturnAddress)
        {
            navigation = Navigation;
            returnAddress = ReturnAddress;
            qrCodeList = new List<Batch>();
            RefreshListView();

            OnQRCodeScannerCmd = new Command(OnQRScanner);
            CancelCmd = new Command(Cancel);
            SaveCmd = new Command(Save);
            TorchCmd = new Command(TorchOn);
            DeleteItemCmd = new Command<Batch>((Batch selected) => DeleteItem(selected));
            CmdAdd = new Command(PromtpEntry);
            IsAnalyzing = true;
            IsCameraExpaneded = true;
            IsShowOtherProperty = true;
        }

        /// <summary>
        /// Use to intial the batch + qty only no need to show attribute 
        /// </summary>
        /// <param name="Navigation"></param>
        /// <param name="ReturnAddress"></param>
        /// <param name="isCollectBatchQty"></param>
        public CollectBatchesVM(INavigation Navigation, string ReturnAddress, bool isCollectBatchQty)
        {
            navigation = Navigation;
            returnAddress = ReturnAddress;
            qrCodeList = new List<Batch>();
            RefreshListView();

            OnQRCodeScannerCmd = new Command(OnQRScanner);
            CancelCmd = new Command(Cancel);
            SaveCmd = new Command(Save);
            TorchCmd = new Command(TorchOn);
            DeleteItemCmd = new Command<Batch>((Batch selected) => DeleteItem(selected));
            CmdAdd = new Command(PromtpEntry);
            IsAnalyzing = true;
            IsCameraExpaneded = true;
            IsShowOtherProperty = isCollectBatchQty;
        }

        /// <summary>
        /// Prompt manual entry
        /// </summary>
        async void PromtpEntry()
        {
            try
            {
                IsCameraExpaneded = false; // collapse the camera view
                var input = await new Dialog().DisplayPromptAsync("Input batch#", "", "OK", "Cancel",
                    null, -1, Keyboard.Default, "");

                if (string.IsNullOrWhiteSpace(input)) return;
                if (input.ToLower().Equals("cancel")) return;

                HandlerInput(input);
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                DisplayAlert(excep.Message);
            }
        }

        /// <summary>
        /// Cancel the screen
        /// </summary>
        public async void Cancel()
        {
            Close();
            qrCodeList.Clear();
            MessagingCenter.Send<List<Batch>>(qrCodeList, returnAddress);
        }

        void Close() => navigation.PopAsync();
            
        /// <summary>
        /// Save the list
        /// </summary>
        async void Save()
        {
            if (qrCodeList == null) return;
            if (qrCodeList.Count == 0) return;

            Close();
            MessagingCenter.Send(qrCodeList, returnAddress);
        }

        /// <summary>
        /// When scan star
        /// </summary>
        void OnQRScanner()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (isAnalyzing)
                {
                    IsAnalyzing = false;
                    HandlerInput(Result.Text);                    
                }
            });
        }

        /// <summary>
        /// handle the captured value, and share code with manual input
        /// </summary>
        /// <param name="code"></param>
        async void HandlerInput(string code)
        {
            qrcode = code.ToUpper();            
            if (string.IsNullOrWhiteSpace(qrcode))
            {
                Status = "Warning";                
                DisplayToast($"The value cannot be null.Please scan again.");
                IsAnalyzing = true;
                return;
            }

            if (CheckDuplicate(qrcode))
            {
                Status = "Warning";
                DisplayToast($"The input {qrcode} found duplicated. Please try again.");
                IsAnalyzing = true;
            }
            else
            {
                // PlaySoundAndVibrate();

                if (qrCodeList == null) qrCodeList = new List<Batch>();                
                if (isShowOtherProperty)
                {
                    // show the batch entry screen and return the batch
                    MessagingCenter.Subscribe(this, _CollectBatchPopUpView, (Batch batch) =>
                    {
                        MessagingCenter.Unsubscribe<Batch>(this, _CollectBatchPopUpView);
                        if (string.IsNullOrWhiteSpace(batch.DistNumber))
                        {
                            IsAnalyzing = true;
                            return;
                        }

                        if (CheckDuplicate(batch.DistNumber))
                        {
                            DisplayToast($"The input {qrcode} found duplicated. Please try again.");
                            IsAnalyzing = true;
                            return;
                        }

                        batch.IsShowOtherProperty = this.isShowOtherProperty;

                        Status = "Success";
                        qrCodeList.Insert(0, batch);
                        RefreshListView();
                        DisplayToast($"{batch.DistNumber} Added.");
                        IsAnalyzing = true;
                    });
                    await navigation.PushPopupAsync(new CollectBatchPopUpView(qrcode, _CollectBatchPopUpView));
                    return;
                }

                // prompt
                string inputQty = await new Dialog().DisplayPromptAsync(
                    $"Input qty {qrcode} #", "", "OK", "Cancel", null, -1, Keyboard.Numeric, "1");

                if (string.IsNullOrWhiteSpace(inputQty)) return;

                bool isnumeric = decimal.TryParse(inputQty, out decimal result);
                if (!isnumeric)
                {
                    DisplayToast($"The {qrcode} inputted qty {result} is invalid, please try again. Thanks");
                    return;
                }

                if (result < 0)
                {
                    DisplayToast($"The {qrcode} inputted qty {result} can not be negative or zero, please try again. Thanks");
                    return;
                }

                var batch1 = new Batch { DistNumber = qrcode, Qty = result , IsShowOtherProperty = this.isShowOtherProperty};
                if (CheckDuplicate(batch1.DistNumber))
                {
                    DisplayToast($"The input {qrcode} found duplicated. Please try again.");
                    IsAnalyzing = true;
                    return;
                }

                Status = "Success";
                qrCodeList.Insert(0, batch1);
                RefreshListView();
                DisplayToast($"{batch1.DistNumber} Added.");
                IsAnalyzing = true;
            }
        }

        /// <summary>
        /// Get the file stream
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        //Stream GetStreamFromFile(string filename)
        //{
        //    var assembly = typeof(App).GetTypeInfo().Assembly;
        //    var stream = assembly.GetManifestResourceStream("Scanner." + filename);
        //    return stream;
        //}

        /// <summary>
        /// Play sound and vibrate
        /// </summary>
        //private void PlaySoundAndVibrate()
        //{
        //    try
        //    {
        //        //Sound
        //        //var stream = GetStreamFromFile("ScannerBeep.mp3");
        //        //var audio = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
        //        //audio.Load(stream);
        //        //audio.Play();

        //        //Vibration
        //       // var duration = TimeSpan.FromSeconds(0.3);
        //       // Vibration.Vibrate(duration);
        //    }
        //    catch (Exception excep)
        //    {
        //        Console.WriteLine(excep.ToString());
        //    }
        //}

        /// <summary>
        /// Check list duplication
        /// </summary>
        /// <param name="QRcode"></param>
        /// <returns></returns>
        bool CheckDuplicate(string QRcode)
        {
            if (qrCodeList == null) return false;
            if (qrCodeList.Count == 0) return false;

            var lowerCase = QRcode.ToLower();
            var foundItem = qrCodeList.Where(x => x.DistNumber.ToLower().Equals(lowerCase)).FirstOrDefault();
            if (foundItem == null) return false;
            return true;
        }

        /// <summary>
        /// Detele the item from list
        /// </summary>
        /// <param name="selected"></param>
        public void DeleteItem(Batch selected)
        {
            if (qrCodeList == null)
            {
                RefreshListView();
                return;
            }

            if (qrCodeList.Count == 0)
            {
                RefreshListView();
                return;
            }

            qrCodeList.Remove(selected);
            RefreshListView();
        }

        /// <summary>
        /// Refresh and binding the listview
        /// </summary>
        void RefreshListView()
        {
            if (qrCodeList == null) return;
            if (qrCodeList.Count == 0)
            {
                QRCodeList = new ObservableCollection<Batch>(qrCodeList);
                OnPropertyChanged(nameof(QRCodeList));
                return;
            }

            QRCodeList = new ObservableCollection<Batch>(qrCodeList);
            OnPropertyChanged(nameof(QRCodeList));
        }

        /// <summary>
        /// Set camera on and off
        /// </summary>
        void TorchOn() => IsTorchOn = !IsTorchOn;

        void DisplayToast(string message) => DependencyService.Get<IToastMessage>()?.ShortAlert(message);

        async void DisplayAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "OK");

    }
}
