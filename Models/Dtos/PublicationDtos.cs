// Models/Dtos/PublicationDtos.cs
using System.ComponentModel.DataAnnotations;

namespace PruebaTecnica2025.Models.Dtos;

// Para respuestas GET
public class PublicationDto
{
    public int Id { get; set; }
    public string TipoPropiedad { get; set; } = string.Empty;
    public string TipoOperacion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Ambientes { get; set; }
    public int M2 { get; set; }
    public int Antiguedad { get; set; }
    public decimal? Lat { get; set; }
    public decimal? Lng { get; set; }
    public List<PublicationImageDto> Imagenes { get; set; } = new();
}

public class PublicationImageDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
}

