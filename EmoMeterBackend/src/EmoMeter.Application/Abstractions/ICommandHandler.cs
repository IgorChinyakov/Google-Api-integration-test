using CSharpFunctionalExtensions;
using EmoMeter.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Abstractions
{
    public interface ICommandHandler<TResponse, TCommand> where TCommand : ICommand
    {
        Task<Result<TResponse, ErrorsList>> Handle(TCommand command, CancellationToken cancellationToken = default);
    }

    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        Task<UnitResult<ErrorsList>> Handle(TCommand command, CancellationToken cancellationToken = default);
    }
}
