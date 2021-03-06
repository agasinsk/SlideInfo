﻿using Microsoft.AspNetCore.Mvc.Rendering;

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
            var routeAction = (string)routeData.Values["action"];
            var routeController = (string)routeData.Values["controller"];

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

        public static string IsSlideChosen(this IHtmlHelper html,
            string controller,
            string item)
        {
            var routeData = html.ViewContext.RouteData;
            var routeController = (string)routeData.Values["controller"];

            // both must match
            var isActive = controller == routeController && item != null;

            return isActive ? "active-red-bg" : "active";
        }

        public static string IsDisabled(this IHtmlHelper html,
            string controller,
            string item)
        {
            var routeData = html.ViewContext.RouteData;
            var routeController = (string)routeData.Values["controller"];

            return controller == routeController && item != null ? "disabled" : "";
        }


        public static string IsDisabled(this IHtmlHelper html,
            string controller,
            bool item)
        {
            var routeData = html.ViewContext.RouteData;
            var routeController = (string)routeData.Values["controller"];

            return controller == routeController && !item ? "disabled" : "";
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

        public static string IsSortedDesc(this IHtmlHelper html,
            object sortOrderParam)
        {
            var sortOrderString = sortOrderParam as string;

            return sortOrderString != null ? html.IsSortedDesc(sortOrderString) : "glyphicon-sort";
        }

        public static string IsSortedDesc(this IHtmlHelper html,
            string sortOrderParam)
        {
            var isDescending = sortOrderParam.Contains("desc");

            return isDescending ? "glyphicon-menu-up" : "glyphicon-menu-down";
        }
    }
}
