﻿using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WMSApp.Models.SAP;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Views.GoodsReturnRequest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GrrGrnLinesPopUpSelectView : PopupPage
    {

        string ReturnCallerAddress { get; set; } = string.Empty;
        public ObservableCollection<PDN1_Ex> ItemsSource { get; set; }

        PDN1_Ex _SelectedItem;
        public PDN1_Ex SelectedItem
        {
            get => _SelectedItem;
            set
            {
                if (_SelectedItem == value) return;
                _SelectedItem = value;

                MessagingCenter.Send(_SelectedItem, ReturnCallerAddress);

                _SelectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
                Navigation.PopPopupAsync(); //<-- cllose the screen
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="duplicatedItems"></param>
        public GrrGrnLinesPopUpSelectView(List<PDN1_Ex> duplicatedItems, string callerAddress)
        {
            InitializeComponent();
            ReturnCallerAddress = callerAddress;
            ItemsSource = new ObservableCollection<PDN1_Ex>(duplicatedItems);
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
    }
}