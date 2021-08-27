// Call this to register your module to main application
var moduleName = "myVC";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider
                .state('workspace.myVCState', {
                    url: '/myVC',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        'platformWebApp.bladeNavigationService', function (bladeNavigationService) {
                            var newBlade = {
                                id: 'blade1',
                                controller: 'myVC.helloWorldController',
                                template: 'Modules/$(myVC)/Scripts/blades/hello-world.html',
                                isClosingDisabled: true
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])

    .run(['platformWebApp.mainMenuService', 'platformWebApp.widgetService', '$state',
        function (mainMenuService, widgetService, $state) {
            //Register module in main menu
            var menuItem = {
                path: 'browse/myVC',
                icon: 'fa fa-cube',
                title: 'myVC-Module',
                priority: 100,
                action: function () { $state.go('workspace.myVCState'); },
                permission: 'myVC:access'
            };
            mainMenuService.addMenuItem(menuItem);
        }
    ]);
