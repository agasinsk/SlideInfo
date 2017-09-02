namespace SlideInfo.App.Constants
{
    public static class MessageConstants
    {
        public const string AppName = "SlideInfo";
        public const string AppEmail = "slide.info.pwr@gmail.com";

        public const string EmailLocalDomain = "SlideInfo";

        public const string EmailUsername = "slide.info.pwr@gmail.com";
        public const string EmailPassword = "pW56rkQG4%Eg";

        public const string EmailHost = "smtp.gmail.com";
        public const int EmailPort = 465;

        public static string ConfirmationEmailBodyTemplate =
                "Welcome to SlideInfo, <br/> " +
                "<br/>please confirm your account by clicking this link: <a href = 'callbackUrl'>confirmation link</a>." +
                "<br/><br/> If you can't click the link, copy and paste this to the adress bar: callbackUrl . " +
                "<br/><br/> Best regards, <br/> SlideInfo"
            ;

        public static string ResetPasswordEmailBodyTemplate =
                "This is SlideInfo, <br/> " +
                "<br/>please reset your password by clicking this link: <a href = 'callbackUrl'>reset password</a>." +
                "<br/><br/> If you can't click the link, copy and paste this to the adress bar: callbackUrl . " +
                "<br/><br/> Best regards, <br/> SlideInfo"
            ;
        
    }
}
