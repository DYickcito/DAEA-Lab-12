using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UmaSalud.Entidades.Models;

namespace UmaSalud.Datos;

public partial class ClinicaContext : DbContext
{
    public ClinicaContext(DbContextOptions<ClinicaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Especialidad> Especialidades { get; set; }

    public virtual DbSet<Paciente> Pacientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Especialidad>().HasIndex(e => e.Nombre).IsUnique().HasDatabaseName("UQ_especialidades_nombre");
        modelBuilder.Entity<Paciente>().HasIndex(p => p.HistoriaClinica).IsUnique().HasDatabaseName("UQ_pacientes_historia");
        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.HasOne(d => d.Especialidad).WithMany(p => p.Pacientes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_pacientes_especialidades");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
