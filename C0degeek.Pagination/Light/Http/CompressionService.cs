using System.IO.Compression;
using System.Text.Json;
using C0deGeek.Pagination.Light.Configuration;

namespace C0deGeek.Pagination.Light.Http;

public class CompressionService(LightPaginationOptions options)
{
    public async Task<byte[]> CompressAsync<T>(T data)
    {
        using var memory = new MemoryStream();
        await using (var gzip = new GZipStream(memory, options.CompressionLevel))
        {
            await JsonSerializer.SerializeAsync(
                gzip, 
                data,
                new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
        }

        return memory.ToArray();
    }
}