using EmoMeter.Domain.Entities;
using EmoMeter.Domain.ValueObjects.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Infrastructure.Configurations
{
    public class UserConfiguration :
        IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.Id).HasName("pk_users");

            builder.Property(u => u.Id).HasConversion(
                userId => userId.Value,
                stringId => UserId.Create(stringId))
                .HasColumnName("id");

            builder.OwnsOne(u => u.AuthorizationCredentials, ab =>
            {
                ab.Property(n => n.AccessToken)
                    .IsRequired()
                    .HasColumnName("access_token");

                ab.Property(n => n.RefreshToken)
                    .IsRequired()
                    .HasColumnName("refresh_token");

                ab.Property(n => n.TokenExpiresIn)
                    .IsRequired()
                    .HasColumnName("token_expires_in");
            });

            builder.ComplexProperty(u => u.NotifyBeforeMinutes, nb =>
            {
                nb.Property(n => n.Value)
                    .IsRequired()
                    .HasDefaultValue(10)
                    .HasColumnName("notify_before_minutes");
            });

            builder.ComplexProperty(u => u.ChatId, cb =>
            {
                var converter = new ValueConverter<BigInteger, long>(
                    model => (long)model,
                    provider => new BigInteger(provider));

                cb.Property(n => n.Value)
                    .HasConversion(converter)
                    .IsRequired()
                    .HasColumnName("chat_id");
            });

            builder.ComplexProperty(u => u.Email, eb =>
            {
                eb.Property(n => n.Value)
                    .IsRequired()
                    .HasColumnName("email");
            });

            builder.HasMany(u => u.Events)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
