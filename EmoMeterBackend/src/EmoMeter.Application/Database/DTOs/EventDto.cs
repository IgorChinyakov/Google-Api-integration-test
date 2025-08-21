using EmoMeter.Domain.ValueObjects.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Database.DTOs
{
    public record EventDto(
        string Title,
        string Description, 
        DateTime Start, 
        DateTime End, 
        bool IsOnline,
        string Location, 
        List<string> Participants);
}
