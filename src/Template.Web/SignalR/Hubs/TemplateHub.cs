using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Template.Services;
using Template.Web.Features.StanzeStudio;

namespace Template.Web.SignalR.Hubs
{
    public interface ITemplateClientEvent
    {
        Task NewMessage(Guid idUser, Guid idMessage);

        // Stanze Studio Events
        Task UserJoined(string userName, System.Collections.Generic.List<string> participants);
        Task UserLeft(string userName, System.Collections.Generic.List<string> participants);
        Task ReceiveChatMessage(string userName, string text, string time);
        Task TimerUpdated(bool isRunning, int remainingSeconds, bool isBreak);
        Task TasksUpdated(System.Collections.Generic.List<RoomTask> tasks);
        Task LobbyRoomUpdated(Guid roomId, string nome, string corsoNome, Guid corsoId, int onlineCount, int remainingSeconds, bool isRunning, bool isBreak);
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    public class TemplateHub : Hub<ITemplateClientEvent>
    {
        private readonly IPublishDomainEvents _publisher;
        private readonly IRoomStateManager _roomStateManager;
        private readonly TemplateDbContext _dbContext;

        public TemplateHub(IPublishDomainEvents publisher, IRoomStateManager roomStateManager, TemplateDbContext dbContext)
        {
            _publisher = publisher;
            _roomStateManager = roomStateManager;
            _dbContext = dbContext;
        }

        public async Task JoinGroup(Guid idGroup)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, idGroup.ToString());
        }

        public async Task LeaveGroup(Guid idGroup)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, idGroup.ToString());
        }

        // ================= Stanze Studio Hub Actions =================

        public async Task JoinLobby()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Lobby");
        }

        public async Task LeaveLobby()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Lobby");
        }

        private async Task NotifyLobbyOfRoomUpdate(Guid roomId)
        {
            var state = _roomStateManager.GetOrCreateState(roomId);
            var roomDb = await _dbContext.StanzeStudio.Include(x => x.Corso).FirstOrDefaultAsync(x => x.Id == roomId);
            if (roomDb != null)
            {
                await Clients.Group("Lobby").LobbyRoomUpdated(
                    roomId,
                    roomDb.Nome,
                    roomDb.Corso != null ? roomDb.Corso.Nome : "Materia",
                    roomDb.CorsoId,
                    state.Participants.Count,
                    state.RemainingSeconds,
                    state.IsTimerRunning,
                    state.IsBreak
                );
            }
        }

        public async Task JoinRoom(Guid roomId, string userName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            _roomStateManager.AddParticipant(roomId, Context.ConnectionId, userName);

            var state = _roomStateManager.GetOrCreateState(roomId);

            // 1. Send current room state only to the caller who just joined
            await Clients.Caller.TasksUpdated(state.Tasks);
            await Clients.Caller.TimerUpdated(state.IsTimerRunning, state.RemainingSeconds, state.IsBreak);
            
            // Send existing messages history to the caller
            foreach (var msg in state.Messages)
            {
                await Clients.Caller.ReceiveChatMessage(msg.User, msg.Text, msg.Time);
            }

            // 2. Broadcast user join to everyone in the room (including their updated list of participants)
            await Clients.Group(roomId.ToString()).UserJoined(userName, state.Participants);

            // 3. Update the Lobby group with new online count
            await NotifyLobbyOfRoomUpdate(roomId);
        }

        public async Task LeaveRoom(Guid roomId, string userName)
        {
            _roomStateManager.RemoveParticipant(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());

            var participants = _roomStateManager.GetParticipants(roomId);
            await Clients.Group(roomId.ToString()).UserLeft(userName, participants);

            // Update the Lobby group with new online count
            await NotifyLobbyOfRoomUpdate(roomId);
        }

        public async Task SendChatMessage(Guid roomId, string userName, string text)
        {
            var timeString = DateTime.Now.ToString("HH:mm");
            var msg = new RoomChatMessage
            {
                User = userName,
                Text = text,
                Time = timeString
            };

            _roomStateManager.AddMessage(roomId, msg);
            await Clients.Group(roomId.ToString()).ReceiveChatMessage(userName, text, timeString);
        }

        public async Task UpdateTimer(Guid roomId, bool isRunning, int remainingSeconds, bool isBreak)
        {
            _roomStateManager.UpdateTimer(roomId, isRunning, remainingSeconds, isBreak);
            await Clients.Group(roomId.ToString()).TimerUpdated(isRunning, remainingSeconds, isBreak);

            // Update the Lobby group
            await NotifyLobbyOfRoomUpdate(roomId);
        }

        public async Task AddTask(Guid roomId, string text)
        {
            var task = new RoomTask
            {
                Id = Guid.NewGuid().ToString(),
                Text = text,
                Done = false
            };

            _roomStateManager.AddTask(roomId, task);
            
            var state = _roomStateManager.GetOrCreateState(roomId);
            await Clients.Group(roomId.ToString()).TasksUpdated(state.Tasks);
        }

        public async Task ToggleTask(Guid roomId, string taskId)
        {
            _roomStateManager.ToggleTask(roomId, taskId);
            
            var state = _roomStateManager.GetOrCreateState(roomId);
            await Clients.Group(roomId.ToString()).TasksUpdated(state.Tasks);
        }

        public async Task DeleteTask(Guid roomId, string taskId)
        {
            _roomStateManager.DeleteTask(roomId, taskId);
            
            var state = _roomStateManager.GetOrCreateState(roomId);
            await Clients.Group(roomId.ToString()).TasksUpdated(state.Tasks);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var info = _roomStateManager.RemoveParticipant(Context.ConnectionId);
            if (info != null)
            {
                var participants = _roomStateManager.GetParticipants(info.Value.RoomId);
                await Clients.Group(info.Value.RoomId.ToString()).UserLeft(info.Value.UserName, participants);

                // Update the Lobby group with new online count
                await NotifyLobbyOfRoomUpdate(info.Value.RoomId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
