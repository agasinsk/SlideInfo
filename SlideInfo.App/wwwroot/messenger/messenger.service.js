(function () {
    'use strict';
    angular.module('messenger')
        .factory('messengerService', messengerService);

    messengerService.$inject = ['$http'];

    function messengerService($http) {
        var service = {
            getUsers: getUsers,
            getConversation: getConversation,
            getMessage: getMessage,
            get: get
        };

        function get() {
            return $http.get('/api/Products')
                .then(function (response) {
                    return response.data;
                });
        }

        return service;
    }
})();