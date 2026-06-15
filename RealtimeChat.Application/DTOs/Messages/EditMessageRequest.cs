using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.DTOs.Messages;

public class EditMessageRequest
{
    public string Content { get; set; } = string.Empty;
}
