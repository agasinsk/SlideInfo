using SlideInfo.App.Helpers;

namespace SlideInfo.App.Models.SlideViewModels
{
    public class PropertiesViewModel
    {
        public string Name { get; set; }
        public PaginatedList<Property> Properties { get; set; }
        public PropertiesViewModel(string name, PaginatedList<Property> properties)
        {
            Name = name;
            Properties = properties;
        }
    }
}
