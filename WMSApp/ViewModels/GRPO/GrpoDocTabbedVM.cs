using System;
using System.Collections.Generic;
using System.Text;
using WMSApp.Views.GRPO;
using Xamarin.Forms;

namespace WMSApp.ViewModels.GRPO
{
    //public enum GrpoDocListStatus
    //{
    //    ALL, OPEN, CLOSED
    //}

    public class GrpoDocTabbedVM
    {

        //GrpoDocListView[] pages = new GrpoDocListView[1
        GrpoDocListView openList;
        TabbedPage PageManager;
        INavigation Navigation;
        /// <summary>
        /// Constructor
        /// </summary>
        public GrpoDocTabbedVM(INavigation navigation, Xamarin.Forms.TabbedPage manager)
        {
            Navigation = navigation;
            PageManager = manager;
            Init();
        }

        void Init()
        {
            if (PageManager == null) return;
            openList = new GrpoDocListView("OPEN");
            PageManager.Children.Add(openList);

            //pages[0] = new GrpoDocListView("ALL")
            //{
            //    Title = "ALL"
            //};

            //var  = new GrpoDocListView("OPEN")
            //{
            //    Title = "OPEN"
            //};

            //pages[1] = new GrpoDocListView("CLOSED")
            //{
            //    Title = "CLOSED"
            //};

            // Add in the page into the tabbed page
            //foreach (var page in pages)
            //{
            //    PageManager.Children.Add(page);
            //}
        }
    }
}
