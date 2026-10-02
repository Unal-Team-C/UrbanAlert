using Domain.Reportes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistencia.Configuraciones;

public class ReporteConfiguration : IEntityTypeConfiguration<Reporte>
{
    public void Configure(EntityTypeBuilder<Reporte> builder)
    {
        builder.ToTable("Reportes");

        builder.HasKey(reporte => reporte.Id);
        builder.Property(reporte => reporte.Id).ValueGeneratedNever();

        builder.Property(reporte => reporte.TipoDano)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(reporte => reporte.Descripcion)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(reporte => reporte.IdCoordenada)
            .IsRequired();

        builder.Property(reporte => reporte.UrlImagen)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(reporte => reporte.IdUsuario)
            .IsRequired();

        builder.Property(reporte => reporte.NivelEmergencia)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(reporte => reporte.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(reporte => reporte.Fecha)
            .IsRequired();

        builder.Property(reporte => reporte.IdResponsable);

        builder.Property(reporte => reporte.MotivoRechazo)
            .HasMaxLength(2000);
    }
}
