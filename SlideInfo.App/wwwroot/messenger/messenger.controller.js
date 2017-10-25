(function () {
    "use strict";
    angular.module("messenger")
        .controller("messengerController", messengerController);

    messengerController.$inject = ["$scope", "$anchorScroll", "messengerService", "messengerHub", "ngNotify"];

    function messengerController($scope, $anchorScroll, messengerService, messengerHub, ngNotify) {
        var vm = this;

        vm.autoScrollDown = true;
        vm.showUsers = true;
        vm.userTyping = "";

        vm.currentUser = {};
        vm.currentReceiver = {};
        vm.messageText = "";
        vm.searchPhrase = "";

        vm.users = [];
        vm.allUsers = [];
        vm.cachedConversations = [];

        vm.currentConversation = {};
        vm.currentPage = 0;
        vm.messageList = {};

        //controller functions
        vm.getUsers = getUsers;
        vm.getCurrentUser = getCurrentUser;
        vm.getConversation = getConversation;

        vm.sendMessage = sendMessage;
        vm.checkEnterPressed = checkEnterPressed;
        vm.receiveMessage = receiveMessage;
        vm.search = search;
        vm.clearSearch = clearSearch;

        vm.listDidRender = listDidRender;
        vm.allMessagesFetched = false;

        init();
        /////////////////

        function init() {

            vm.getUsers();
            vm.getCurrentUser();

            vm.messageList = $(document.getElementById("chat-area"));
            vm.messageList.bind("scroll", _.throttle(watchScroll, 300));

            messengerHub.client.onUserTyping = function (typingUserId) {
                _.throttle(onUserTyping(typingUserId), 1000);
            };

            messengerHub.client.addNewMessageToPage = function (message) {
                console.log("Adding message to the page...");
                receiveMessage(message);
            };

            messengerHub.client.onNewUserConnected = function (userId) {
                console.log("User connected: " + userId);
                var foundUser = _.find(vm.users, user => user.Id === userId);
                if (foundUser) {
                    foundUser.IsActive = true;
                    $scope.$apply();
                }
            };

            messengerHub.client.onUserDisconnected = function (userId) {
                console.log("User disconnected: " + userId);
                var foundUser = _.find(vm.users, user => user.Id === userId);
                if (foundUser) {
                    foundUser.IsActive = false;
                    $scope.$apply();
                }
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
                    vm.allUsers = users;
                });
        }

        function getCurrentUser() {
            messengerService.getCurrentUser()
                .then(function (currentUser) {
                    console.log("Current user: ", currentUser);
                    vm.currentUser = currentUser;
                });
        }

        function getConversation(subject) {

            vm.currentReceiver = _.find(vm.users, function (user) {
                return user.PrivateConversationSubject === subject;
            });
            vm.currentReceiver.UnreadMessagesCount = 0;
            vm.currentPage = 0;
            if (!_.isEmpty(vm.cachedConversations[subject])) {
                vm.currentConversation = vm.cachedConversations[subject];
            } else {
                messengerService.getConversation(subject, vm.currentPage)
                    .then(function (conversation) {
                        conversation.Messages.reverse();
                        console.log("Got conversation: ", conversation); 
                        vm.currentConversation = conversation;
                        vm.cachedConversations[conversation.Subject] = conversation;
                    });
            }
        }

        function fetchPreviousMessages() {
            vm.currentPage++;
            ngNotify.set("Loading previous messages...", "success");
            var currentMessageId = vm.currentConversation.Messages[0].Id.toString();

            messengerService.getConversation(vm.currentConversation.Subject, vm.currentPage)
                .then(function (conversation) {
                    console.log("Got conversation page: ", conversation);
                    conversation.Messages.forEach(message => vm.currentConversation.Messages.unshift(message));
                    vm.cachedConversations[conversation.Subject].Messages = vm.currentConversation.Messages;

                    _.defer(function () {
                        $anchorScroll(currentMessageId);
                    });
                });
        }

        function sendMessage() {

            // Create the new message for sending - leaving message.Id empty
            var message = {};

            message.FromId = vm.currentUser.Id;
            message.ToId = vm.currentReceiver.Id;
            message.Subject = vm.currentConversation.Subject;
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
                message.Id = _.last(vm.currentConversation.Messages).Id + 1;
            }
            else {
                message.Id = 1;
            }
            vm.currentConversation.Messages.push(message);
        }

        function checkEnterPressed($event) {
            var keyCode = $event.which || $event.keyCode;

            //if enter was pressed, send the message
            if (keyCode === 13) {
                $event.preventDefault();
                sendMessage();
            } else {
                if (vm.currentReceiver.Id && vm.currentUser.Id) {
                    _.throttle(messengerHub.server.onUserTyping(vm.currentReceiver.Id, vm.currentUser.Id), 500);
                }
            }
        }

        function receiveMessage(messageJson) {
            var message = JSON.parse(messageJson);
            console.log("Received: ", message);

            //push to current conversation
            if (message.FromId === vm.currentReceiver.Id) {
                if (!_.isEmpty(vm.currentConversation)) {
                    message.Id = _.last(vm.currentConversation.Messages).Id + 1;
                }
                else {
                    message.Id = 1;
                }
                vm.currentConversation.Messages.push(message);
            } else {
                //show unread message badge
                var sender = _.find(vm.users, user => user.Id === message.FromId);
                sender.UnreadMessagesCount++;
            }
            $scope.$apply();
        }

        function allMessagesFetched() {
            return vm.currentConversation.Messages.length >= vm.currentConversation.MessagesCount;
        }

        function search() {
            if (vm.searchPhrase !== "") {
                var searchResults = [];
                _.each(vm.users,
                    function (user) {
                        if (messengerService.foundSearchPhrase(user, vm.searchPhrase)) {
                            searchResults.push(user);
                        }
                    });
                vm.users = searchResults;
            } else {
                vm.users = vm.allUsers;
            }
            $scope.$applyAsync();
        }

        function clearSearch() {
            vm.searchPhrase = "";
            vm.users = vm.allUsers;
            $scope.$applyAsync();
        }

        function onUserTyping(typingUserId) {
            if (vm.currentReceiver.Id === typingUserId) {
                vm.userTyping = " is typing...";
                console.log(vm.userTyping);
                window.setTimeout(function () {
                    vm.userTyping = "";
                    $scope.$applyAsync();
                }, 1500);
            }
            $scope.$applyAsync();
        }

        function watchScroll() {
            if (hasScrollReachedTop() && !_.isEmpty(vm.currentConversation)) {
                if (allMessagesFetched()) {
                    ngNotify.set("All the messages have been loaded!", "grimace");
                } else {
                    fetchPreviousMessages();
                }
            }
            vm.autoScrollDown = hasScrollReachedBottom();
            console.log("vm.autoScrollDown = ", vm.autoScrollDown);
        }

        function hasScrollReachedTop() {
            return vm.messageList.scrollTop() === 0;
        }

        function hasScrollReachedBottom() {
            var bool = vm.messageList.scrollTop() + vm.messageList.innerHeight() >= vm.messageList.prop("scrollHeight");
            return bool;
        }

        function scrollToBottom() {
            if (vm.currentConversation !== null) {
                var lastMessageId = _.last(vm.currentConversation.Messages).Id;
                $anchorScroll(lastMessageId);
            }
        }

        function listDidRender() {
            if (vm.autoScrollDown) {
                scrollToBottom();
            }
        }
    }
})();