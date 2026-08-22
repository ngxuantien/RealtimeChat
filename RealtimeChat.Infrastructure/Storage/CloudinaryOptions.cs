using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Infrastructure.Storage;

public class CloudinaryOptions
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}
