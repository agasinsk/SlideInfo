(function () {
    "use strict";
    angular.module("messenger", ["ngNotify"]);

    // SignalR's hub object.
    var messengerHub = $.connection.messenger;

    $.connection.hub.logging = true;
    $.connection.hub.start();

    angular.module("messenger").value("messengerHub", messengerHub);
})();