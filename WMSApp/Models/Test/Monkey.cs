using System;
using System.Collections.Generic;
using System.Text;

namespace WMSApp.Models.Test
{
    public class Monkey
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string ImageUrl { get; set; }

        //Name = "Baboon",
        //            Location = "Africa & Asia",
        //            ImageUrl = "https://

        public override string ToString()
        {
            return Name;
        }
    }
}
