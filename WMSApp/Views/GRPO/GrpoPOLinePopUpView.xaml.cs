using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WMSApp.Models.SAP;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GRPO
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GrpoPOLinePopUpView : PopupPage
    {        
        public ObservableCollection<POR1_Ex> ItemsSource { get; set; }
        public string CardName { get; set; }
        public string CardCode { get; set; }
        public int DocNum { get; set; }
        public DateTime DocDate { get; set; }

        POR1_Ex selectedItem;
        public POR1_Ex SelectedItem
        {
            get => selectedItem; 
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;

                MessagingCenter.Send(selectedItem, repliedAddr);

                selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
                Navigation.PopPopupAsync(); //<-- cllose the screen
            }
        }

        string repliedAddr { get; set; }

        public GrpoPOLinePopUpView(List<POR1_Ex> PoLines, OPOR_Ex doc, string callerAddress)
        {
            InitializeComponent();
            ItemsSource = new ObservableCollection<POR1_Ex>(PoLines);
            repliedAddr = callerAddress;

            CardName = doc.PO.CardName;
            CardCode = doc.PO.CardCode;
            DocNum = doc.PO.DocNum;
            DocDate = doc.PO.DocDate;
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
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
            return base.OnBackButtonPressed();
        }

        // Invoked when background is clicked
        protected override bool OnBackgroundClicked()
        {
            // Return false if you don't want to close this popup page when a background of the popup page is clicked            
            return base.OnBackgroundClicked();
        }

        private void BackScreen_Tapped(object sender, EventArgs e)
        {
            Navigation.PopPopupAsync();
        }

        private void LineChangeWhs_Invoked(object sender, EventArgs e)
        {

        }
    }
}