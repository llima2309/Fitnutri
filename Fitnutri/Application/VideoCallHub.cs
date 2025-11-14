using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Fitnutri.Application;

public record CallParticipant(string UserId, string UserType, string ConnectionId);

[Authorize]
public class VideoCallHub : Hub
{
    // Armazena conexões ativas por sala (appointmentId -> lista de participantes)
    private static readonly ConcurrentDictionary<string, List<CallParticipant>> _activeRooms = new();

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public async Task JoinCall(string appointmentId, string userId, string userType)
    {
        var groupName = $"call_{appointmentId}";
        
        // Adiciona ao grupo do SignalR
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        // Registra participante na sala
        var participant = new CallParticipant(userId, userType, Context.ConnectionId);
        _activeRooms.AddOrUpdate(
            appointmentId,
            new List<CallParticipant> { participant },
            (key, list) =>
            {
                list.Add(participant);
                return list;
            }
        );

        // Notifica outros participantes
        await Clients.OthersInGroup(groupName).SendAsync("UserJoined", userId, userType, Context.ConnectionId);

        // Envia lista de participantes já presentes para o novo usuário
        var existingParticipants = _activeRooms[appointmentId]
            .Where(p => p.ConnectionId != Context.ConnectionId)
            .Select(p => new { p.UserId, p.UserType, p.ConnectionId });

        await Clients.Caller.SendAsync("ExistingParticipants", existingParticipants);
    }

    public async Task SendOffer(string appointmentId, string offer, string targetConnectionId)
    {
        await Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", offer, Context.ConnectionId);
    }

    public async Task SendAnswer(string appointmentId, string answer, string targetConnectionId)
    {
        await Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", answer, Context.ConnectionId);
    }

    public async Task SendIceCandidate(string appointmentId, string candidate, string targetConnectionId)
    {
        await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", candidate);
    }

    public async Task ToggleAudio(string appointmentId, bool enabled)
    {
        var groupName = $"call_{appointmentId}";
        await Clients.OthersInGroup(groupName).SendAsync("UserToggledAudio", Context.ConnectionId, enabled);
    }

    public async Task ToggleVideo(string appointmentId, bool enabled)
    {
        var groupName = $"call_{appointmentId}";
        await Clients.OthersInGroup(groupName).SendAsync("UserToggledVideo", Context.ConnectionId, enabled);
    }

    public async Task LeaveCall(string appointmentId)
    {
        var groupName = $"call_{appointmentId}";
        
        // Notifica outros participantes
        await Clients.OthersInGroup(groupName).SendAsync("UserLeft", Context.ConnectionId);
        
        // Remove do grupo
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        // Remove participante da sala
        if (_activeRooms.TryGetValue(appointmentId, out var participants))
        {
            participants.RemoveAll(p => p.ConnectionId == Context.ConnectionId);
            
            // Se não há mais participantes, remove a sala
            if (participants.Count == 0)
            {
                _activeRooms.TryRemove(appointmentId, out _);
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove de todas as salas em caso de desconexão
        foreach (var room in _activeRooms)
        {
            var participant = room.Value.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (participant != null)
            {
                room.Value.Remove(participant);
                await Clients.Group($"call_{room.Key}").SendAsync("UserLeft", Context.ConnectionId);
                
                if (room.Value.Count == 0)
                {
                    _activeRooms.TryRemove(room.Key, out _);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task<bool> IsRoomActive(string appointmentId)
    {
        var isActive = _activeRooms.ContainsKey(appointmentId) && _activeRooms[appointmentId].Count > 0;
        await Clients.Caller.SendAsync("RoomStatus", isActive);
        return isActive;
    }
}

