
using System.Collections.Generic;
using System.ComponentModel;
namespace WMSApp.Models.Setting
{
    public class zsCommDocList : List<zsCommDocFields>, INotifyPropertyChanged
    {
        public string Heading { get; set; }
        public bool isVisible { get; set; }
        int _cellHeight = 30;
        public int cellHeight
        {
            get
            {
                return (isVisible) ? _cellHeight : 0;
            }
            private set
            {
                if (_cellHeight != value)
                {
                    _cellHeight = value;
                    OnPropertyChanged(nameof(cellHeight));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Handle the on property changed, value update to screen
        /// </summary>
        /// <param name="propertyname"></param>
        public void OnPropertyChanged(string propertyname) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
    }
}
