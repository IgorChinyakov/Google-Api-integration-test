using System;
using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmoMeter.Domain.ValueObjects.Ids;
using EmoMeter.Domain.ValueObjects.User;
using EmoMeter.Domain.ValueObjects.Shared;

namespace EmoMeter.Domain.Entities
{
    public class User : Entity<UserId>
    {
        private readonly List<Event> _events = [];

        //ef core
        private User()
        {
        }

        public Email Email { get; private set; }

        public NotifyBeforeMinutes NotifyBeforeMinutes { get; private set; }

        public ChatId ChatId { get; private set; }

        public AuthorizationCredentials? AuthorizationCredentials { get; private set; }

        public IReadOnlyList<Event> Events => _events;

        public User(
            UserId id, 
            Email email, 
            ChatId chatId,
            NotifyBeforeMinutes notifyBeforeMinutes) : base(id)
        {
            Email = email;
            NotifyBeforeMinutes = notifyBeforeMinutes;
            ChatId = chatId;
        }

        public void UpdateChatId(ChatId id)
            => ChatId = id;

        public void UpdateNotificationMinutes(NotifyBeforeMinutes notifyBeforeMinutes)
            => NotifyBeforeMinutes = notifyBeforeMinutes;

        public void UpdateAuthorizationCredentials(
            AuthorizationCredentials authorizationCredentials)
            => AuthorizationCredentials = authorizationCredentials;

        public void AddEvent(Event @event)
            => _events.Add(@event);
    }
}
