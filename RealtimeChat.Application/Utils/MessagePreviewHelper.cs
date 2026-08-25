using RealtimeChat.Domain.Entities;
using RealtimeChat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.Utils;

public static class MessagePreviewHelper
{
    public static string Build(Message message)
    {
        return message.Type switch
        {
            MessageType.Image => "[Hình ảnh]",
            MessageType.Video => "[Video]",
            MessageType.Voice => "[Tin nhắn thoại]",
            MessageType.File => "[Tệp đính kèm]",
            _ => message.Content.Length > 120 ? message.Content[..120] : message.Content,
        };
    }
}
