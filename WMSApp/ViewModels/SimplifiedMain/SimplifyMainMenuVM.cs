using Newtonsoft.Json;
using Rg.Plugins.Popup.Extensions;
using System;
using System.ComponentModel;
using System.Linq;
using WMSApp.Class;
using WMSApp.Interface;
using WMSApp.Models.Setup.Group;
using WMSApp.View.Item;
using WMSApp.Views.DeliveryOrder;
using WMSApp.Views.GIGR;
using WMSApp.Views.GoodsReturn;
using WMSApp.Views.GoodsReturnRequest;
using WMSApp.Views.GRPO;
using WMSApp.Views.InventoryCounts;
using WMSApp.Views.RequestSummary;
using WMSApp.Views.ReturnRequest;
using WMSApp.Views.Setting;
using WMSApp.Views.Test;
using WMSApp.Views.Transfer;
using WMSApp.Views.TransferRequest;
using Xamarin.Forms;

namespace WMSApp.ViewModels.SimplifiedMain
{
    public enum EnumScreen
    {
        GoodsReceiptPO = 1,
        Delivery = 2,
        GoodsReceipt = 3,
        GoodsIssue = 4,
        InventoryCounting = 5,
        InventoryRequest = 7,
        InventoryTransfer = 8,
    }

    public class ButtonColor
    {
        public static string GoodsReceiptPoColor = "#4285F4";
        public static string DeliveryColor = "#DB4437";
        public static string GoodsReceiptColor = "#F4B400";
        public static string GoodsIssuesColor = "#0F9D58";
        public static string InventoryCounting = "#5e7991";
        public static string InventoryRequest = "#4285F4";
        public static string InventoryTransfer = "#DB4437";
        public static string GoodsReturnRequest = "#F4B400";
        public static string GoodsReturn = "#0F9D58";
        public static string ReturnRequest = "#4285F4";
        public static string Return = "#DB4437";
    }

    //public class ResourceColorKey
    //{
    //    public static string GoodsReceiptPoColor = "GoogleBlue";
    //    public static string DeliveryColor = "GoogleRed";
    //    public static string GoodsReceiptColor = "GoogleYellow";
    //    public static string GoodsIssuesColor = "GoogleGreen";
    //    public static string InventoryCounting = "ColorTone5";
    //    public static string InventoryRequest = "GoogleBlue";
    //    public static string InventoryTransfer = "GoogleRed";
    //}

    public class SimplifyMainMenuVM : INotifyPropertyChanged
    {
        public Command CmdGrpo { get; set; }
        public Command CmdDeliveryOrder { get; set; }
        public Command CmdNotification { get; set; }
        public Command CmdGIGR { get; set; }
        public Command CmdInventoryCount { get; set; }
        public Command CmdSettingView { get; set; }
        public Command CmdLogout { get; set; }
        public Command CmdInventoryRequest { get; set; }
        public Command CmdInventoryTransfer { get; set; }
        public Command CmdGoodsReturnRequest { get; set; } // for purchase
        public Command CmdGoodsReturn { get; set; } // for purchase - inbound
        public Command CmdReturnRequest { get; set; } // from sales - outbound
        public Command CmdReturn { get; set; } // sales 

        bool isSettingEnable;
        public bool IsSettingEnable
        {
            get => isSettingEnable;
            set
            {
                if (isSettingEnable != value)
                {
                    isSettingEnable = value;
                    OnPropertyChanged(nameof(IsSettingEnable));
                }
            }
        }
        ContentPage page;

        INavigation Navigation;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string pName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pName));

        public SimplifyMainMenuVM(INavigation navigation, ContentPage targetPage)
        {
            Navigation = navigation;
            page = targetPage;
            InitCmd();
            InitMessageCenter();
            LoadSystemProperty();
            CreateMainScreenWithPermission();

            if (App.IsSuperAdmin)
            {
                page.ToolbarItems.Add(new ToolbarItem
                {
                    Order = ToolbarItemOrder.Default,
                    IconImageSource = "settings25.png",
                    Command = CmdSettingView,
                    Text = "Setting"
                });
            }

            // for admin user roles
            if (App.IsAdminRoles)
            {
                page.ToolbarItems.Add(new ToolbarItem
                {
                    Order = ToolbarItemOrder.Default,
                    IconImageSource = "settings25.png",
                    Command = CmdSettingView,
                    Text = "Setting"
                });
            }

            page.ToolbarItems.Add(new ToolbarItem
            {
                Order = ToolbarItemOrder.Primary,
                IconImageSource = "logout512.png",
                Command = CmdLogout,
                Text = "Logout"
            });
        }

        void InitMessageCenter ()
        {
            MessagingCenter.Subscribe<string>(this, App._RequestSummaryView, (string request) =>
            {
                if(!string.IsNullOrWhiteSpace(request))
                {
                    var json = JsonConvert.DeserializeObject<zwaRequest>(request);
                    Navigation.PushPopupAsync(new RequestSummaryView(json));                    
                }
            });
        }


        /// <summary>
        /// Return the button for link the command and parameter
        /// </summary>
        /// <param name="permit"></param>
        /// <returns></returns>
        Button GetButton(zwaUserGroup1 permit)
        {
            var layoutOptions_horizonta = LayoutOptions.Center;
            var layoutOptions_vertical = LayoutOptions.Center;
            var buttonHeight = 100;
            var buttonWidth = 220;

            switch (permit.screenId)
            {
                case 1:
                    {
                        return new Button
                        {
                            //BackgroundColor = Color.FromHex(ButtonColor.GoodsReceiptPoColor),
                            BackgroundColor = Color.Accent,
                            TextColor = Color.White,
                            BorderWidth = 5,
                            BorderColor = Color.White,
                            CornerRadius = 100,
                            Text = permit.title,
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            Command = CmdGrpo,
                            HorizontalOptions = layoutOptions_horizonta,
                            VerticalOptions = layoutOptions_vertical,
                            HeightRequest = buttonHeight,
                            WidthRequest = buttonWidth
                        };
                    }
                case 2:
                    {
                        return new Button
                        {
                            //BackgroundColor = Color.FromHex(ButtonColor.DeliveryColor),
                            BackgroundColor = Color.Accent,
                            TextColor = Color.White,
                            BorderWidth = 5,
                            BorderColor = Color.White,
                            CornerRadius = 100,
                            Text = permit.title,
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            Command = CmdDeliveryOrder,
                            HorizontalOptions = layoutOptions_horizonta,
                            VerticalOptions = layoutOptions_vertical,
                            HeightRequest = buttonHeight,
                            WidthRequest = buttonWidth
                        };
                    }
                case 3:
                    {
                        return new Button
                        {
                            //BackgroundColor = Color.FromHex(ButtonColor.GoodsReceiptColor),
                            BackgroundColor = Color.Accent,
                            TextColor = Color.White,
                            BorderWidth = 5,
                            BorderColor = Color.White,
                            CornerRadius = 100,
                            Text = permit.title,
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            Command = CmdGIGR,
                            CommandParameter = "GR",
                            HorizontalOptions = layoutOptions_horizonta,
                            VerticalOptions = layoutOptions_vertical,
                            HeightRequest = buttonHeight,
                            WidthRequest = buttonWidth
                        };

                    }
                case 4:
                    {
                        return new Button
                        {
                            //BackgroundColor = Color.FromHex(ButtonColor.GoodsIssuesColor),
                            BackgroundColor = Color.Accent,
                            TextColor = Color.White,
                            BorderWidth = 5,
                            BorderColor = Color.White,
                            CornerRadius = 100,
                            Text = permit.title,
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            Command = CmdGIGR,
                            CommandParameter = "GI",
                            HorizontalOptions = layoutOptions_horizonta,
                            VerticalOptions = layoutOptions_vertical,
                            HeightRequest = buttonHeight,
                            WidthRequest = buttonWidth
                        };

                    }
                case 5:
                    {
                        
                        return new Button
                        {
                            //BackgroundColor = Color.FromHex(ButtonColor.InventoryRequest),
                            BackgroundColor = Color.Accent,
                            TextColor = Color.White,
                            BorderWidth = 5,
                            BorderColor = Color.White,
                            CornerRadius = 100,
                            Text = permit.title,
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            Command = CmdInventoryRequest,
                            HorizontalOptions = layoutOptions_horizonta,
                            VerticalOptions = layoutOptions_vertical,
                            HeightRequest = buttonHeight,
                            WidthRequest = buttonWidth
                        };
                    }               
                case 6:
                    {
                       
                        return new Button
                        {
                            //BackgroundColor = Color.FromHex(ButtonColor.InventoryTransfer),
                            BackgroundColor = Color.Accent,
                            TextColor = Color.White,
                            BorderWidth = 5,
                            BorderColor = Color.White,
                            CornerRadius = 100,
                            Text = permit.title,
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            Command = CmdInventoryTransfer,
                            HorizontalOptions = layoutOptions_horizonta,
                            VerticalOptions = layoutOptions_vertical,
                            HeightRequest = buttonHeight,
                            WidthRequest = buttonWidth
                        };
                    }
                case 7:
                    {
                        
                        return new Button
                        {
                            //BackgroundColor = Color.FromHex(ButtonColor.GoodsReturnRequest),
                            BackgroundColor = Color.Accent,
                            TextColor = Color.White,
                            BorderWidth = 5,
                            BorderColor = Color.White,
                            CornerRadius = 100,
                            Text = permit.title,
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            Command = CmdGoodsReturnRequest,
                            HorizontalOptions = layoutOptions_horizonta,
                            VerticalOptions = layoutOptions_vertical,
                            HeightRequest = buttonHeight,
                            WidthRequest = buttonWidth
                        };
                    }
                case 8:
                    {
                        
                        return new Button
                        {
                            //BackgroundColor = Color.FromHex(ButtonColor.GoodsReturn),
                            BackgroundColor = Color.Accent,
                            TextColor = Color.White,
                            BorderWidth = 5,
                            BorderColor = Color.White,
                            CornerRadius = 100,
                            Text = permit.title,
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            Command = CmdGoodsReturn,
                            HorizontalOptions = layoutOptions_horizonta,
                            VerticalOptions = layoutOptions_vertical,
                            HeightRequest = buttonHeight,
                            WidthRequest = buttonWidth
                        };
                    }
                case 9:
                    {

                        return new Button
                        {
                            //BackgroundColor = Color.FromHex(ButtonColor.ReturnRequest),
                            BackgroundColor = Color.Accent,
                            TextColor = Color.White,
                            BorderWidth = 5,
                            BorderColor = Color.White,
                            CornerRadius = 100,
                            Text = permit.title,
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            Command = CmdReturnRequest,
                            HorizontalOptions = layoutOptions_horizonta,
                            VerticalOptions = layoutOptions_vertical,
                            HeightRequest = buttonHeight,
                            WidthRequest = buttonWidth
                        };
                    }

                case 10:
                    {
                        
                        return new Button
                        {
                            //BackgroundColor = Color.FromHex(ButtonColor.Return),
                            BackgroundColor = Color.Accent,
                            TextColor = Color.White,
                            BorderWidth = 5,
                            BorderColor = Color.White,
                            CornerRadius = 100,
                            Text = permit.title,
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            Command = CmdReturn,
                            HorizontalOptions = layoutOptions_horizonta,
                            VerticalOptions = layoutOptions_vertical,
                            HeightRequest = buttonHeight,
                            WidthRequest = buttonWidth
                        };
                    }

                case 11:
                    {
                        return new Button
                        {
                            //BackgroundColor = Color.FromHex(ButtonColor.InventoryCounting),
                            BackgroundColor = Color.Accent,
                            TextColor = Color.White,
                            BorderWidth = 5,
                            BorderColor = Color.White,
                            CornerRadius = 100,
                            Text = permit.title,
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            Command = CmdInventoryCount,
                            HorizontalOptions = layoutOptions_horizonta,
                            VerticalOptions = layoutOptions_vertical,
                            HeightRequest = buttonHeight,
                            WidthRequest = buttonWidth
                        };

                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Init the command action
        /// </summary>
        void InitCmd()
        {
            CmdGrpo = new Command(() =>
            {
                //Navigation.PushAsync(new GrpoDocTabbedView());
                //Navigation.PushAsync(new GrpoDocListView("OPEN"));
                // GRPOSelectionMenuView
                //Navigation.PushAsync(new GRPOSelectionMenuView());

                Navigation.PushAsync(new GrpoDocListView("OPEN"));
            });

            //20200616T1253
            CmdDeliveryOrder = new Command(() =>
            {
                Navigation.PushAsync(new SoDocListView("OPEN"));
                //Navigation.PushAsync(new DOSelectionMenuView());

            });

            CmdNotification = new Command(() =>
            {
                Navigation.PushAsync(new TestNotification());
            });

            CmdGIGR = new Command<string>((string transType) =>
            {
                Navigation.PushAsync(new GoodsTransView(transType));
                return;
            });

            CmdInventoryCount = new Command<string>((string transType) =>
            {
                Navigation.PushAsync(new InventoryCountsDocListView(transType));
                return;
            });

            // 20200920T1321
            CmdInventoryRequest = new Command<string>((string transType) =>
            {
                Navigation.PushAsync(new TransferRequestView());
                return;
            });

            // 20200921T1238
            CmdInventoryTransfer = new Command<string>((string transType) =>
            {
                Navigation.PushAsync(new TransferListView(transType));
                return;
            });

            // 20201118T2212
            CmdGoodsReturnRequest = new Command<string>((string transType) =>
            {
                Navigation.PushAsync(new GoodsReturnRequestListView("OPEN"));
                return;
            });

            CmdGoodsReturn = new Command<string>((string transType) =>
            {
                Navigation.PushAsync(new GoodsReturnListView("OPEN"));
                return;
            });

            CmdReturnRequest = new Command<string>((string transType) =>
            {
                Navigation.PushAsync(new ReturnRequestListView("OPEN"));
                return;
            });

            CmdReturn = new Command<string>((string transType) =>
            {
                Navigation.PushAsync(new ReturnListView("OPEN"));
                return;
            });

            // 20200621
            CmdSettingView = new Command(() =>
            {
                Navigation.PushAsync(new SettingView());
                return;
            });

            // logout 20200623
            CmdLogout = new Command(() =>
            {
               var iclose = DependencyService.Get<ICloseApplication>();
               iclose?.closeApplication();
           });
        }

        /// <summary>
        /// Create Main Screen WithPermission
        /// </summary>
        void CreateMainScreenWithPermission()
        {
            try
            {
                if (App.currentPermissions == null)
                {
                    ShowAlert("There is no permission setup for this user, please contact support on this. Thanks");
                    return;
                }

                if (App.currentPermissions.Length == 0)
                {
                    ShowAlert("There is no permission setup for this user, please contact support on this. Thanks");
                    return;
                }

                // here craete the button table
                var buttonGrid = new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition(),
                        new RowDefinition(),
                        new RowDefinition()
                    },
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(),
                        new ColumnDefinition()
                    },
                    RowSpacing = 3,
                    ColumnSpacing = 3,
                    Padding = 3
                };

                // load the pemission from db and populate
                var permitList = App.currentPermissions.Where(x => x.authorised == "1").ToList();
                if (permitList == null)
                {
                    ShowAlert("There is no permission setup for this user, please contact support on this. Thanks");
                    return;
                }

                // here control two button a row
                int gridRow = 0, gridCol = 0;
                foreach (var permission in permitList)
                {
                    if (gridCol > 1) // when grid col == 1 then reset to 0
                    {
                        gridRow++;
                        gridCol = 0;
                    }
                    var button = GetButton(permission);
                    if (button == null) continue;
                    buttonGrid.Children.Add(button, gridCol, gridRow);
                    gridCol++;
                }

                /// setup the page title
                var stackLayout = new StackLayout { Padding = 0, Spacing = 0 };
                //stackLayout.Children.Add(new Label
                //{
                //    //Text = App.companyName,
                //    //HorizontalOptions = LayoutOptions.Start,
                //    //FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                //    //FontAttributes = FontAttributes.Bold
                //});

                //stackLayout.Children.Add(
                //    new Label
                //    {
                //        //Text = $"Good day, {App.waCurrentUser}",
                //        //HorizontalOptions = LayoutOptions.Start,
                //        //FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                //        //FontAttributes = FontAttributes.None
                //    });
                //stackLayout.Children.Add(
                //    new Label
                //    {
                //        //Text = $"{App.currentGroup.groupName}, {App.currentGroup.groupDesc}",
                //        //HorizontalOptions = LayoutOptions.Start,
                //        //FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                //        //FontAttributes = FontAttributes.Italic
                //    });
                //stackLayout.Children.Add(
                //    new Label
                //    {
                //        //Text = $"App Version {DependencyService.Get<IAppVersion>()?.Version}",
                //        //HorizontalOptions = LayoutOptions.Start,
                //        //FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                //        //FontAttributes = FontAttributes.Italic
                //    });

                stackLayout.Children.Add(buttonGrid);
                var scrollView = new ScrollView { Content = stackLayout };

                //stackLayout.Children.Add(scrollView);                
                var imageButton = new ImageButton
                {
                    CornerRadius = 30,
                    Source = "query512.png",
                    HeightRequest = 15,
                    WidthRequest = 15,
                    BackgroundColor = Color.Wheat, 
                    Command = new Command( () => 
                    {
                        //Navigation.PushAsync(new InventoryView());
                        Navigation.PushAsync(new ItemListV());

                    })
                };

                var absoluteLayout = new AbsoluteLayout();
                absoluteLayout.Children.Add(scrollView, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.SizeProportional);
                absoluteLayout.Children.Add(imageButton, new Rectangle(0.95, 0.95, 80, 80), AbsoluteLayoutFlags.PositionProportional);
                page.Content = absoluteLayout;
            }
            catch (Exception excep)
            {
                ShowAlert(excep.ToString());
            }
        }

        /// <summary>
        /// to load the warehouse from the system
        /// </summary>
        async void LoadSystemProperty()
        {
            var cioRequest = new Cio() // load the data from web server 
            {
                sap_logon_name = App.waUser,
                sap_logon_pw = App.waPw,
                token = App.waToken,
                phoneRegID = App.waToken,
                request = "GetWarehouseList",
            };

            string content, lastErrMessage;
            bool isSuccess = false;
            using (var httpClient = new HttpClientWapi())
            {
                content = await httpClient.RequestSvrAsync_mgt(cioRequest, "Warehouses");
                isSuccess = httpClient.isSuccessStatusCode;
                lastErrMessage = httpClient.lastErrorDesc;
            }

            if (!isSuccess) return;
            var retBag = JsonConvert.DeserializeObject<Cio>(content);

            if (retBag == null) return;
            if (retBag.dtoWhs != null) App.Warehouses = retBag.dtoWhs;
            if (retBag.DtoBins != null) App.Bins = retBag.DtoBins;
        }

        /// <summary>
        /// Show onscreen alert message
        /// </summary>
        /// <param name="message"></param>
        async void ShowAlert(string message) => await new Dialog().DisplayAlert("Alert", message, "OK");
    }
}
