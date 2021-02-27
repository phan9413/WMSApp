using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WMSApp.Class
{
    [ContentProperty("Text")]
    public class TranslateExtension : IDisposable, IMarkupExtension
    {
        public string ResourceId { get; set; }
        public string Text { get; set; }
        public string GetLastErrorMessage { get; private set; } = "";

        // eg, example of the resource Id 'WMSApp.Resource.Login.LoginView'

        /// <summary>
        /// Construtor
        /// </summary>
        public TranslateExtension()
        {
           // to do code
        }

        /// <summary>
        /// Dispose code
        /// </summary>
        public void Dispose() { }// => GC.Collect();

        /// <summary>
        /// Return the label id from code
        /// </summary>
        /// <param name="stringid"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public string GetLabelValue(string resourceId, string keyValue, CultureInfo culture)
        {
            try
            {
                ResourceId = resourceId;
                Assembly assembly = typeof(TranslateExtension).GetTypeInfo().Assembly;

                var resourceManager = new ResourceManager(resourceId, assembly);
                return resourceManager.GetString(keyValue, culture);
            }
            catch (Exception excep)
            {
                GetLastErrorMessage = $"{excep}";
                return "";
            }
        }

        /// <summary>
        /// Return the label name in variuos langugae
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            try
            {
                if (Text == null) return null;
                if (ResourceId.Length == 0) return null;

                ResourceManager resourceManager = new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
                return resourceManager.GetString(Text, App.currentCultureInfo);
            }
            catch (Exception excep)
            {
                GetLastErrorMessage = $"{excep}";
                return null;
            }
        }
    }

}
