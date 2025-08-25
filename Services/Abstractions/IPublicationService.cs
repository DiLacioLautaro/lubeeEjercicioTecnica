using PruebaTecnica2025.Models.Dtos;

namespace PruebaTecnica2025.Services.Abstractions;

public interface IPublicationService
{
    Task<List<PublicationDto>> GetAllAsync(CancellationToken ct);
    Task<PublicationDto> GetOneAsync(int id, CancellationToken ct);
    Task<PublicationDto> CreateAsync(PublicationCreateUpdateDto dto, CancellationToken ct);
    Task UpdateAsync(int id, PublicationCreateUpdateDto dto, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<BulkDeleteResult> DeleteManyAsync(IEnumerable<int> ids, CancellationToken ct);

}
