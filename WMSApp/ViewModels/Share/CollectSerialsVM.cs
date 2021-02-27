using System.ComponentModel;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using WMSApp.Interface;
using ZXing;
using System;
using WMSApp.Class;
using WMSApp.Class.Helper;

namespace WMSApp.ViewModels.Share
{
    public class CollectSerialsVM : INotifyPropertyChanged
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
                if (status != value)
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
                if (selectedItem != value)
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
                if (!bool.Equals(isTorchOn, value))
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
                if (qrcode != value)
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
                if (!bool.Equals(isAnalyzing, value))
                {
                    isAnalyzing = value;
                    OnPropertyChanged(nameof(IsAnalyzing));
                }
            }
        }

        bool isScanning = true;
        public bool IsScanning
        {
            get => isScanning;
            set
            {
                if (!bool.Equals(isScanning, value))
                {
                    isScanning = value;
                    OnPropertyChanged(nameof(IsScanning));
                }
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

        List<string> qrCodeList = new List<string>();
        public ObservableCollection<string> QRCodeList { get; set; }

        INavigation navigation;
        string returnAddress;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Navigation"></param>
        /// <param name="ReturnAddress"></param>
        public CollectSerialsVM(INavigation Navigation, string ReturnAddress)
        {
            navigation = Navigation;
            returnAddress = ReturnAddress;
            qrCodeList = new List<string>();
            RefreshListView();

            OnQRCodeScannerCmd = new Command(OnQRScanner);
            CancelCmd = new Command(Cancel);
            SaveCmd = new Command(Save);
            TorchCmd = new Command(TorchOn);
            DeleteItemCmd = new Command<string>(DeleteItem);
            CmdAdd = new Command(PromtpEntry);

            IsCameraExpaneded = true;
        }

        /// <summary>
        /// Prompt manual entry
        /// </summary>
        async void PromtpEntry()
        {
            try
            {
                IsCameraExpaneded = false;
                var input = await new Dialog().DisplayPromptAsync("Input serial#", "", "OK", "Cancel",
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
            MessagingCenter.Send<List<string>>(qrCodeList, returnAddress);
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
            MessagingCenter.Send<List<string>>(qrCodeList, returnAddress);
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
                    isAnalyzing = false;
                    HandlerInput(Result.Text);
                    IsAnalyzing = true;
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
                //await Application.Current.MainPage.DisplayAlert("Attention",
                //    "The value cannot be null. Please scan again.", "Ok");

                DisplayToast($"The value cannot be null.Please scan again.");
                IsAnalyzing = true;
                return;
            }

            if (CheckDuplicate(qrcode))
            {
                Status = "Warning";
                DisplayToast($"The input {qrcode} found duplicated. Please try again.");
            }
            else
            {
                // PlaySoundAndVibrate();
                //CodeFormat = (result.BarcodeFormat.Equals(ZXing.BarcodeFormat.All_1D)) ?
                //        "barcode_back512.png" : "qrcode_black512.png";

                Status = "Success";
                if (qrCodeList == null) qrCodeList = new List<string>();
                qrCodeList.Insert(0, qrcode);
                RefreshListView();
                DisplayToast($"{qrcode} Added.");
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

            var foundItem = qrCodeList.Where(x => x.Equals(QRcode)).FirstOrDefault();
            if (foundItem == null) return false;
            return true;

            //return true;

            //bool foundDup = false;
            //foreach (var value in QRCodeList)
            //{
            //    if (QRcode == value)
            //    {
            //        foundDup = true;
            //    }
            //}
            //Console.WriteLine(foundDup);
            //return foundDup;
        }

        /// <summary>
        /// Detele the item from list
        /// </summary>
        /// <param name="selected"></param>
        public void DeleteItem(string selected)
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
                QRCodeList = new ObservableCollection<string>(qrCodeList);
                OnPropertyChanged(nameof(QRCodeList));
                return;
            }

            QRCodeList = new ObservableCollection<string>(qrCodeList);
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
