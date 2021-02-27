using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Threading.Tasks;
using WMSApp.Models.GRPO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.Share
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CollectBatchPopUpView : PopupPage
    {
        public string DisNumber { get; set; }
        public string Attribute1 { get; set; }
        public string Attribute2 { get; set; }
        public DateTime AdmissionDate { get; set; }
        public decimal Qty { get; set; } = 1;
        public DateTime ExpiredDate { get; set; }
        string returnAddress { get; set; }

        //public Command  CmdSave { get; set; }
        //public Command CmdCancel { get; set; }

        /// <summary>
        /// Use direct binding model
        /// </summary>
        public CollectBatchPopUpView(string distNum, string callerAddr)
        {
            InitializeComponent();
            DisNumber = distNum.ToUpper();
            returnAddress = callerAddr;
            AdmissionDate = DateTime.Now;
            ExpiredDate = DateTime.Now.AddMonths(1);
            BindingContext = this;
        }

        /// <summary>
        /// Peform the check and send bacth the object
        /// </summary>
        void Save ()
        {
            // validattion check 
            if (Qty <= 0)
            {
                DisplayAlert("Alert", "Invalid qty input, please try again. Thanks", "OK");
                return;
            }

            Navigation.PopPopupAsync();
            var newEmptyBatch = new Batch
            {
                DistNumber = this.DisNumber,
                Attribute1 = this.Attribute1,
                Attribute2 = this.Attribute2,
                Admissiondate = this.AdmissionDate,
                Expireddate = this.ExpiredDate,
                Qty = this.Qty
            };
            MessagingCenter.Send(newEmptyBatch, returnAddress);
        }

        void Cancel ()
        {
            Navigation.PopPopupAsync();
            var newEmptyBatch = new Batch { DistNumber = string.Empty };            
            MessagingCenter.Send(newEmptyBatch, returnAddress);
        }

        protected override void OnAppearing()
        {            
            base.OnAppearing();
            EntryQty.Focus();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        // ### Methods for supporting animations in your popup page ###

        // Invoked before an animation appearing
        protected override void OnAppearingAnimationBegin()
        {
            base.OnAppearingAnimationBegin();
        }

        // Invoked after an animation appearing
        protected override void OnAppearingAnimationEnd()
        {
            base.OnAppearingAnimationEnd();
        }

        // Invoked before an animation disappearing
        protected override void OnDisappearingAnimationBegin()
        {
            base.OnDisappearingAnimationBegin();
        }

        // Invoked after an animation disappearing
        protected override void OnDisappearingAnimationEnd()
        {
            base.OnDisappearingAnimationEnd();
        }

        protected override Task OnAppearingAnimationBeginAsync()
        {
            return base.OnAppearingAnimationBeginAsync();
        }

        protected override Task OnAppearingAnimationEndAsync()
        {
            return base.OnAppearingAnimationEndAsync();
        }

        protected override Task OnDisappearingAnimationBeginAsync()
        {
            return base.OnDisappearingAnimationBeginAsync();
        }

        protected override Task OnDisappearingAnimationEndAsync()
        {
            return base.OnDisappearingAnimationEndAsync();
        }

        // ### Overrided methods which can prevent closing a popup page ###

        // Invoked when a hardware back button is pressed
        protected override bool OnBackButtonPressed()
        {
            // Return true if you don't want to close this popup page when a back button is pressed
            Cancel();
            return base.OnBackButtonPressed();
        }

        // Invoked when background is clicked
        protected override bool OnBackgroundClicked()
        {
            // Return false if you don't want to close this popup page when a background of the popup page is clicked
            Cancel();
            return base.OnBackgroundClicked();
        }

        /// <summary>
        /// Link to cancel method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageButtonCancel_Clicked(object sender, EventArgs e) => Cancel();        

        /// <summary>
        /// Link to save method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageButtonSave_Clicked(object sender, EventArgs e) => Save();      
    }
}