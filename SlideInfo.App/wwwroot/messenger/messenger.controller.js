(function () {
    "use strict";
    angular.module("messenger")
        .controller("messengerController", messengerController);

    messengerController.$inject = ["$scope", "$anchorScroll", "messengerService", "messengerHub"];

    function messengerController($scope, $anchorScroll, messengerService, messengerHub) {
        var vm = this;

        vm.autoScrollDown = true;

        vm.currentUserName = {};
        vm.currentReceiverName = {};
        vm.messageText = "";

        vm.users = [];
        vm.currentConversation = [];

        vm.messageList = angular.element("message-content");

        //controller functions
        vm.getUsers = getUsers;
        vm.getCurrentUserName = getCurrentUserName;
        vm.getUsers = getUsers;
        vm.getConversation = getConversation;
        vm.getConversationByUserName = getConversationByUserName;

        vm.sendMessage = sendMessage;
        vm.receiveMessage = receiveMessage;

        vm.listDidRender = listDidRender;

        init();
        /////////////////

        var hasScrollReachedBottom = function () {
            return vm.messageList.scrollTop() + vm.messageList.innerHeight() >= vm.messageList.prop('scrollHeight');
        };

        var watchScroll = function () {
            scope.autoScrollDown = hasScrollReachedBottom();
        };

        var hasScrollReachedTop = function () {
            return vm.messageList.scrollTop() === 0;
        };

        function init() {

            vm.getUsers();
            vm.getCurrentUserName();
            vm.messageList.bind("scroll", _.throttle(watchScroll, 250));

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

        function getCurrentUserName() {
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
                    scrollToBottom();
                });
        }

        function getConversationByUserName(userName) {
            vm.currentReceiverName = userName;
            messengerService.getConversationByUserName(userName)
                .then(function (conversation) {
                    console.log('new conversation : ', conversation);
                    vm.currentConversation = conversation;
                    scrollToBottom();
                });
        }

        function sendMessage() {

            // Create the new message for sending
            var message = {};

            message.Id = _.last(vm.currentConversation).Id + 1;
            message.FromId = vm.currentUserName;
            message.ToId = vm.currentReceiverName;
            message.Subject = vm.currentUserName;
            message.Content = vm.messageText;
            message.DateReceived = new Date();
            console.log('sending: ', message);

            vm.currentConversation.push(message);
            scrollToBottom();
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

        function scrollToBottom() {
            var lastMessageId = _.last(vm.currentConversation).Id;
            var container = document.getElementById("messages-content");
            var scrollTo = document.getElementById(lastMessageId);
            container.scrollTop = scrollTo.offsetTop;
        };

        function listDidRender() {
            if (vm.autoScrollDown) {
                scrollToBottom();
            }
        };

    }
})();