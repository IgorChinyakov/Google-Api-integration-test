using CSharpFunctionalExtensions;
using EmoMeter.Application.Database;
using EmoMeter.Application.Database.DTOs;
using EmoMeter.Domain.Entities;
using EmoMeter.Domain.Shared;
using EmoMeter.Domain.ValueObjects.Event;
using EmoMeter.Domain.ValueObjects.Ids;
using EmoMeter.Domain.ValueObjects.User;
using EmoMeter.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Infrastructure.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UsersRepository(
            ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Add(
            User user,
            CancellationToken cancellationToken = default)
        {
            await _dbContext.Users.AddAsync(user, cancellationToken);
            return user.Id.Value;
        }

        public async Task<Result<User, Error>> GetByEmail(
            Email email,
            CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .Include(u => u.Events)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if (user == null)
                return Errors.NotFound("User");

            return user;
        }

        public async Task<Result<User, Error>> GetByChatId(
            ChatId chatId,
            CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .Include(u => u.Events)
                .FirstOrDefaultAsync(u => u.ChatId == chatId, cancellationToken);
            if (user == null)
                return Errors.NotFound("User");

            return user;
        }

        public async Task Save(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<PagedList<EventDto>> GetEventsWithPagination(
            int page,
            int pageSize,
            ChatId chatId,
            CancellationToken cancellationToken = default,
            DateTime? start = null,
            DateTime? end = null)
        {
            var query = _dbContext.Events
                .Where(e => e.User.ChatId == chatId)
                .OrderBy(e => e.EventDate.Start)
                .AsNoTracking();

            var count = await query.CountAsync(cancellationToken);

            if (start != null && end != null)
                query = query.Where(e => DateTime.SpecifyKind(end.Value, DateTimeKind.Utc) > e.EventDate.Start && 
                DateTime.SpecifyKind(start.Value, DateTimeKind.Utc) < e.EventDate.End);

            query = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var eventsList = await query.ToListAsync(cancellationToken);

            var pagedList = new PagedList<EventDto>
            {
                Items = eventsList.Select(e =>
                new EventDto(
                        e.Title.Value,
                        e.Description.Value,
                        e.EventDate.Start,
                        e.EventDate.End,
                        e.Format.Value == Format.FormatType.Online ? true : false,
                        e.Location.Value,
                        e.Participants.Select(p => p.Name).ToList())).ToList(),
                TotalCount = count,
                Page = page,
                PageSize = pageSize
            };

            return pagedList;
        }
    }
}
