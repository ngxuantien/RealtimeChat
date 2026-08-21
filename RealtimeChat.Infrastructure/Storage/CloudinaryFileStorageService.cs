using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using RealtimeChat.Application.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Infrastructure.Storage;

public class CloudinaryFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedContentTypes = new()
    {
        "image/jpeg", "image/png", "image/webp",
    };

    private const long MaxFileSizeBytes = 4 * 1024 * 1024;

    private readonly Cloudinary _cloudinary;

    public CloudinaryFileStorageService(IOptions<CloudinaryOptions> options)
    {
        var opt = options.Value;
        var account = new Account(opt.CloudName, opt.ApiKey, opt.ApiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<string> SaveAvatarAsync(Stream content, string contentType, long length, CancellationToken ct = default)
    {
        if (!AllowedContentTypes.Contains(contentType))
            throw new InvalidOperationException("Chỉ chấp nhận ảnh JPEG, PNG hoặc WEBP.");

        if (length is <= 0 or > MaxFileSizeBytes)
            throw new InvalidOperationException("Ảnh đại diện không được vượt quá 4MB.");

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(Guid.NewGuid().ToString("N"), content),
            Folder = "realtime-chat/avatars",
            Transformation = new Transformation().Width(300).Height(300).Crop("fill").Gravity("face"),
        };

        var result = await _cloudinary.UploadAsync(uploadParams, ct);

        if (result.Error != null)
            throw new InvalidOperationException($"Upload ảnh thất bại: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }
}
