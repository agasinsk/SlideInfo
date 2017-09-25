using Microsoft.AspNetCore.Http;
using SlideInfo.App.Constants;

namespace SlideInfo.App.Helpers
{
    public class AlertFactory
    {
        private readonly HttpContext httpContext;

        public AlertFactory(HttpContext httpContext)
        {
            this.httpContext = httpContext;
        }

        public void CreateAlert(AlertType alertType, string alertContent)
        {
            var alertTemplate = GetAlertTemplate(alertType, alertContent);
            httpContext.Session.SetString(SessionConstants.ALERT, alertTemplate);
        }

        public static string GetAlertTemplate(AlertType alertType, string alertContent)
        {
            var htmlAlertType = GetAlertType(alertType);
            var htmlAlertTitle = GetAlertTitle(alertType);
            var alert = SessionConstants.AlertTemplate.Replace("alert-type", htmlAlertType)
                .Replace("alert-title", htmlAlertTitle).Replace("alert-content", alertContent);
            return alert;
        }

        private static string GetAlertType(AlertType alertType)
        {
            switch (alertType)
            {
                case AlertType.Danger:
                    return "alert-danger";
                case AlertType.Warning:
                    return "alert-warning";
                case AlertType.Success:
                    return "alert-success";
                case AlertType.Info:
                    return "alert-info";
                default:
                    return "alert-info";
            }
        }

        private static string GetAlertTitle(AlertType alertType)
        {
            switch (alertType)
            {
                case AlertType.Danger:
                    return "Error";
                case AlertType.Warning:
                    return "Warning";
                case AlertType.Success:
                    return "Success";
                case AlertType.Info:
                    return "Info";
                default:
                    return "Info";
            }
        }
    }
}
