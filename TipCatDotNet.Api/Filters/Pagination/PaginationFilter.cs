using System.Configuration;
using Newtonsoft.Json;
namespace TipCatDotNet.Api.Filters.Pagination
{
    public class PaginationFilter
    {
        public PaginationFilter()
        {
            StartFrom = defaultPage;
            Count = defaultCount;
        }


        [JsonConstructor]
        public PaginationFilter(int startFrom, int count)
        {
            StartFrom = startFrom;
            Count = count;
        }


        public int StartFrom { get; set; }
        [IntegerValidator(MinValue = 1, MaxValue = 100)]
        public int Count { get; set; }


        private const int defaultPage = 1;
        private const int defaultCount = 20;
    }
}