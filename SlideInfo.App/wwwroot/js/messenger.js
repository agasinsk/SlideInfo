'use strict';

function _classCallCheck(instance, Constructor) {
    if (!(instance instanceof Constructor)) {
        throw new TypeError("Cannot call a class as a function");
    }
}

var Messenger = function () {
    function Messenger() {
        _classCallCheck(this, Messenger);

        this.messageList = [];
        this.deletedList = [];

        this.me = 
        this.them = 5; // and another one

        this.onReceive = function (message) {
            return console.log('Received: ' + message.text);
        };
        this.onSend = function (message) {
            return console.log('Sent: ' + message.text);
        };
        this.onDelete = function (message) {
            return console.log('Deleted: ' + message.text);
        };
    }

    Messenger.prototype.send = function send() {
        var text = arguments.length <= 0 || arguments[0] === undefined ? '' : arguments[0];

        text = this.filter(text);

        if (this.validate(text)) {
            var message = {
                user: this.me,
                text: text,
                time: new Date().getTime()
            };

            this.messageList.push(message);

            this.onSend(message);
        }
    };

    Messenger.prototype.receive = function receive() {
        var text = arguments.length <= 0 || arguments[0] === undefined ? '' : arguments[0];

        text = this.filter(text);

        if (this.validate(text)) {
            var message = {
                user: this.them,
                text: text,
                time: new Date().getTime()
            };

            this.messageList.push(message);

            this.onReceive(message);
        }
    };

    Messenger.prototype.delete = function _delete(index) {
        index = index || this.messageLength - 1;

        var deleted = this.messageLength.pop();

        this.deletedList.push(deleted);
        this.onDelete(deleted);
    };

    Messenger.prototype.filter = function filter(input) {
        var output = input.replace('bad input', 'good output'); // such amazing filter there right?
        return output;
    };

    Messenger.prototype.validate = function validate(input) {
        return !!input.length; // an amazing example of validation I swear.
    };

    return Messenger;
}();

var BuildHtml = function () {
    function BuildHtml() {
        _classCallCheck(this, BuildHtml);

        this.messageWrapper = 'message-wrapper';
        this.circleWrapper = 'circle-wrapper';
        this.textWrapper = 'text-wrapper';

        this.meClass = 'me';
        this.themClass = 'them';
    }

    BuildHtml.prototype._build = function build(text, who) {
        return '<div class="' +
            this.messageWrapper +
            ' ' +
            this[who + 'Class'] +
            '">\n              <div class="' +
            this.circleWrapper +
            ' animated bounceIn"></div>\n              <div class="' +
            this.textWrapper +
            '">...</div>\n            </div>';
    };

    BuildHtml.prototype.me = function me(text) {
        return this._build(text, 'me');
    };

    BuildHtml.prototype.them = function them(text) {
        return this._build(text, 'them');
    };

    return BuildHtml;
}();

var model;

function getViewModelData(response) {
    model = response;
}

$(function () {
    var chat = $.connection.messenger;
    // Start Hub
    $.connection.hub.start().done(function () {

        console.log("Testing after signal r start");

    });
    var messenger = new Messenger();
    var buildHtml = new BuildHtml();
    var currentUsername = model.userName;
    
    chat.client.addNewMessageToPage = function (name, message) {
        console.log("name test: ", name);
        console.log("message test: ", message);
        var messageObj = {
            'text': message
        }
        if (currentUsername === name) {
            buildSent(messageObj);
        } else {
            buildReceived(messageObj);
        }
    };

    var $input = $('#message-input');
    var $send = $('#send-message');
    var $content = $('#content');
    var $inner = $('#messages-inner');


    function saveText(text) {
        $content.find('.message-wrapper').last().find('.text-wrapper').text(text);
    }

    function animateText() {
        setTimeout(function () {
            $content.find('.message-wrapper').last().find('.text-wrapper').addClass('animated fadeIn');
        },
            350);
    }

    function scrollBottom() {
        $($inner).animate({
            scrollTop: $($content).offset().top + $($content).outerHeight(true)
        },
            {
                queue: false,
                duration: 'ease'
            });
    }

    function buildSent(message) {
        console.log('sending: ', message.text);

        $content.append(buildHtml.me(message.text));
        saveText(message.text);
        animateText();

        scrollBottom();
    }

    function buildReceived(message) {
        console.log('receiving: ', message.text);

        $content.append(buildHtml.them(message.text));
        saveText(message.text);
        animateText();

        scrollBottom();
    }

    function sendMessage() {
        var text = $input.val();
        // Call the Send method on the hub.
        chat.server.send(model.receiverUserName, text);

        $input.val('');
        $input.focus();
    }

    messenger.onSend = buildSent;
    messenger.onReceive = buildReceived;

    $input.focus();

    $send.on('click',
        function (e) {
            sendMessage();
        });

    $input.on('keydown',
        function (e) {
            var key = e.which || e.keyCode;

            if (key === 13) {
                // enter key
                e.preventDefault();

                sendMessage();
            }
        });
});