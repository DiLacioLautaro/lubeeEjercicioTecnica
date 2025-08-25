namespace PruebaTecnica2025.Models;

public class PublicationImage
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;

    public int PublicationId { get; set; }
    public Publication? Publication { get; set; }
}

