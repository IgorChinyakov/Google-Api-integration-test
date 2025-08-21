using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Database.DTOs
{
    public record UserDto(long ChatId, string Email, int NotifyBeforeMinutes);
}
