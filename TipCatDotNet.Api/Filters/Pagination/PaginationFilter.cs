using Newtonsoft.Json;

namespace TipCatDotNet.Api.Filters.Pagination
{
    public class PaginationFilter
    {
        public PaginationFilter()
        {
            this.PageNumber = defaultPage;
            this.PageSize = defaultCount;
        }

        [JsonConstructor]
        public PaginationFilter(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber < defaultPage ? defaultPage : pageNumber;
            PageSize = pageSize > defaultCount ? defaultCount : pageSize;
        }


        public int PageNumber { get; set; }
        public int PageSize { get; set; }


        private const int defaultPage = 1;
        private const int defaultCount = 20;
    }
}