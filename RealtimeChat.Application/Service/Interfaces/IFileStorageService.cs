using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.Service.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAvatarAsync(Stream content, string contentType, long length, CancellationToken ct = default);
}
