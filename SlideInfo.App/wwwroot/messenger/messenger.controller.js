(function () {
    "use strict";
    angular.module("messenger")
        .controller("messengerController", messengerController);

    messengerController.$inject = ["$scope", "$anchorScroll", "messengerService", "messengerHub", "ngNotify"];

    function messengerController($scope, $anchorScroll, messengerService, messengerHub, ngNotify) {
        var vm = this;

        vm.autoScrollDown = true;
        vm.showUsers = true;
        vm.showConversations = false;

        vm.currentUser = {};
        vm.currentReceiver = {};
        vm.messageText = "";

        vm.users = [];
        vm.conversations = [];

        vm.currentConversation = {};

        vm.messageList = $(document.getElementById("message-content"));

        //controller functions
        vm.getUsers = getUsers;
        vm.getConversations = getConversations;
        vm.getCurrentUser = getCurrentUser;
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
        }

        function hasScrollReachedTop() {
            return vm.messageList.scrollTop() === 0;
        }

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
        }

        function init() {

            vm.getUsers();
            vm.getCurrentUser();
            vm.getConversations();

            vm.messageList.bind("scroll", _.throttle(watchScroll, 250));

            messengerHub.client.addNewMessageToPage = function (message) {
                console.log("adding message to the page...");
                receiveMessage(message);
            };

            messengerHub.client.onNewUserConnected = function (userId) {
                console.log("User connected: " + userId);

                var userLink = $(document.getElementById(userId));
                userLink.find('.status-icon').css("background", "green");
            };

            messengerHub.client.onUserDisconnected = function (userId) {
                console.log("User disconnected: " + userId);

                var userLink = $(document.getElementById(userId));
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
                    console.log("Users: ", users);
                    vm.users = users;
                });
        }

        function getCurrentUser() {
            messengerService.getCurrentUser()
                .then(function (currentUser) {
                    console.log("Current user: ", currentUser);
                    vm.currentUser = currentUser;
                });
        }

        function getConversations(userId) {
            messengerService.getConversations(userId)
                .then(function (conversations) {
                    console.log("Conversations: ", conversations);
                    vm.conversations = conversations;
                });
        }

        function getConversation(subject) {
            vm.currentReceiver = _.find(vm.users, function (user) {
                return user.PrivateConversationSubject === subject;
            });
            vm.currentReceiver.UnreadMessagesCount = 0;

            messengerService.getConversation(subject)
                .then(function (conversation) {
                    console.log("Got conversation: ", conversation);
                    vm.currentSubject = conversation.Subject;
                    console.log("Current subject: ", vm.currentSubject);
                    vm.currentConversation = conversation.Messages;
                    console.log("Current receiver: ", vm.currentReceiver);
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

            // Create the new message for sending - leaving message.Id empty
            var message = {};

            message.FromId = vm.currentUser.Id;
            message.ToId = vm.currentReceiver.Id;
            console.log("message.ToId = ", vm.currentReceiver.Id);
            message.Subject = vm.currentSubject;
            message.Content = vm.messageText;
            message.DateSent = new Date();
            console.log("Sending: ", message);

            var messageJson = JSON.stringify(message);
            // Send data to server without id
            messengerHub.server.send(messageJson);
            messengerService.saveMessage(messageJson);

            // clear the input
            vm.messageText = "";

            if (!_.isEmpty(vm.currentConversation)) {
                message.Id = _.last(vm.currentConversation).Id + 1;
            }
            else {
                message.Id = 1;
            }
            vm.currentConversation.push(message);
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
            var message = JSON.parse(messageJson);
            console.log("Received: ", message);

            //push to current conversation
            if (message.FromId === vm.currentReceiver.Id) {
                if (!_.isEmpty(vm.currentConversation)) {
                    message.Id = _.last(vm.currentConversation).Id + 1;
                }
                else {
                    message.Id = 1;
                }
                vm.currentConversation.push(message);
            } else {
                //show unread message badge
                var sender = _.find(vm.users, function (user) {
                    return user.Id === message.FromId;
                });
                sender.UnreadMessagesCount++;
            }
            $scope.$apply();
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