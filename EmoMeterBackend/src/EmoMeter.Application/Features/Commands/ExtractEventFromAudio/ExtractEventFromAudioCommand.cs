using EmoMeter.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Commands.ExtractEventFromAudio
{
    public record ExtractEventFromAudioCommand(Stream Stream) : ICommand;
}
