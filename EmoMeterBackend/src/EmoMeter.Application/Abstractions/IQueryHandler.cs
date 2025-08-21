using CSharpFunctionalExtensions;
using EmoMeter.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Abstractions
{
    public interface IQueryHandler<TResponse, TQuery> where TQuery : IQuery
    {
        Task<PagedList<TResponse>> Handle(TQuery query, CancellationToken cancellationToken = default);
    }

    public interface IQueryHandlerWithResult<TResponse, TQuery> where TQuery : IQuery
    {
        public Task<Result<TResponse, ErrorsList>> Handle(TQuery query, CancellationToken cancellationToken = default);
    }
}
