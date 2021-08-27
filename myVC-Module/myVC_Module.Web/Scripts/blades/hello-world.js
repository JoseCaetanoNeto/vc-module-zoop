angular.module('myVC')
    .controller('myVC.helloWorldController', ['$scope', 'myVC.webApi', function ($scope, api) {
        var blade = $scope.blade;
        blade.title = 'myVC-Module';

        blade.refresh = function () {
            api.get(function (data) {
                blade.title = 'myVC.blades.hello-world.title';
                blade.data = data.result;
                blade.isLoading = false;
            });
        };

        blade.refresh();
    }]);
