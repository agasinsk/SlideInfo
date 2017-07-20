using Microsoft.AspNetCore.Mvc.Rendering;

namespace SlideInfo.App.Helpers
{
	public static class HtmlHelperExtensions
	{
	    public static string IsActiveController(this IHtmlHelper html,
	        string controller)
	    {
	        var routeData = html.ViewContext.RouteData;
	        var routeController = (string)routeData.Values["controller"];

	        return controller == routeController ? "active" : "";
	    }

        public static string IsActive(this IHtmlHelper html,
			string controller,
			string action)
		{
			var routeData = html.ViewContext.RouteData;
			var routeAction = (string) routeData.Values["action"];
			var routeController = (string) routeData.Values["controller"];

			// both must match
			var isActive = controller == routeController &&
			                   action == routeAction;

			return isActive ? "active" : "";
		}

	    public static string IsDropdownActive(this IHtmlHelper html,
	        string controller,
	        string item)
	    {
	        if (item == null)
	            return "";

            var routeData = html.ViewContext.RouteData;
	        var routeController = (string)routeData.Values["controller"];

	        // both must match
	        var isActive = controller == routeController;

	        return isActive ? "dropdown-toggle" : "";
	    }

        public static string SetActiveOrInvisible(this IHtmlHelper html,
			string control,
			string action, 
			string item)
		{
			if (item == null)
				return "invisible";

			var routeData = html.ViewContext.RouteData;

			var routeAction = (string)routeData.Values["action"];
			var routeControl = (string)routeData.Values["controller"];

			var isActive = control == routeControl &&
			               action == routeAction;

			
			return isActive ? "active" : "";
		}
	}
}
