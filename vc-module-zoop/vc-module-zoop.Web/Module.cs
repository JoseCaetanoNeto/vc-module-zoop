using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Zoop.Web.Managers;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Zoop.Web.Services;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.DynamicProperties;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.Platform.Core.Security;
using Zoop.Web.Validation;
using FluentValidation;
using VirtoCommerce.OrdersModule.Core.Model;

namespace Zoop.Web
{
    public class Module : IModule
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            var snapshot = serviceCollection.BuildServiceProvider();
            var configuration = snapshot.GetService<IConfiguration>();

            serviceCollection.AddOptions<ZoopSecureOptions>().Bind(configuration.GetSection("Payments:Zoop")).ValidateDataAnnotations();
            serviceCollection.AddTransient<IZoopRegisterPaymentService, ZoopRegisterPaymentService>();
            serviceCollection.AddTransient<IValidator<PaymentIn>, ZoopPaymentInValidator>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            // register settings
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);
            settingsRegistrar.RegisterSettingsForType(ModuleConstants.Settings.Zoop.Settings, nameof(ZoopMethodCard));
            settingsRegistrar.RegisterSettingsForType(ModuleConstants.Settings.ZoopBoleto.Settings, nameof(ZoopMethodBoleto));

            var ZoopOptions = appBuilder.ApplicationServices.GetRequiredService<IOptions<ZoopSecureOptions>>();
            var paymentMethodsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPaymentMethodsRegistrar>();
            var customer = appBuilder.ApplicationServices.GetRequiredService<IMemberService>();
            var dynamicPropertySearchService = appBuilder.ApplicationServices.GetRequiredService<IDynamicPropertySearchService>();
            var userManagerService = appBuilder.ApplicationServices.GetRequiredService<UserManager<ApplicationUser>>();
            
            paymentMethodsRegistrar.RegisterPaymentMethod(() => new ZoopMethodCard(ZoopOptions, dynamicPropertySearchService));
            paymentMethodsRegistrar.RegisterPaymentMethod(() => new ZoopMethodBoleto(ZoopOptions, dynamicPropertySearchService, customer, userManagerService));
        }

        public void Uninstall()
        {
            // do nothing in here
        }
    }
}
