namespace C0deGeek.Pagination.Abstractions;

public interface IEntity
{
    public int Id { get; set; }
    public byte[] RowVersion { get; set; }
    public DateTime LastModified { get; set; }
}