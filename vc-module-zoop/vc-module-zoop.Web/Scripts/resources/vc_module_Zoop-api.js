angular.module('vcmoduleZoop')
    .factory('vcmoduleZoop.webApi', ['$resource', function ($resource) {
        return $resource('api/vcmoduleZoop');
}]);
