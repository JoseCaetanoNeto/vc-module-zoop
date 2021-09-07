using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace Zoop.Web
{
    public static class ModuleConstants
    {
        public static class Settings
        {
            public static class ZoopBoleto
            {

                public static readonly SettingDescriptor DefaultSaller = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.ZoopBoleto.DefaultSaller",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "informe código do vendedor padrão da zoop"
                };

                public static readonly SettingDescriptor VCmanagerURL = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.ZoopBoleto.VCmanagerURL",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.SecureString,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor statusOrderOnWaitingConfirm = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.ZoopBoleto.statusOrderOnWaitingConfirm",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "Pending"
                };

                public static readonly SettingDescriptor statusOrderOnOverdue = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.ZoopBoleto.statusOrderOnOverdue",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "Unpaid"
                };

                public static readonly SettingDescriptor statusOrderOnPaid = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.ZoopBoleto.statusOrderOnPaid",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "Processing"
                };

                public static readonly SettingDescriptor interestMode = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.ZoopBoleto.interestMode",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    AllowedValues = new[] { "DAILY_AMOUNT", "DAILY_PERCENTAGE" , "MONTHLY_PERCENTAGE" },
                    DefaultValue = "DAILY_AMOUNT"
                };

                public static readonly SettingDescriptor interestAmount = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.ZoopBoleto.interestAmount",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.Decimal,
                    DefaultValue = 0d
                };

                public static readonly SettingDescriptor lateFeeMode = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.ZoopBoleto.lateFeeMode",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    AllowedValues = new[] { "PERCENTAGE", "FIXED" },
                    DefaultValue = "PERCENTAGE"
                };

                public static readonly SettingDescriptor lateFeeAmount = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.ZoopBoleto.lateFeeAmount",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.Decimal,
                    DefaultValue = 0d
                };


                public static readonly SettingDescriptor ExpirationInDays = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.ZoopBoleto.ExpirationInDays",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.PositiveInteger,
                    DefaultValue = 2
                };

                public static readonly SettingDescriptor PaymentLimitInDays = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.ZoopBoleto.PaymentLimitInDays",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.PositiveInteger,
                    DefaultValue = 2
                };

                public static readonly SettingDescriptor Description = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.ZoopBoleto.Description",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor UrlLogo = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.ZoopBoleto.UrlLogo",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = ""
                };

                public static readonly SettingDescriptor BodyInstructions = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.ZoopBoleto.BodyInstructions",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "Pedido :#order.Number"
                };

                public static IEnumerable<SettingDescriptor> Settings
                {
                    get
                    {
                        return new List<SettingDescriptor>
                        {
                            DefaultSaller,
                            statusOrderOnWaitingConfirm,
                            statusOrderOnOverdue,
                            statusOrderOnPaid,
                            interestMode,
                            interestAmount,
                            lateFeeMode,
                            lateFeeAmount,
                            ExpirationInDays,
                            PaymentLimitInDays,
                            Description,
                            UrlLogo,
                            BodyInstructions,
                            VCmanagerURL,
                        };
                    }
                }

            }
            public static class Zoop
            {

                public static readonly SettingDescriptor DefaultSaller = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Zoop.DefaultSaller",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "informe código do vendedor padrão da zoop"
                };

                public static readonly SettingDescriptor MaxNumberInstallments = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Payment.Zoop.MaxNumberInstallments",
                    GroupName = "Payment|Zoop",
                    ValueType = SettingValueType.PositiveInteger,
                    DefaultValue = 1
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
                            MaxNumberInstallments,
                        };
                    }
                }
            }

            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    var list = new List<SettingDescriptor>();
                    list.AddRange(Zoop.Settings);
                    list.AddRange(ZoopBoleto.Settings);
                    return list;
                }
            }
        }
    }
}
