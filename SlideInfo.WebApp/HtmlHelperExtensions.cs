using Microsoft.AspNetCore.Mvc.Rendering;

namespace SlideInfo.WebApp
{
	public static class HtmlHelperExtensions
	{
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

		public static string LinkStatus(this IHtmlHelper html,
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
