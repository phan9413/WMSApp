using System;
using System.Drawing;
namespace WMSApp.Class
{
    /// <summary>
    /// Use by the transfer request on hold 
    /// 20201020
    /// </summary>
    public class zwaHoldRequest // database 
    {
        public int Id { get; set; }
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public string Picker { get; set; }
        public DateTime TransDate { get; set; }
        public Guid HeadGuid { get; set; }
        public string Status { get; set; }

        /// <summary>
        /// For app on screen display
        /// </summary>
        public string StatusDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Status)) return "Open";
                if (Status.Equals("O")) return "Open";
                return "Close";
            }
        }

        public string DocNumDisplay =>(DocEntry == -1) ? $"SAT# {Id}" : $"Request# {DocNum}";

        public string DetailsDisplay => $"{Picker} . {TransDate:dd/MM/yyyy}";
      
        public Color CellColor => (DocEntry == -1) ? Color.White : Color.White;  
      
    }
}
