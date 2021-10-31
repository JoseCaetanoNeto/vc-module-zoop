// Call this to register your module to main application
var moduleName = "vcmoduleZoop";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider
                .state('workspace.vcmoduleZoopState', {
                    url: '/vcmoduleZoop',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        'platformWebApp.bladeNavigationService', function (bladeNavigationService) {
                            var newBlade = {
                                id: 'blade1',
                                controller: 'vcmoduleZoop.helloWorldController',
                                template: 'Modules/$(vc_module_zoop)/Scripts/blades/hello-world.html',
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
                path: 'browse/vcmoduleZoop',
                icon: 'fa fa-cube',
                title: 'vc-module-Zoop',
                priority: 100,
                action: function () { $state.go('workspace.vcmoduleZoopState'); },
                permission: 'vcmoduleZoop:access'
            };
            mainMenuService.addMenuItem(menuItem);
        }
    ]);
