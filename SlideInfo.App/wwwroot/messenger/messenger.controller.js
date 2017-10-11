(function () {
    'use strict';
    angular.module('messenger').controller('messengerController', messengerController);

    messengerController.$inject = ['$scope', 'productsService', 'productMessageHub'];

    function messengerController($scope, productsService, productMessageHub) {
        var vm = this;

        vm.applicationBlocked = false;
        vm.tableBlocked = false;
        vm.selectedProduct = null;
        vm.newProductAdded = false;

        vm.messagesList = [];

        vm.tableRowClick = tableRowClick;
        vm.addNewProduct = addNewProduct;
        vm.saveProduct = saveProduct;
        vm.deleteProduct = deleteProduct;

        init();

        function init() {
            productsService.get()
                .then(function (products) {
                    vm.products = products;
                });

            // Method which receives data.
            productMessageHub.client.handleProductMessage = function (message) {
                // Method which handles messages.
                receivedMessageHandler(message);
            };
        }

        function tableRowClick(product) {
            if (vm.tableBlocked === true) {
                return;
            }

            if (vm.newProductAdded === true && vm.tableBlocked === false) {
                vm.tableBlocked = true;
                return;
            }

            vm.selectedProduct = product;
        }

        function addNewProduct() {
            var newProduct = { id: null, name: null, description: null };
            vm.products.push(newProduct);
            vm.selectedProduct = newProduct;
            vm.newProductAdded = true;
            vm.tableBlocked = true;
        }

        function saveProduct() {
            vm.applicationBlocked = true;

            if (vm.newProductAdded === true) {
                // Message type – 1, data for insert.
                sendProductDataMessage(1);
            } else {
                // Message type – 2, data for update.
                sendProductDataMessage(2);
            }
        }

        function deleteProduct() {
            vm.applicationBlocked = true;

            if (vm.newProductAdded === true) {
                vm.removeProductById(vm.selectedProduct.id);
                vm.resetState();
            } else {
                // Message type – 3, data for delete.
                sendProductDataMessage(3);
            }
        }

        function sendProductDataMessage(messageType) {

            // Create the new message for sending.
            var productDataMessage = {};
            productDataMessage.Product = {};

            // Set message type.
            productDataMessage.MessageType = messageType;

            // Set message data.
            productDataMessage.Product.Id = vm.selectedProduct.id;
            productDataMessage.Product.Name = vm.selectedProduct.name;
            productDataMessage.Product.Description = vm.selectedProduct.description;

            // Send data to server.
            productMessageHub.server.handleProductMessage(JSON.stringify(productDataMessage));
        }

        function receivedMessageHandler(productDataMessageJsonString) {
            var productDataMessage = JSON.parse(productDataMessageJsonString);
            vm.applicationBlocked = false;

            if (productDataMessage.DataProcessedSuccessfully) {

                switch (productDataMessage.MessageType) {
                    case 1: // New record.
                        insertProduct(productDataMessage.Product);
                        break;
                    case 2: // Update existing record.
                        updateProduct(productDataMessage.Product);
                        break;
                    case 3: // Delete record.
                        removeProductById(productDataMessage.Product.Id);
                        resetState();
                        break;
                    default:
                        return;
                }
            }

            setOperationResulStatus(productDataMessage.ResponseMessage);
        }

        function resetState() {
            vm.tableBlocked = false;
            vm.selectedProduct = null;
            vm.newProductAdded = false;
        }

        function setOperationResulStatus(statusString) {
            var date = new Date();
            var dateString = date.getHours() + ':' + date.getMinutes() + ':' + date.getSeconds();
            vm.messagesList.push({ dateString: dateString, statusString: statusString });
            $scope.$apply();
        }

        function insertProduct(product) {
            if (getProductById(product.Id) === null) {
                var newProduct = {
                    id: product.Id,
                    name: product.Name,
                    description: product.Description
                };
                vm.products.push(newProduct);
            } else {
                updateProduct(product);
                vm.tableBlocked = false;
                vm.newProductAdded = false;
            }
        }

        function updateProduct(updatedProduct) {
            var product = getProductById((updatedProduct.Id));
            product.name = updatedProduct.Name;
            product.description = updatedProduct.Description;
        }

        function removeProductById(productId) {
            var i = vm.products.length;
            var copy = vm.products.slice();

            while (i--) {
                if (copy[i].id === productId) {
                    copy.splice(i, 1);
                    vm.products = copy;
                    return;
                }
            }
        }

        function getProductById(productId) {
            for (var i = 0; i < vm.products.length; i++) {
                if (vm.products[i].id === productId) {
                    return vm.products[i];
                }
            }

            return null;
        }
    }
})();