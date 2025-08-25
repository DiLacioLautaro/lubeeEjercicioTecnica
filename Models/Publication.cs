using System.Collections.Generic;

namespace PruebaTecnica2025.Models;

public class Publication
{
    public int Id { get; set; }
    public string TipoPropiedad { get; set; } = string.Empty;
    public string TipoOperacion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    public int Ambientes { get; set; }
    public int M2 { get; set; }
    public int Antiguedad { get; set; }

    public decimal Lat { get; set; }
    public decimal Lng { get; set; }

    public List<PublicationImage> Imagenes { get; set; } = new();
}