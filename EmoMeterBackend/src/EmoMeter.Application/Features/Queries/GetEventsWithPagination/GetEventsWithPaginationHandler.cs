using EmoMeter.Application.Abstractions;
using EmoMeter.Application.Database;
using EmoMeter.Application.Database.DTOs;
using EmoMeter.Domain.Shared;
using EmoMeter.Domain.ValueObjects.Ids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Queries.GetEventsWithPagination
{
    public class GetEventsWithPaginationHandler : 
        IQueryHandler<EventDto, GetEventsWithPaginationQuery>
    {
        private readonly IUsersRepository _repository;

        public GetEventsWithPaginationHandler(IUsersRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedList<EventDto>> Handle(
            GetEventsWithPaginationQuery query,
            CancellationToken cancellationToken = default)
        {
            var dtos = await _repository.GetEventsWithPagination(
                query.Page, 
                query.PageSize, 
                ChatId.Create(query.ChatId), 
                cancellationToken,
                query.Start,
                query.End);

            return dtos;
        }
    }
}
