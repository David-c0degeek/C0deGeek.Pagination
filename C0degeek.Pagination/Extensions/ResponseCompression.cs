using System.IO.Compression;
using System.Text.Json;

namespace C0deGeek.Pagination.Extensions;

public static class ResponseCompression
{
    public static async Task<byte[]> CompressResponse<TData>(
        TData data,
        JsonSerializerOptions? options = null)
    {
        using var memory = new MemoryStream();
        await using (var gzip = new GZipStream(memory, CompressionLevel.Optimal))
        {
            await JsonSerializer.SerializeAsync(
                gzip, 
                data,
                options ?? new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
        }

        return memory.ToArray();
    }
}