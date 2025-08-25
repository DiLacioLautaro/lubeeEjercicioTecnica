using Microsoft.EntityFrameworkCore;
using PruebaTecnica2025.Common;
using PruebaTecnica2025.Data;
using PruebaTecnica2025.Models.Dtos;
using PruebaTecnica2025.Models.Mappers; 
using PruebaTecnica2025.Services.Abstractions;

namespace PruebaTecnica2025.Services.Implementations;

public sealed class PublicationService : IPublicationService
{
    private readonly AppDbContext _db;

    public PublicationService(AppDbContext db) => _db = db;

    // ================== QUERIES (lectura) ==================
    public async Task<List<PublicationDto>> GetAllAsync(CancellationToken ct)
        => await _db.Publications
            .AsNoTracking()
            .Include(p => p.Imagenes)
            .OrderByDescending(p => p.Id)
            .Select(p => p.ToDto())
            .ToListAsync(ct);

    public async Task<PublicationDto> GetOneAsync(int id, CancellationToken ct)
    {
        var entity = await _db.Publications
            .AsNoTracking()
            .Include(p => p.Imagenes)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (entity is null)
            throw new NotFoundException($"Publicación {id} no encontrada.");

        return entity.ToDto();
    }

    // ================== COMMANDS (escritura) ==================
    public async Task<PublicationDto> CreateAsync(PublicationCreateUpdateDto dto, CancellationToken ct)
    {
        Validate(dto);

        var entity = dto.FromDto();              
        _db.Publications.Add(entity);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(entity).Collection(e => e.Imagenes).LoadAsync(ct);

        return entity.ToDto();              
    }

    public async Task UpdateAsync(int id, PublicationCreateUpdateDto dto, CancellationToken ct)
    {
        Validate(dto);

        var entity = await _db.Publications
            .Include(p => p.Imagenes)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (entity is null)
            throw new NotFoundException($"Publicación {id} no encontrada.");

        entity.UpdateFromDto(dto);                
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _db.Publications.FindAsync(new object[] { id }, ct);
        if (entity is null)
            throw new NotFoundException($"Publicación {id} no encontrada.");

        _db.Publications.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<BulkDeleteResult> DeleteManyAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        if (ids is null)
            throw new DomainValidationException("Debes enviar una lista de IDs.");

        var idList = ids.Where(id => id > 0).Distinct().ToList();
        if (idList.Count == 0)
            throw new DomainValidationException("La lista de IDs está vacía.");

        var existing = await _db.Publications
                                .Where(p => idList.Contains(p.Id))
                                .ToListAsync(ct);

        var toDeleteIds = existing.Select(e => e.Id).ToList();
        var notFoundIds = idList.Except(toDeleteIds).ToList();

        _db.Publications.RemoveRange(existing);
        await _db.SaveChangesAsync(ct);

        return new BulkDeleteResult
        {
            DeletedIds = toDeleteIds,
            NotFoundIds = notFoundIds
        };
    }

    // ================== VALIDACIONES ==================
    private static void Validate(PublicationCreateUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TipoPropiedad))
            throw new DomainValidationException("El campo 'TipoPropiedad' es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.TipoOperacion))
            throw new DomainValidationException("El campo 'TipoOperacion' es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Descripcion))
            throw new DomainValidationException("El campo 'Descripcion' es obligatorio.");

        if (dto.Ambientes < 0 || dto.M2 < 0 || dto.Antiguedad < 0)
            throw new DomainValidationException("Ambientes, M2 y Antigüedad no pueden ser negativos.");

         if (dto.Lat < 0 || dto.Lng < 0)
            throw new DomainValidationException("Latitud y Longitud son obligatorias.");

        if (dto.Lat < -90 || dto.Lat > 90)
            throw new DomainValidationException("La latitud debe estar entre -90 y 90.");

        if (dto.Lng < -180 || dto.Lng > 180)
            throw new DomainValidationException("La longitud debe estar entre -180 y 180.");

        if (dto.Imagenes is not null && dto.Imagenes.Any(i => string.IsNullOrWhiteSpace(i.Url)))
            throw new DomainValidationException("Todas las imágenes deben tener una URL válida.");
    }

}
