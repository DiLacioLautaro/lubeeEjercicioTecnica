using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PruebaTecnica2025.Models.Dtos
{
    // Para crear/actualizar (POST/PUT)
    public class PublicationCreateUpdateDto
    {
        [Required] public string TipoPropiedad { get; set; } = string.Empty;
        [Required] public string TipoOperacion { get; set; } = string.Empty;

        [Required] public string Descripcion { get; set; } = string.Empty;

        [Range(0, int.MaxValue)] public int Ambientes { get; set; }
        [Range(0, int.MaxValue)] public int M2 { get; set; }
        [Range(0, int.MaxValue)] public int Antiguedad { get; set; }

        public decimal Lat { get; set; }
        public decimal Lng { get; set; }

        public List<PublicationImageCreateDto> Imagenes { get; set; } = new();
    }

    public class PublicationImageCreateDto
    {
        [Required] public string Url { get; set; } = string.Empty;
    }

}
