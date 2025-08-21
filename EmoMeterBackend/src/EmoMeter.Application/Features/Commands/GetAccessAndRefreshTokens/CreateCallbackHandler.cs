using CSharpFunctionalExtensions;
using EmoMeter.Application.Abstractions;
using EmoMeter.Application.Calendar;
using EmoMeter.Application.Database;
using EmoMeter.Domain.Entities;
using EmoMeter.Domain.Shared;
using EmoMeter.Domain.ValueObjects.Ids;
using EmoMeter.Domain.ValueObjects.Shared;
using EmoMeter.Domain.ValueObjects.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Commands.GetAccessAndRefreshTokens
{
    public class CreateCallbackHandler :
        ICommandHandler<CreateCallbackCommand>
    {
        private readonly ICalendarService _calendarService;
        private readonly IUsersRepository _usersRepository;

        public CreateCallbackHandler(
            ICalendarService calendarService, 
            IUsersRepository usersRepository)
        {
            _calendarService = calendarService;
            _usersRepository = usersRepository;
        }

        public async Task<UnitResult<ErrorsList>> Handle(
            CreateCallbackCommand command, CancellationToken cancellationToken = default)
        {
            var tokens = await _calendarService.ExchangeCodeForTokensAsync(command.Code);
            if (tokens == null)
                return Errors.Unauthorized().ToErrorsList();

            long.TryParse(command.State, out var longState);
            var user = await _usersRepository.GetByChatId(ChatId.Create(longState), cancellationToken);
            if(user.IsFailure)
                return Errors.Unauthorized().ToErrorsList();

            var credentials = AuthorizationCredentials.Create(
                tokens.AccessToken, 
                tokens.ExpiresIn, 
                tokens.RefreshToken);
            if(credentials.IsFailure)
                return Errors.Unauthorized().ToErrorsList();

            user.Value.UpdateAuthorizationCredentials(credentials.Value);

            await _usersRepository.Save();

            return Result.Success<ErrorsList>();
        }
    }
}
