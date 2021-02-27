using WMSApp.Class;
using WMSApp.Models.SAP;
using WMSApp.ViewModels.Transfer;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace WMSApp.Views.Transfer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TransferListLineView : ContentPage
    {
        /// <summary>
        /// Request doc
        /// </summary>
        /// <param name="selectedDoc"></param>
        /// <param name="flowDirection"></param>
        public TransferListLineView(OWTQ_Ex selectedDoc, string flowDirection)
        {
            InitializeComponent();
            // show IsTransferDocVisible = true
            BindingContext = new TransferListLineVM(Navigation, selectedDoc, flowDirection);
        }

        /// <summary>
        /// for SAT
        /// </summary>
        /// <param name="selectedDoc"></param>
        /// <param name="flowDirection"></param>
        public TransferListLineView(zwaHoldRequest selectedDoc, string flowDirection)
        {
            InitializeComponent();
            // show IsTransferDocVisible = true
            BindingContext = new TransferListLineVM(Navigation, selectedDoc, flowDirection);
        }
    }
}