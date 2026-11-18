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

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Especialidad>().HasIndex(e => e.Nombre).IsUnique().HasDatabaseName("UQ_especialidades_nombre");
        modelBuilder.Entity<Paciente>().HasIndex(p => p.HistoriaClinica).IsUnique().HasDatabaseName("UQ_pacientes_historia");
        modelBuilder.Entity<Usuario>().HasIndex(u => u.Correo).IsUnique().HasDatabaseName("UQ_usuarios_correo");
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.Property(u => u.Correo).HasMaxLength(100).IsRequired();
            entity.Property(u => u.ClaveHash).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Rol).HasMaxLength(20).IsRequired();
        });
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
