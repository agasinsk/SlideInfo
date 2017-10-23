﻿(function () {
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
            generateConversationSubject: generateConversationSubject
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

        function getConversation(subject) {
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

        function generateConversationSubject(userIds) {

            var subject = "";
            var sortedIds = _.sortBy(userIds, function (userId) {
                return userId;
            });

            _.each(sortedIds, function (userId) {
                var userIdTruncated = userId.split("-")[0];
                subject += userIdTruncated;
                subject += "-";
            });
            console.log("Generated conversation subject: ", subject);
            return subject;
        }

        return service;
    }
})();