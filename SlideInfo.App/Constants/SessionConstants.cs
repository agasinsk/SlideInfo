using SlideInfo.App.Controllers;

namespace SlideInfo.App.Constants
{
    public static class SessionConstants
    {
        public static string AlertTemplate = "<div class=\"alert alert-type\"><a href=\"\" class=\"close\" data-dismiss=\"alert\" aria-label=\"close\">&times;</a><strong>alert-title!</strong> alert-content</div>";

        public const string Alert = "Alert";

        //account management
        public const string ChangeNameSuccess = "Your name has been changed.";
        public const string ChangeEmailSuccess = "Your email has been changed.";
        public const string ChangePasswordSuccess = "Your password has been changed.";
        public const string SetPasswordSuccess = "Your password has been set.";
        public const string SetTwoFactorSuccess = "Your two-factor authentication provider has been set.";
        public const string Error = "An error has occurred.";
        public const string RemoveLoginSuccess = "The external login was removed.";
        public const string AddLoginSuccess = "The external login was added.";


        //typical messages
        public const string ItemNotFound = "Item not found";
        public const string CantLoadData = "Data cannot be loaded";
        public const string NoAccess = "No access to data";

        //serializable objects
        public const string CurrentSlide = "Slide";


    }
}
