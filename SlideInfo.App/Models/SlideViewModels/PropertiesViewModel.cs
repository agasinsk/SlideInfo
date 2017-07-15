using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlideInfo.App.Models.SlideViewModels
{
    public class PropertiesViewModel
    {
        public string Name { get; set; }
        public IEnumerable<KeyValuePair<string,string>> Properties { get; set; }

        public PropertiesViewModel(string name, IEnumerable<KeyValuePair<string, string>> properties)
        {
            Name = name;
            Properties = properties;
        }
    }
}
