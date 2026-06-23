using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Template.Web.Features.StanzeStudio
{
    public class RoomTask
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool Done { get; set; }
    }

    public class RoomChatMessage
    {
        public string User { get; set; }
        public string Text { get; set; }
        public string Time { get; set; }
    }

    public class RoomState
    {
        public Guid RoomId { get; set; }
        public List<RoomTask> Tasks { get; set; } = new List<RoomTask>();
        public List<RoomChatMessage> Messages { get; set; } = new List<RoomChatMessage>();
        public List<string> Participants { get; set; } = new List<string>();
        public List<(string UserName, Guid UserId)> ParticipantsWithIds { get; set; } = new List<(string UserName, Guid UserId)>();
        public bool IsTimerRunning { get; set; }
        public DateTime? TimerStartedAt { get; set; }
        public int SecondsAtStart { get; set; } = 25 * 60;
        public bool IsBreak { get; set; }

        public int RemainingSeconds
        {
            get
            {
                if (!IsTimerRunning || !TimerStartedAt.HasValue)
                {
                    return SecondsAtStart;
                }
                var elapsed = (int)(DateTime.UtcNow - TimerStartedAt.Value).TotalSeconds;
                var remaining = SecondsAtStart - elapsed;
                return remaining > 0 ? remaining : 0;
            }
            set
            {
                SecondsAtStart = value;
            }
        }
    }

    public interface IRoomStateManager
    {
        RoomState GetOrCreateState(Guid roomId);
        void AddTask(Guid roomId, RoomTask task);
        void ToggleTask(Guid roomId, string taskId);
        void DeleteTask(Guid roomId, string taskId);
        void AddMessage(Guid roomId, RoomChatMessage msg);
        void AddParticipant(Guid roomId, string connectionId, string userName, Guid userId);
        (Guid RoomId, string UserName, Guid UserId)? RemoveParticipant(string connectionId);
        void UpdateTimer(Guid roomId, bool isRunning, int remainingSeconds, bool isBreak);
        List<string> GetParticipants(Guid roomId);
        List<(string UserName, Guid UserId)> GetParticipantsWithUserIds(Guid roomId);
        Guid? GetActiveRoomIdForUser(Guid userId);
    }

    public class RoomStateManager : IRoomStateManager
    {
        private readonly ConcurrentDictionary<Guid, RoomState> _states = new ConcurrentDictionary<Guid, RoomState>();
        private readonly ConcurrentDictionary<string, (Guid RoomId, string UserName, Guid UserId)> _connections = new ConcurrentDictionary<string, (Guid RoomId, string UserName, Guid UserId)>();
        private readonly Microsoft.Extensions.DependencyInjection.IServiceScopeFactory _scopeFactory;

        public RoomStateManager(Microsoft.Extensions.DependencyInjection.IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public RoomState GetOrCreateState(Guid roomId)
        {
            return _states.GetOrAdd(roomId, id =>
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<Services.TemplateDbContext>();
                    var room = dbContext.StanzeStudio.AsNoTracking().FirstOrDefault(r => r.Id == id);
                    if (room != null)
                    {
                        return new RoomState
                        {
                            RoomId = id,
                            IsTimerRunning = room.IsInEsecuzione,
                            SecondsAtStart = (int)room.TempoRimanente.TotalSeconds,
                            TimerStartedAt = room.IsInEsecuzione ? DateTime.UtcNow : null
                        };
                    }
                }
                return new RoomState { RoomId = id };
            });
        }

        public void AddTask(Guid roomId, RoomTask task)
        {
            var state = GetOrCreateState(roomId);
            lock (state)
            {
                state.Tasks.Add(task);
            }
        }

        public void ToggleTask(Guid roomId, string taskId)
        {
            var state = GetOrCreateState(roomId);
            lock (state)
            {
                var task = state.Tasks.FirstOrDefault(t => t.Id == taskId);
                if (task != null)
                {
                    task.Done = !task.Done;
                }
            }
        }

        public void DeleteTask(Guid roomId, string taskId)
        {
            var state = GetOrCreateState(roomId);
            lock (state)
            {
                state.Tasks.RemoveAll(t => t.Id == taskId);
            }
        }

        public void AddMessage(Guid roomId, RoomChatMessage msg)
        {
            var state = GetOrCreateState(roomId);
            lock (state)
            {
                state.Messages.Add(msg);
                if (state.Messages.Count > 100)
                {
                    state.Messages.RemoveAt(0);
                }
            }
        }

        public void AddParticipant(Guid roomId, string connectionId, string userName, Guid userId)
        {
            var state = GetOrCreateState(roomId);
            lock (state)
            {
                if (!state.Participants.Contains(userName))
                {
                    state.Participants.Add(userName);
                }
                if (!state.ParticipantsWithIds.Any(p => p.UserId == userId))
                {
                    state.ParticipantsWithIds.Add((userName, userId));
                }
            }
            _connections[connectionId] = (roomId, userName, userId);
        }

        public (Guid RoomId, string UserName, Guid UserId)? RemoveParticipant(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out var info))
            {
                var state = GetOrCreateState(info.RoomId);
                lock (state)
                {
                    bool hasOtherConnections = _connections.Values.Any(v => v.RoomId == info.RoomId && v.UserName == info.UserName);
                    if (!hasOtherConnections)
                    {
                        state.Participants.Remove(info.UserName);
                        state.ParticipantsWithIds.RemoveAll(p => p.UserId == info.UserId);
                    }
                }
                return info;
            }
            return null;
        }

        public void UpdateTimer(Guid roomId, bool isRunning, int remainingSeconds, bool isBreak)
        {
            var state = GetOrCreateState(roomId);
            lock (state)
            {
                state.IsTimerRunning = isRunning;
                state.IsBreak = isBreak;
                if (isRunning)
                {
                    state.TimerStartedAt = DateTime.UtcNow;
                    state.SecondsAtStart = remainingSeconds;
                }
                else
                {
                    state.TimerStartedAt = null;
                    state.SecondsAtStart = remainingSeconds;
                }
            }
        }

        public List<string> GetParticipants(Guid roomId)
        {
            var state = GetOrCreateState(roomId);
            lock (state)
            {
                return state.Participants.ToList();
            }
        }

        public List<(string UserName, Guid UserId)> GetParticipantsWithUserIds(Guid roomId)
        {
            var state = GetOrCreateState(roomId);
            lock (state)
            {
                return state.ParticipantsWithIds.ToList();
            }
        }

        public Guid? GetActiveRoomIdForUser(Guid userId)
        {
            if (userId == Guid.Empty) return null;
            var conn = _connections.Values.FirstOrDefault(c => c.UserId == userId);
            return conn.RoomId == Guid.Empty ? (Guid?)null : conn.RoomId;
        }
    }
}

