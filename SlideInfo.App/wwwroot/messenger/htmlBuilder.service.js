(function () {
    "use strict";
    angular.module("messenger")
        .factory("htmlBuilder", htmlBuilder);

    function htmlBuilder() {

        this.messageWrapper = "message-wrapper";
        this.circleWrapper = "circle-wrapper";
        this.textWrapper = "text-wrapper";

        this.meClass = "me";
        this.themClass = "them";

        var service = {
            build: build,
            me: me,
            them: them
        };

        function build(text, who) {
            return '<div class="' +
                messageWrapper +
                " " +
                this[who + "Class"] +
                '">\n <div class="' +
                circleWrapper +
                ' animated bounceIn"></div>\n <div class="' +
                textWrapper +
                '">...</div>\n </div>';
        };

        function me(text) {
            return build(text, "me");
        };

        function them(text) {
            return build(text, "them");
        };

        return service;
    }
})();