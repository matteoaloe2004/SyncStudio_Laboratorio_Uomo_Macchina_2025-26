using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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
        public bool IsTimerRunning { get; set; }
        public int RemainingSeconds { get; set; } = 25 * 60;
        public bool IsBreak { get; set; }
    }

    public interface IRoomStateManager
    {
        RoomState GetOrCreateState(Guid roomId);
        void AddTask(Guid roomId, RoomTask task);
        void ToggleTask(Guid roomId, string taskId);
        void DeleteTask(Guid roomId, string taskId);
        void AddMessage(Guid roomId, RoomChatMessage msg);
        void AddParticipant(Guid roomId, string connectionId, string userName);
        (Guid RoomId, string UserName)? RemoveParticipant(string connectionId);
        void UpdateTimer(Guid roomId, bool isRunning, int remainingSeconds, bool isBreak);
        List<string> GetParticipants(Guid roomId);
    }

    public class RoomStateManager : IRoomStateManager
    {
        private readonly ConcurrentDictionary<Guid, RoomState> _states = new ConcurrentDictionary<Guid, RoomState>();
        private readonly ConcurrentDictionary<string, (Guid RoomId, string UserName)> _connections = new ConcurrentDictionary<string, (Guid RoomId, string UserName)>();

        public RoomState GetOrCreateState(Guid roomId)
        {
            return _states.GetOrAdd(roomId, id => new RoomState { RoomId = id });
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

        public void AddParticipant(Guid roomId, string connectionId, string userName)
        {
            var state = GetOrCreateState(roomId);
            lock (state)
            {
                if (!state.Participants.Contains(userName))
                {
                    state.Participants.Add(userName);
                }
            }
            _connections[connectionId] = (roomId, userName);
        }

        public (Guid RoomId, string UserName)? RemoveParticipant(string connectionId)
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
                state.RemainingSeconds = remainingSeconds;
                state.IsBreak = isBreak;
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
    }
}
