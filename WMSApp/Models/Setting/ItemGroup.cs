using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using WMSApp.Models.Setup.Group;
using Xamarin.Forms;

namespace WMSApp.Models.Setting
{
    public class ItemGroup : ObservableCollection<Item>, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Command cmdGroupHeaderTapper { get; private set; } //<---- for authorisation screen
        public Command cmdGroupHeaderTapped { get; private set; } //<--- for main menu screen
        public int headerId { get; set; }
        public string Title { get; set; }
        public string Desc { get; set; }
        public string TitleWithItemCount
        {
            get => Title;
        }

        public string ShortName { get; set; }

        bool _expanded;
        public bool Expanded
        {
            get => _expanded;
            set
            {
                if (_expanded != value)
                {
                    _expanded = value;
                    OnPropertyChanged("Expanded");
                    OnPropertyChanged("StateIcon");
                }
            }
        }

        public string StateIcon
        {
            get => Expanded ? "expanded_blue.png" : "collapsed_blue.png";
        }

        public int ItemCount { get; set; }

        object _Tag; // represent the zwaUserGroup
        public object Tag
        {
            get => _Tag;
            set
            {
                if (_Tag != value)
                {
                    _Tag = value;
                    if (_Tag != null)
                    {
                        _isSwitchOn = (((zwaUserGroup1)_Tag).authorised == "1") ? true : false;
                    }
                }
            }
        }

        bool _isSwitchOn;
        public bool isSwitchOn
        {
            get => _isSwitchOn;
            set
            {
                if (_isSwitchOn != value)
                {
                    _isSwitchOn = value;

                    if (!switchOwnOnly)
                        SetItemSwicthValue(_isSwitchOn);

                    if (Tag != null)
                        ((zwaUserGroup1)Tag).authorised = (_isSwitchOn) ? "1" : "0";

                    switchOwnOnly = false;
                    OnPropertyChanged(nameof(isSwitchOn));
                }
            }
        }

        bool switchOwnOnly = false;

        /// <summary>
        /// View notification
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Constructor of the class, starting point
        /// </summary>
        /// <param name="title"></param>
        /// <param name="shortName"></param>
        /// <param name="headerid"></param>
        /// <param name="expenaded"></param>
        /// <param name="desp"></param>
        public ItemGroup(string title, string shortName, int headerid, bool expenaded = false, string desp = "")
        {
            Title = title;
            ShortName = shortName;
            Expanded = expenaded;
            headerId = headerid;
            Desc = desp;
            InitCmd();
            InitMC();
        }

        /// <summary>
        /// Initial the command binding
        /// </summary>
        void InitCmd()
        {
            // for authorisation screen
            cmdGroupHeaderTapper = new Command(() =>
            {
                isSwitchOn = !_isSwitchOn;
            });

            // for main menu
            //cmdGroupHeaderTapped = new Command((object obj) =>
            //{
            //    MessagingCenter.Send<object>((int)obj, "MenuHeaderClicked");
            //});
        }

        /// <summary>
        /// Init the message center
        /// </summary>
        void InitMC() => MessagingCenter.Subscribe<Item>(this, "UpdateHeaderSwitchOn", (Item obj) => UpdateHeaderSwitchOn(obj));

        /// <summary>
        /// Set wach item in this group
        /// </summary>
        /// <param name="isSwicthOn"></param>
        void SetItemSwicthValue(bool isSwicthOn)
        {
            foreach (Item item in this)
            {
                item.isSwitchOn = isSwicthOn;
            }
        }

        /// <summary>
        /// Hsndle item click / un tick
        /// </summary>
        /// <param name="obj"></param>
        void UpdateHeaderSwitchOn(Item obj)
        {
            try
            {
                var selected = (zwaUserGroup1)obj.Tag;
                var parent = (zwaUserGroup1)_Tag;

                if (parent == null) return;
                if (parent.screenId != selected.parentId) return;

                bool gotItemChecked = false;
                foreach (var item in this)
                {
                    if (item.isSwitchOn)
                    {
                        gotItemChecked = true;
                        break;
                    }
                }

                if (_isSwitchOn != gotItemChecked)
                {
                    switchOwnOnly = true; // turn on prevent all item check
                    isSwitchOn = gotItemChecked;
                }
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.Message);
            }
        }

        /// <summary>
        /// Dispose code
        /// </summary>
        public void Dispose()
        {
            MessagingCenter.Unsubscribe<Item>(this, "UpdateHeaderSwitchOn");
            //GC.Collect();
        }
    }
}
