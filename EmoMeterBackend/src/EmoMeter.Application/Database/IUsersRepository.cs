using CSharpFunctionalExtensions;
using EmoMeter.Application.Database.DTOs;
using EmoMeter.Domain.Entities;
using EmoMeter.Domain.Shared;
using EmoMeter.Domain.ValueObjects.Ids;
using EmoMeter.Domain.ValueObjects.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Database
{
    public interface IUsersRepository
    {
        Task<Guid> Add(User user, CancellationToken cancellationToken = default);

        Task<Result<User, Error>> GetByChatId(ChatId chatId, CancellationToken cancellationToken = default);

        Task<Result<User, Error>> GetByEmail(Email email, CancellationToken cancellationToken = default);

        Task<PagedList<EventDto>> GetEventsWithPagination(int page, int pageSize, ChatId chatId, CancellationToken cancellationToken = default, DateTime? start = null, DateTime? end = null);
        
        Task Save(CancellationToken cancellationToken = default);
    }
}
