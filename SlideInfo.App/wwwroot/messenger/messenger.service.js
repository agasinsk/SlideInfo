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
            getConversations: getConversations,
            getMessage: getMessage,
            saveMessage: saveMessage,
            deleteMessage: deleteMessage,
            foundSearchPhrase: foundSearchPhrase
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

        function getConversations() {
            return $http.get("/Messenger/Conversations")
                .then(function (response) {
                    return response.data;
                });
        }

        function getConversation(subject, pageNumber) {
            return $http.get("/Messenger/Conversation/" + subject + "/" + pageNumber)
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

        function saveMessage(messageJson) {
            return $http.post("/Messenger/Save", messageJson)
                .then(function (response) {
                    return response.data;
                });
        }

        function deleteMessage() {
            return $http.delete("/Messenger/Conversation")
                .then(function (response) {
                    return response.data;
                });
        }

        function foundSearchPhrase(user, phrase) {
            var searchPhrase = phrase.toLowerCase();
            if (user.Id.toLowerCase().indexOf(searchPhrase) !== -1) {
                return true;
            }
            if (user.FullName.toLowerCase().indexOf(searchPhrase) !== -1) {
                return true;
            }
            if (user.Email.toLowerCase().indexOf(searchPhrase) !== -1) {
                return true;
            }
            return false;
        }

        return service;
    }
})();