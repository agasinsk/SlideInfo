(function () {
    "use strict";
    angular.module("messenger")
        .controller("messengerController", messengerController);

    messengerController.$inject = ["$scope", "$anchorScroll", "messengerService", "messengerHub", "ngNotify"];

    function messengerController($scope, $anchorScroll, messengerService, messengerHub, ngNotify) {
        var vm = this;

        vm.autoScrollDown = true;

        vm.currentUserName = "";
        vm.currentReceiverName = "";
        vm.messageText = "";

        vm.users = [];
        vm.currentConversation = [];

        vm.messageList = $(document.getElementById("message-content"));

        //controller functions
        vm.getUsers = getUsers;
        vm.getCurrentUserName = getCurrentUserName;
        vm.getUsers = getUsers;
        vm.getConversation = getConversation;

        vm.sendMessage = sendMessage;
        vm.checkEnterPressed = checkEnterPressed;
        vm.receiveMessage = receiveMessage;

        vm.listDidRender = listDidRender;

        init();
        /////////////////

        function hasScrollReachedBottom() {
            var bool = vm.messageList.scrollTop() + vm.messageList.innerHeight() >= vm.messageList.prop("scrollHeight");
            return bool;
        };

        function hasScrollReachedTop() {
            return vm.messageList.scrollTop() === 0;
        };

        function watchScroll() {
            if (hasScrollReachedTop()) {

                ngNotify.set("All the messages have been loaded", {
                    type: "grimace",
                    target: "#message-content"
                });

                //fetchPreviousMessages();

            }
            vm.autoScrollDown = hasScrollReachedBottom();
            console.log("vm.autoScrollDown = ", vm.autoScrollDown);
        };

        function init() {

            vm.getUsers();
            vm.getCurrentUserName();
            vm.messageList.bind("scroll", _.throttle(watchScroll, 250));

            messengerHub.client.addNewMessageToPage = function (message) {
                console.log("adding message to the page...");
                receiveMessage(message);
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

            messengerHub.client.onConnected = function (connectedUsers) {
                console.log("Messenger users list: " + connectedUsers);
                connectedUsers.forEach(function (userName) {
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
                    vm.currentSubject = conversation.Subject;
                    vm.currentConversation = conversation.Messages;
                    vm.currentReceiverName = subject;
                });
        }

        function fetchPreviousMessages() {
            ngNotify.set("Loading previous messages...", {
                type: "success",
                target: "#message-content"
            });
            var firstLoadedMessageId = vm.currentConversation[0].Id.toString();

            //scope.messages.$load(10).then(function (m) {
            //    // Scroll to the previous message 
            //    _.defer(function () { $anchorScroll(currentMessage) });
            //});
            console.log("loading more messages/// bakcend working hard");
        }

        function sendMessage() {

            // Create the new message for sending
            var message = {};

            message.Id = _.last(vm.currentConversation).Id + 1;
            message.FromId = vm.currentUserName;
            console.log("message.FromId = ", vm.currentReceiverName);
            message.ToId = vm.currentReceiverName;
            console.log("message.ToId = ", vm.currentReceiverName);
            message.Subject = vm.conversationSubject;
            message.Content = vm.messageText;
            message.DateReceived = new Date();
            console.log("sending: ", message);

            vm.currentConversation.push(message);
            // Send data to server
            messengerHub.server.send(JSON.stringify(message));
            // clear the input
            vm.messageText = "";
        }

        function checkEnterPressed($event) {
            var keyCode = $event.which || $event.keyCode;

            //if enter was pressed, send the message
            if (keyCode === 13) {
                $event.preventDefault();
                sendMessage();
            }
        }

        function receiveMessage(messageJson) {

            // Create the new message for sending
            var message = JSON.parse(messageJson);
            console.log('receiving: ', message);
            if (message.Subject === vm.currentSubject && message.FromId === vm.currentReceiverName) {
                vm.currentConversation.push(message);
            } else {
                var sender = _.find(vm.users, function (user) { return user.UserName === message.FromId });
                sender.UnreadMessagesCount++;
                console.log("sender ", sender);
                $scope.$apply();
            }
        }

        function scrollToBottom() {
            var lastMessageId = _.last(vm.currentConversation).Id;
            $anchorScroll(lastMessageId);
        }

        function listDidRender() {
            if (vm.autoScrollDown) {
                scrollToBottom();
            }
        }

    }
})();