angular.module("messenger")
    .directive("messageItem", function (messengerService) {
        return {
            restrict: "E",
            templateUrl: "/templates/message-item.html",
            vm: {
                content: "@",
                date: "@"
            }
        }
    });