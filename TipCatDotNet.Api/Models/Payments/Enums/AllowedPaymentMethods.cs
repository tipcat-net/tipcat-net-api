using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TipCatDotNet.Api.Models.Payments.Enums
{
    public static class AllowedPaymentMethods
    {
        public static List<string> ToList()
        {
            var t = typeof(PaymentMethods);
            return Enum.GetValues(t).Cast<Enum>().Select(x => x.GetDescription()).ToList();
        }


        private static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name != null)
            {
                var field = type.GetField(name);
                if (field != null)
                {
                    var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return value.ToString();
        }


        private enum PaymentMethods
        {
            [Description("card")]
            Card,
            // [Description("card_present")]
            // CardPresent,
            // [Description("acss_debit")]
            // ACSSdebit,
            // [Description("afterpay_clearpay")]
            // AfterPayClearPay,
            // [Description("alipay")]
            // Alipay,
            // [Description("au_becs_debit")]
            // AuBECSdebit,
            // [Description("bacs_debit")]
            // BACSdebit,
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
            // Sepadebit,
            // [Description("sofort")]
            // Sofort,
            // [Description("wechat_pay")]
            // Wechatpay
        }
    }
}