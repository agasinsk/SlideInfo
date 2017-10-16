angular.module("messenger")
    .directive("messageList", function ($timeout, $anchorScroll, messengerService, ngNotify) {
        return {
            restrict: "E",
            replace: true,
            templateUrl: "templates/message-list.html",
            link: function (scope, element, attrs, ctrl) {
                var element = angular.element(element)
                var init = function () { };
                init();
            },
            controller: function ($scope) {
                $scope.messages = messengerService;
            }
        };
    });