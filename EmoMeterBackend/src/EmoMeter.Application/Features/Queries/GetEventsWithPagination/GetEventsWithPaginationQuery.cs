using EmoMeter.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Queries.GetEventsWithPagination
{
    public record GetEventsWithPaginationQuery(
        long ChatId, 
        int Page, 
        int PageSize,
        DateTime? Start = null,
        DateTime? End = null) : IQuery;
}
