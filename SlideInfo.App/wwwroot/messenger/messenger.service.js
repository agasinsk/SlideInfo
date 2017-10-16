(function () {
    "use strict";
    angular.module("messenger")
        .factory("messengerService", messengerService);

    messengerService.$inject = ["$http"];

    function messengerService($http) {
        var service = {
            getUsers: getUsers,
            getCurrentUser: getCurrentUser,
            getConversation: getConversation,
            getConversationByUserName: getConversationByUserName,
            getMessage: getMessage,
            saveMessage: saveMessage,
            deleteMessage: deleteMessage
        };

        ///////////////

        function getUsers() {
            return $http.get("/Messenger/Users")
                .then(function (response) {
                    return response.data;
                });
        }

        function getCurrentUser() {
            return $http.get("/Messenger/CurrentUser")
                .then(function (response) {
                    return response.data;
                });
        }

        function getConversation(subject) {
            return $http.get("/Messenger/Conversation/" + subject)
                .then(function (response) {
                    return response.data;
                });
        }

        function getConversationByUserName(subject) {
            return $http.get("/Messenger/Conversation/" + subject)
                .then(function (response) {
                    return response.data;
                });
        }

        function getMessage() {
            return $http.get("/Messenger/Conversation")
                .then(function (response) {
                    return response.data;
                });
        }

        function saveMessage() {
            return $http.get("/Messenger/Conversation")
                .then(function (response) {
                    return response.data;
                });
        }

        function deleteMessage() {
            return $http.get("/Messenger/Conversation")
                .then(function (response) {
                    return response.data;
                });
        }

        return service;
    }
})();