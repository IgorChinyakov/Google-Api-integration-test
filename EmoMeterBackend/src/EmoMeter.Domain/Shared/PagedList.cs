using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.Shared
{
    public class PagedList<T>
    {
        public IReadOnlyList<T> Items { get; set; } = [];

        public long TotalCount { get; init; }

        public int PageSize { get; init; }

        public int Page { get; init; }

        public bool HasNextPage => Page * PageSize < TotalCount;

        public bool HasPreviousPage => Page * PageSize > 1;
    }
}
