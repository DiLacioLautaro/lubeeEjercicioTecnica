using System.Collections.Generic;

namespace PruebaTecnica2025.Models.Dtos
{
    public class BulkDeleteRequest
    {
        public List<int> Ids { get; set; } = new();
    }

    public class BulkDeleteResult
    {
        public List<int> DeletedIds { get; set; } = new();
        public List<int> NotFoundIds { get; set; } = new();

        public int DeletedCount => DeletedIds.Count;
        public int NotFoundCount => NotFoundIds.Count;
    }
}
