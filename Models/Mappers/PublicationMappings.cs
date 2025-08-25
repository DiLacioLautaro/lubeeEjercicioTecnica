// Models/Mappers/PublicationMappings.cs
using PruebaTecnica2025.Models.Dtos;

namespace PruebaTecnica2025.Models.Mappers;

public static class PublicationMappings
{
    // Entidad -> DTO (para respuestas GET)
    public static PublicationDto ToDto(this Publication p) => new()
    {
        Id = p.Id,
        TipoPropiedad = p.TipoPropiedad,
        TipoOperacion = p.TipoOperacion,
        Descripcion = p.Descripcion,
        Ambientes = p.Ambientes,
        M2 = p.M2,
        Antiguedad = p.Antiguedad,
        Lat = p.Lat,
        Lng = p.Lng,
        Imagenes = p.Imagenes.Select(i => new PublicationImageDto
        {
            Id = i.Id,
            Url = i.Url
        }).ToList()
    };

    // DTO (create/update) -> Entidad (para POST)
    public static Publication FromDto(this PublicationCreateUpdateDto dto) => new()
    {
        TipoPropiedad = dto.TipoPropiedad,
        TipoOperacion = dto.TipoOperacion,
        Descripcion = dto.Descripcion,
        Ambientes = dto.Ambientes,
        M2 = dto.M2,
        Antiguedad = dto.Antiguedad,
        Lat = dto.Lat,
        Lng = dto.Lng,
        Imagenes = (dto.Imagenes ?? Enumerable.Empty<PublicationImageCreateDto>())
        .Select(i => new PublicationImage { Url = i.Url })
        .ToList()

    };

    // Actualizar entidad desde DTO (para PUT)
    public static void UpdateFromDto(this Publication entity, PublicationCreateUpdateDto dto)
    {
        entity.TipoPropiedad = dto.TipoPropiedad;
        entity.TipoOperacion = dto.TipoOperacion;
        entity.Descripcion = dto.Descripcion;
        entity.Ambientes = dto.Ambientes;
        entity.M2 = dto.M2;
        entity.Antiguedad = dto.Antiguedad;
        entity.Lat = dto.Lat;
        entity.Lng = dto.Lng;
        entity.Imagenes ??= new List<PublicationImage>();
        entity.Imagenes.Clear();
        foreach (var img in (dto.Imagenes ?? Enumerable.Empty<PublicationImageCreateDto>()))
            entity.Imagenes.Add(new PublicationImage { Url = img.Url });

    }
}
