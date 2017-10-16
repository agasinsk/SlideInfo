(function () {
    "use strict";
    angular.module("messenger")
        .controller("messengerController", messengerController);

    messengerController.$inject = ["$scope", "messengerService", "messengerHub", "htmlBuilder"];

    function messengerController($scope, messengerService, messengerHub, htmlBuilder) {
        var vm = this;

        vm.currentUserName = {};
        vm.currentReceiverName = {};
        vm.messageText = "";

        vm.users = [];
        vm.currentConversation = [];

        //controller functions
        vm.getUsers = getUsers;
        vm.getcurrentUserName = getcurrentUserName;
        vm.getUsers = getUsers;
        vm.getConversation = getConversation;
        vm.getConversationByUserName = getConversationByUserName;

        vm.sendMessage = sendMessage;
        vm.receiveMessage = receiveMessage;

        init();
        /////////////////

        function init() {

            vm.getUsers();
            vm.getcurrentUserName();

            messengerHub.client.addNewMessageToPage = function (name, message) {
                var messageObj = {
                    'text': message
                };
                if (vm.currentUserName.userName === name) {
                    buildSent(messageObj);
                } else {
                    buildReceived(messageObj);
                }
            };

            messengerHub.client.onNewUserConnected = function (userName) {
                console.log("User connected: " + userName);

                var userLink = $(document.getElementById(userName));
                userLink.find('.status-icon').css("background", "green");
            };

            messengerHub.client.onUserDisconnected = function (userName) {
                console.log("User disconnected: " + userName);

                var userLink = $(document.getElementById(userName));
                userLink.find('.status-icon').css("background", "red");
            };

            messengerHub.client.onConnected = function (userNames) {
                console.log("Messenger users list: " + userNames);
                userNames.forEach(function (userName) {
                    messengerHub.client.onNewUserConnected(userName);
                });
            };
        }

        function getUsers() {
            messengerService.getUsers()
                .then(function (users) {
                    vm.users = users;
                });
        }

        function getcurrentUserName() {
            messengerService.getCurrentUser()
                .then(function (user) {
                    console.log("current user: ", user);
                    vm.currentUserName = user;
                });
        }

        function getConversation(subject) {
            messengerService.getConversation(subject)
                .then(function (conversation) {
                    vm.currentConversation = conversation;
                });
        }

        function getConversationByUserName(userName) {
            vm.currentReceiverName = userName;
            messengerService.getConversationByUserName(userName)
                .then(function (conversation) {
                    console.log('new conversation : ', conversation);
                    vm.currentConversation = conversation;
                });
        }

        function sendMessage() {

            // Create the new message for sending
            var message = {};

            message.Id = vm.currentConversation[vm.currentConversation.length - 1].Id + 1;
            message.FromId = vm.currentUserName;
            message.ToId = vm.currentReceiverName;
            message.Subject = vm.currentUserName;
            message.Content = vm.messageText;
            message.DateReceived = new Date();
            console.log('sending: ', message);

            vm.currentConversation.push(message);

            // Send data to server
            messengerHub.server.send(JSON.stringify(message));
            vm.messageText = "";
        }

        function checkEnterPressed($event) {
            var keyCode = $event.which || $event.keyCode;
            //if enter was pressed
            if (keyCode === 13) {
                $event.preventDefault();
                sendMessage();
            }
        }

        function generateMessageSubject() {
            // TODO: find a better way to id conversation subject

            console.debug("subject: " + subject);
            return subject;
        }

        function receiveMessage(messageJson) {

            // Create the new message for sending
            var message = JSON.parse(messageJson);
            console.log('receiving: ', message);
            //TODO: check subject && sender
            vm.currentConversation.push(message);
        }
    }
})();