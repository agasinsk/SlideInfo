using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlideInfo.App.Services
{
    public static class MessageConstants
    {
        public const string APP_NAME = "SlideInfo";
        public const string APP_EMAIL = "slide.info.pwr@gmail.com";

        public const string EMAIL_LOCAL_DOMAIN = "SlideInfo";

        public const string EMAIL_USERNAME = "slide.info.pwr@gmail.com";
        public const string EMAIL_PASSWORD = "pW56rkQG4%Eg";

        public const string EMAIL_HOST = "smtp.gmail.com";
        public const int EMAIL_PORT = 465;

        public static string ConfirmationEmailBodyTemplate =
                "Welcome to SlideInfo, <br/><br/> thank you for registering. " +
                "<br/>Please confirm your account by clicking this link: <a href = 'callbackUrl'>confirmation link</a>." +
                "<br/><br/> If you can't click the link, copy and paste this to the adress bar: callbackUrl . " +
                "<br/><br/> Best regards, <br/> SlideInfo"
            ;

    }
}
