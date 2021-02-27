﻿using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WMSApp.Models.SAP;
using WMSApp.ViewModels.ReturnRequest;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.ReturnRequest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReturnRequestLinePopUpView : PopupPage
    {
        public ReturnRequestLinePopUpView(List<DLN1_Ex> grnLines, ODLN_Ex doc, string callerAddress)
        {
            InitializeComponent();
            BindingContext = new ReturnRequestLinePopUpVM(Navigation, doc, grnLines, callerAddress);
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
            ((ReturnRequestLinePopUpVM)BindingContext).Close();
        }
    }
}
 