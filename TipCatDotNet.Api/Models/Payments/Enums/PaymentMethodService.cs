using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TipCatDotNet.Api.Models.Payments.Enums
{
    public static class PaymentMethodService
    {
        public static List<string> GetAllowed()
        {
            var t = typeof(PaymentMethods);
            return Enum.GetValues(t).Cast<Enum>().Select(x => x.GetDescription()).ToList();
        }


        private static string GetDescription(this Enum value)
        {
            var type = value.GetType();

            var name = Enum.GetName(type, value);
            if (name is null)
                return value.ToString();

            var field = type.GetField(name);
            if (field is null)
                return value.ToString();

            var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (attr is null)
                return value.ToString();

            return attr.Description;
        }


        private enum PaymentMethods
        {
            [Description("card")]
            Card,
            // [Description("card_present")]
            // CardPresent,
            // [Description("acss_debit")]
            // ACSSDebit,
            // [Description("afterpay_clearpay")]
            // AfterPayClearPay,
            // [Description("alipay")]
            // Alipay,
            // [Description("au_becs_debit")]
            // AuBECSDebit,
            // [Description("bacs_debit")]
            // BACSDebit,
            // [Description("bancontact")]
            // BanContact,
            // [Description("boleto")]
            // Boleto,
            // [Description("eps")]
            // EPS,
            // [Description("fpx")]
            // FPX,
            // [Description("giropay")]
            // Giropay,
            // [Description("grabpay")]
            // Grabpay,
            // [Description("ideal")]
            // Ideal,
            // [Description("interac_present")]
            // InteracPresent,
            // [Description("klarna")]
            // Klarna,
            // [Description("oxxo")]
            // OXXO,
            // [Description("p24")]
            // P24,
            // [Description("sepa_debit")]
            // SepaDebit,
            // [Description("sofort")]
            // Sofort,
            // [Description("wechat_pay")]
            // Wechatpay
        }
    }
}