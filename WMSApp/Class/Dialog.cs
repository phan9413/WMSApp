using System;
using System.Threading.Tasks;
using WMSApp.Interface;
using Xamarin.Forms;
namespace WMSApp.Class
{
    public class Dialog
    {
        /// <summary>
        /// Display message to screen and return user input 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="accept"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public async Task<bool> DisplayAlert(string title, string message, string accept, string cancel) => 
            await App.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        
        /// <summary>
        /// Display Alert with accept string only
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public async Task DisplayAlert(string title, string message, string cancel) =>
            await App.Current.MainPage.DisplayAlert(title, message, cancel);
        
        /// <summary>
        /// Display prompt user input on screen
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="accept"></param>
        /// <param name="cancel"></param>
        /// <param name="placeholder"></param>
        /// <param name="maxLength"></param>
        /// <param name="keyboard"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        public async Task<string> DisplayPromptAsync(string title, string message,
            string accept = "OK", string cancel = "Cancel", string placeholder = null,
            int maxLength = -1, Keyboard keyboard = null, string initialValue = "")
        {
            try
            {
                string input = await App.Current.MainPage.DisplayPromptAsync(title, message,
                    accept, cancel, placeholder, maxLength, keyboard, initialValue);

                if (input == null) return "";
                if (input.ToLower().Equals(cancel.ToLower())) return "";
                return input;
            }
            catch (Exception excep)
            {
                var toast = DependencyService.Get<IToastMessage>();
                toast?.LongAlert($"{excep}");
                return "";
            }
        }

        /// <summary>
        /// Display a sheet list for user selection
        /// </summary>
        /// <param name="title"></param>
        /// <param name="cancel"></param>
        /// <param name="destruction"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public async Task<string> DisplayActionSheet(string title, string cancel, string destruction, string[] buttons) => 
            await App.Current.MainPage.DisplayActionSheet(title, cancel, destruction, buttons);
        
    }
}