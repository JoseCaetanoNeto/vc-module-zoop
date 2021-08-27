using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Zoop.Web.Managers;
using VirtoCommerce.PaymentModule.Core.Services;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Zoop.Web.Services;

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
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            // register settings
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

            var ZoopOptions = appBuilder.ApplicationServices.GetRequiredService<IOptions<ZoopSecureOptions>>();
            var paymentMethodsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPaymentMethodsRegistrar>();
            paymentMethodsRegistrar.RegisterPaymentMethod(() => new ZoopMethod(ZoopOptions));
            settingsRegistrar.RegisterSettingsForType(ModuleConstants.Settings.Zoop.Settings, nameof(ZoopMethod));
        }

        public void Uninstall()
        {
            // do nothing in here
        }
    }
}
