using CSharpFunctionalExtensions;
using EmoMeter.Application.Abstractions;
using EmoMeter.Application.Database;
using EmoMeter.Application.Extensions;
using EmoMeter.Domain.Shared;
using EmoMeter.Domain.ValueObjects.Ids;
using EmoMeter.Domain.ValueObjects.Shared;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Commands.UpdateUserNotifyBeforeMinutes
{
    public class UpdateUserNotifyBeforeMinutesHandler :
        ICommandHandler<UpdateUserNotifyBeforeMinutesCommand>
    {
        private readonly IValidator<UpdateUserNotifyBeforeMinutesCommand> _validator;
        private readonly IUsersRepository _usersRepository;

        public UpdateUserNotifyBeforeMinutesHandler(
            IValidator<UpdateUserNotifyBeforeMinutesCommand> validator, IUsersRepository usersRepository)
        {
            _validator = validator;
            _usersRepository = usersRepository;
        }

        public async Task<UnitResult<ErrorsList>> Handle(
            UpdateUserNotifyBeforeMinutesCommand command, 
            CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                return validationResult.ToErrorsList();

            var userResult = await _usersRepository.GetByChatId(
                ChatId.Create(command.ChatId), 
                cancellationToken);
            if (userResult.IsFailure)
                return userResult.Error.ToErrorsList();

            userResult.Value.UpdateNotificationMinutes(
                NotifyBeforeMinutes.Create(command.Minutes).Value);

            await _usersRepository.Save();

            return Result.Success<ErrorsList>();
        }
    }
}
