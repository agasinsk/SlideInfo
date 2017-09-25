using System;
using SlideInfo.Helpers;

namespace SlideInfo.App.Helpers
{
    public class AlertTypeConverter
    {
        public static AlertType Convert(string alertType)
        {
            if(alertType.Contains("info",StringComparison.CurrentCultureIgnoreCase))
                return AlertType.Info;
            if (alertType.Contains("danger", StringComparison.CurrentCultureIgnoreCase))
                return AlertType.Danger;
            if (alertType.Contains("success", StringComparison.CurrentCultureIgnoreCase))
                return AlertType.Success;
            if (alertType.Contains("danger", StringComparison.CurrentCultureIgnoreCase))
                return AlertType.Danger;
            return AlertType.Info;
        }
    }
}