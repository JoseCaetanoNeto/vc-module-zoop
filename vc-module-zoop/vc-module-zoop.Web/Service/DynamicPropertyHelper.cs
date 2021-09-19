using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace Zoop.Web
{
    public static class DynamicPropertyHelper
    {
        public static void SetDynamicProp(this IList<DynamicProperty> resultSearch, PaymentIn pPayment, string pName, object pValue)
        {
            var property = pPayment.DynamicProperties.FirstOrDefault(o => o.Name == pName);
            if (property == null)
            {
                if (pPayment.DynamicProperties.IsReadOnly)
                    pPayment.DynamicProperties = new List<DynamicObjectProperty>();

                property = new DynamicObjectProperty { Name = pName };
                pPayment.DynamicProperties.Add(property);
            }
            var prop = resultSearch.FirstOrDefault(o => o.Name == pName);
            property.ValueType = GetValueType(pValue);
            property.Values = new List<DynamicPropertyObjectValue>(new[] { new DynamicPropertyObjectValue { Value = pValue, PropertyId = prop.Id } });
        }

        private static DynamicPropertyValueType GetValueType(object pValue)
        {
            return (pValue is int ? DynamicPropertyValueType.Integer : pValue is decimal ? DynamicPropertyValueType.Decimal : pValue is DateTime ? DynamicPropertyValueType.DateTime : pValue is bool ? DynamicPropertyValueType.Boolean : DynamicPropertyValueType.ShortText);
        }
    }
}
