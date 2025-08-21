using EmoMeter.Domain.Entities;
using EmoMeter.Domain.ValueObjects.Event;
using EmoMeter.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Infrastructure.Configurations
{
    public class EventConfiguration :
        IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.ToTable("events");

            builder.HasKey(e => e.Id).HasName("pk_events");

            builder.Property(u => u.Id).HasConversion(
                eventId => eventId.Value,
                stringId => EventId.Create(stringId))
                .HasColumnName("id");

            builder.Property(e => e.UserId)
                    .HasColumnName("user_id");

            builder.ComplexProperty(e => e.CreatedAt, cb =>
            {
                cb.Property(c => c.Value)
                    .IsRequired()
                    .HasColumnName("created_at");
            });

            builder.ComplexProperty(e => e.Title, db =>
            {
                db.Property(c => c.Value)
                    .IsRequired()
                    .HasMaxLength(Title.TITLE_MAX_LENGTH)
                    .HasColumnName("title");
            });

            builder.ComplexProperty(e => e.Description, db =>
            {
                db.Property(c => c.Value)
                    .IsRequired()
                    .HasMaxLength(Description.DESCRIPTION_MAX_LENGTH)
                    .HasColumnName("description");
            });

            builder.ComplexProperty(e => e.Location, db =>
            {
                db.Property(c => c.Value)
                    .IsRequired()
                    .HasColumnName("location");
            });

            builder.ComplexProperty(e => e.EventDate, eb =>
            {
                eb.Property(e => e.Start)
                    .IsRequired()
                    .HasColumnName("start_date");

                eb.Property(e => e.End)
                    .IsRequired()
                    .HasColumnName("end_date");
            });

            builder.ComplexProperty(e => e.Format, fb =>
            {
                fb.Property(f => f.Value)
                    .HasConversion(ft => ft.ToString(), ft => ConvertToFormatType(ft))
                    .IsRequired()
                    .HasColumnName("format");
            });

            builder.OwnsMany(e => e.Participants, pb =>
            {
                pb.ToJson("participants");

                pb.Property(p => p.Name)
                    .HasMaxLength(Participant.PARTICIPANT_NAME_MAX_LENGTH)
                    .HasColumnName("name");
            });
        }

        private Format.FormatType ConvertToFormatType(string formatType)
        {
            var type = formatType switch
            {
                "Online" => Format.FormatType.Online,
                "Offline" => Format.FormatType.Offline,
                _ => throw new ApplicationException()
            };

            return type;
        }
    }
}
