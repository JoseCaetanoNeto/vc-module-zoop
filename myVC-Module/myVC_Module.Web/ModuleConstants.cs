using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace Zoop.Web
{
    public static class ModuleConstants
    {
        public static class Settings
        {
            public static class Zoop
            {

                public static readonly SettingDescriptor DefaultSaller = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Zoop.DefaultSaller",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "informe código do vendedor padrão da zoop"
                };

                public static readonly SettingDescriptor MaxParc = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Zoop.Parc",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.Json,
                    DefaultValue = "{'visa':1,'cred':1}"
                };

                public static readonly SettingDescriptor VCmanagerURL = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Zoop.VCmanagerURL",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.SecureString,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor Capture = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Zoop.Capture",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = "true"
                };

                public static readonly SettingDescriptor installmentPlan = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Zoop.installmentPlan",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    AllowedValues = new[] { "interest_free", "with_interest" },
                    DefaultValue = "interest_free"
                };

                public static readonly SettingDescriptor statusOrderOnWaitingConfirm = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Zoop.statusOrderOnWaitingConfirm",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "Pending"
                };

                public static readonly SettingDescriptor statusOrderOnAuthorization = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Zoop.statusOrderOnAuthorization",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "Processing"
                };

                public static readonly SettingDescriptor statusOrderOnFailedAuthorization = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Zoop.statusOrderOnFailedAuthorization",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "Unpaid"
                };

                public static readonly SettingDescriptor statusOrderOnPreAuthorization = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Zoop.statusOrderOnPreAuthorization",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "Pending"
                };

                public static readonly SettingDescriptor statusOrderOnFailedPreAuthorization = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Zoop.statusOrderOnFailedPreAuthorization",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "Unpaid"
                };

                public static readonly SettingDescriptor statusOrderOnCancelAuthorization = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Zoop.statusOrderOnCancelAuthorization",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "Unpaid"
                };

                public static IEnumerable<SettingDescriptor> Settings
                {
                    get
                    {
                        return new List<SettingDescriptor>
                        {
                            DefaultSaller,
                            Capture,
                            installmentPlan,
                            statusOrderOnWaitingConfirm,
                            statusOrderOnAuthorization,
                            statusOrderOnFailedAuthorization,
                            statusOrderOnPreAuthorization,
                            statusOrderOnFailedPreAuthorization,
                            statusOrderOnCancelAuthorization,
                            VCmanagerURL,
                            MaxParc,
                        };
                    }
                }
            }

            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    return Zoop.Settings;
                }
            }
        }
    }
}
