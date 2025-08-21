using CSharpFunctionalExtensions;
using EmoMeter.Application.Abstractions;
using EmoMeter.Application.TextAndAudioProcessing;
using EmoMeter.Domain.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Application.Features.Commands.ExtractEventFromAudio
{
    public class ExtractEventFromAudioHandler :
        ICommandHandler<string, ExtractEventFromAudioCommand>
    {
        private readonly ITextFromAudioExtractor _extractor;

        public ExtractEventFromAudioHandler(
            ITextFromAudioExtractor extractor)
        {
            _extractor = extractor;
        }

        public async Task<Result<string, ErrorsList>> Handle(
            ExtractEventFromAudioCommand command, 
            CancellationToken cancellationToken = default)
        {
            using var memoryStream = new MemoryStream();
            await command.Stream.CopyToAsync(memoryStream, cancellationToken);

            byte[] fileBytes = memoryStream.ToArray();

            var extractedTextDto = await _extractor.TranscribeAudioAsync(fileBytes);

            if (extractedTextDto == null || extractedTextDto.Result == null)
                return Error.Failure(
                    "text.extraction.failure", 
                    "Failed to extract text from audio").ToErrorsList();

            return extractedTextDto.Result;
        }
    }
}
