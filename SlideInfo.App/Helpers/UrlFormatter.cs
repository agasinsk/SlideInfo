using System.Text;
using System.Text.RegularExpressions;

namespace SlideInfo.App.Helpers
{
	public static class UrlFormatter
	{
	    public static string ToUrlSlug(this string value)
	    {
	        //First to lower case 
	        value = value.ToLowerInvariant();

	        //Remove all accents
	        var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(value);

	        value = Encoding.ASCII.GetString(bytes);

	        //Replace spaces 
	        value = Regex.Replace(value, @"\s", "-", RegexOptions.Compiled);

	        //Remove invalid chars 
	        value = Regex.Replace(value, @"[^\w\s\p{Pd}]", "", RegexOptions.Compiled);

	        //Trim dashes from end 
	        value = value.Trim('-', '_');

	        //Replace double occurences of - or \_ 
	        value = Regex.Replace(value, @"([-_]){2,}", "$1", RegexOptions.Compiled);

	        return value;
	    }

        public static string DziUrlFor(string associated)
		{
			var urlBase = associated == null ? "slide" : ToUrlSlug(associated);

			return urlBase;
		}

		public static string UrlFor(string name)
		{
			return name == null ? "slide" : ToUrlSlug(name);
		}

	}
}