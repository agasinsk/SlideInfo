namespace SlideInfo.App.Constants
{
    public static class SessionConstants
    {
        public static string AlertTemplate = "<div class=\"alert alert-type\"><a href=\"\" class=\"close\" data-dismiss=\"alert\" aria-label=\"close\">&times;</a><strong>alert-title!</strong> alert-content</div>";

        public const string ALERT = "Alert";

        //account management
        public const string CHANGE_NAME_SUCCESS = "Your name has been changed.";
        public const string CHANGE_EMAIL_SUCCESS = "Your email has been changed.";
        public const string CHANGE_PASSWORD_SUCCESS = "Your password has been changed.";
        public const string SET_PASSWORD_SUCCESS = "Your password has been set.";
        public const string SET_TWO_FACTOR_SUCCESS = "Your two-factor authentication provider has been set.";
        public const string ERROR = "An error has occurred.";
        public const string REMOVE_LOGIN_SUCCESS = "The external login was removed.";
        public const string ADD_LOGIN_SUCCESS = "The external login was added.";


        //typical messages
        public const string ITEM_NOT_FOUND = "Item not found";
        public const string CANT_LOAD_DATA = "Data cannot be loaded";
        public const string NO_ACCESS = "No access to data";

        //serializable objects
        public const string CURRENT_SLIDE = "Slide";


    }
}
