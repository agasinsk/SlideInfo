using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlideInfo.App.Models.SlideViewModels
{
    public class PropertiesViewModel
    {
        public string Name { get; set; }
        public IDictionary<string, string> Properties { get; set; }

        public PropertiesViewModel(string name, IDictionary<string, string> properties)
        {
            Name = name;
            Properties = properties;
        }
    }
}
