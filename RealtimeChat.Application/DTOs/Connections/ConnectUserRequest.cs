namespace RealtimeChat.Application.DTOs.Connections;

public class ConnectUserRequest
{
    public string UserId { get; set; } = string.Empty;

    public string ConnectionId { get; set; } = string.Empty;

    public string? DeviceId { get; set; }

    public string Platform { get; set; } = "web";
}
