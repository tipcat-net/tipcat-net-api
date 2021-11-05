using TipCatDotNet.Api.Models.Company;

namespace TipCatDotNet.Api.Models.Mailing
{
    public abstract record BaseEmail
    {
        protected BaseEmail(in CompanyInfo companyInfo)
        {
            CompanyInfo = companyInfo;
        }


        public CompanyInfo CompanyInfo { get; }
    }
}
