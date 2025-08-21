using CSharpFunctionalExtensions;
using EmoMeter.Application.Abstractions;
using EmoMeter.Application.Calendar;
using EmoMeter.Application.Database;
using EmoMeter.Application.Extensions;
using EmoMeter.Domain.Entities;
using EmoMeter.Domain.Shared;
using EmoMeter.Domain.ValueObjects.Ids;
using EmoMeter.Domain.ValueObjects.Shared;
using EmoMeter.Domain.ValueObjects.User;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Commands.Create
{
    public class CreateUserHandler : ICommandHandler<string, CreateUserCommand>
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ICalendarService _calendarService;
        private readonly IValidator<CreateUserCommand> _validator;

        public CreateUserHandler(
            IUsersRepository usersRepository,
            IValidator<CreateUserCommand> validator,
            ICalendarService calendarService)
        {
            _usersRepository = usersRepository;
            _validator = validator;
            _calendarService = calendarService;
        }

        public async Task<Result<string, ErrorsList>> Handle(
            CreateUserCommand command, 
            CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                return validationResult.ToErrorsList();

            var existingUser = await _usersRepository
                .GetByEmail(Email.Create(command.Email).Value, cancellationToken);

            if (existingUser.IsFailure)
            {
                await _usersRepository.Add(
                    new User(
                    UserId.New(),
                    Email.Create(command.Email).Value,
                    ChatId.Create(command.ChatId),
                    NotifyBeforeMinutes.Create(command.NotifyBeforeMinutes).Value));
                await _usersRepository.Save();

                var link = _calendarService.GenerateAuthLink(command.ChatId.ToString());

                return Result.Success<string, ErrorsList>(link);
            }

            if (existingUser.Value.AuthorizationCredentials == null)
            {
                return Result.Success<string, ErrorsList>("need_auth");
            }

            var user = existingUser.Value;
            var isExpiredToken = user.AuthorizationCredentials.IsExpired();

            if (isExpiredToken)
            {
                var result = await _calendarService.RefreshAccessTokenAsync(
                    user.AuthorizationCredentials.RefreshToken);

                if (result == null || result.AccessToken == null)
                    return Errors.Unauthorized().ToErrorsList();

                if (result.RefreshToken == null)
                    result.RefreshToken = user.AuthorizationCredentials.RefreshToken;

                var credentials = AuthorizationCredentials.Create(
                    result.AccessToken,
                    result.ExpiresIn,
                    result.RefreshToken);

                if (credentials.IsFailure)
                    return credentials.Error.ToErrorsList();

                user.UpdateAuthorizationCredentials(credentials.Value);
            }

            user.UpdateChatId(ChatId.Create(command.ChatId));
            user.UpdateNotificationMinutes(
                NotifyBeforeMinutes.Create(command.NotifyBeforeMinutes).Value);

            await _usersRepository.Save(cancellationToken);

            return Result.Success<string, ErrorsList>("ok");
        }
    }
}
