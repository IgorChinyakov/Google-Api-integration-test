using CSharpFunctionalExtensions;
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

namespace EmoMeter.Application.Features.Queries.GetUserInfo
{
    public class GetUserInfoHandler : IQueryHandlerWithResult<UserDto, GetUserInfoQuery>
    {
        private readonly IUsersRepository _usersRepository;

        public GetUserInfoHandler(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<Result<UserDto, ErrorsList>> Handle(
            GetUserInfoQuery query, 
            CancellationToken cancellationToken = default)
        {
            var userResult = await _usersRepository.GetByChatId(
                ChatId.Create(query.ChatId), 
                cancellationToken);
            if (userResult.IsFailure)
                return userResult.Error.ToErrorsList();

            var dto = new UserDto(
                query.ChatId, 
                userResult.Value.Email.Value, 
                userResult.Value.NotifyBeforeMinutes.Value);

            return dto;
        }
    }
}
