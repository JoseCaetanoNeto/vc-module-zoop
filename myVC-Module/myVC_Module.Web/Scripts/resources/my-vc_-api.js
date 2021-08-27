angular.module('myVC')
    .factory('myVC.webApi', ['$resource', function ($resource) {
        return $resource('api/myVC');
}]);
