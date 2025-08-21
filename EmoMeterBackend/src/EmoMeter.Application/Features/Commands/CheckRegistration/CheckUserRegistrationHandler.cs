using CSharpFunctionalExtensions;
using EmoMeter.Application.Abstractions;
using EmoMeter.Application.Database;
using EmoMeter.Domain.Shared;
using EmoMeter.Domain.ValueObjects.Ids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Commands.CheckRegistration
{
    public class CheckUserRegistrationHandler : 
        ICommandHandler<CheckUserRegistrationCommand>
    {
        private readonly IUsersRepository _usersRepository;

        public CheckUserRegistrationHandler(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<UnitResult<ErrorsList>> Handle(
            CheckUserRegistrationCommand command,
            CancellationToken cancellationToken = default)
        {
            var userResult = await _usersRepository.GetByChatId(ChatId.Create(command.ChatId), cancellationToken);
            if (userResult.IsFailure)
                return Errors.NotFound("User").ToErrorsList();

            if (userResult.Value.AuthorizationCredentials == null)
                return Errors.Unauthorized().ToErrorsList();

            var expirationTime = userResult.Value.AuthorizationCredentials.TokenExpiresIn;
            if(expirationTime < DateTime.UtcNow)
                return Errors.Unauthorized().ToErrorsList();

            return Result.Success<ErrorsList>();
        }
    }
}
