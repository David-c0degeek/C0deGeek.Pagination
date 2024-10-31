namespace C0deGeek.Pagination.Core;

public class ConcurrencyPaginationParameters : PaginationParameters
{
    public byte[]? LastKnownVersion { get; set; }
}