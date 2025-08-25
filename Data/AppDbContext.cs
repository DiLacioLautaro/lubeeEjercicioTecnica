using Microsoft.EntityFrameworkCore;
using PruebaTecnica2025.Models;

namespace PruebaTecnica2025.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Publication> Publications => Set<Publication>();
    public DbSet<PublicationImage> PublicationImages => Set<PublicationImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Publication>(e =>
        {
            e.ToTable("Publications"); // nombre exacto de la tabla en la BD

            e.HasKey(p => p.Id);

            // Mapeo 1:1 a los nombres reales de columna
            e.Property(p => p.Id).HasColumnName("Id");
            e.Property(p => p.TipoPropiedad).HasColumnName("TipoPropiedad");
            e.Property(p => p.TipoOperacion).HasColumnName("TipoOperacion");
            e.Property(p => p.Descripcion).HasColumnName("Descripcion");
            e.Property(p => p.Ambientes).HasColumnName("Ambientes");
            e.Property(p => p.M2).HasColumnName("M2");
            e.Property(p => p.Antiguedad).HasColumnName("Antiguedad");
            e.Property(p => p.Lat).HasColumnName("Lat");
            e.Property(p => p.Lng).HasColumnName("Lng");

            e.HasMany(p => p.Imagenes)
             .WithOne(i => i.Publication!)
             .HasForeignKey(i => i.PublicationId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PublicationImage>(e =>
        {
            e.ToTable("PublicationImages");

            e.HasKey(i => i.Id);
            e.Property(i => i.Id).HasColumnName("Id");
            e.Property(i => i.Url).HasColumnName("Url");
            e.Property(i => i.PublicationId).HasColumnName("PublicationId");
        });

        base.OnModelCreating(modelBuilder);
    }

}
