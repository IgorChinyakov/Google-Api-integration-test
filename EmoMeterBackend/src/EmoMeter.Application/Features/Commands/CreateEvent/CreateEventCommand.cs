using EmoMeter.Application.Abstractions;
using EmoMeter.Application.Database.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Commands.CreateEvent
{
    public record CreateEventCommand(long ChatId, EventDto CreateEventDto) : ICommand;
}
