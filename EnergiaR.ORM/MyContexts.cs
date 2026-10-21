using EnergiaR.DTO;
using Microsoft.EntityFrameworkCore;

namespace EnergiaR.ORM;

// DbContext de backend
public partial class MyContexts : DbContext
{
    readonly String PathApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public MyContexts()
    {
    }

    public MyContexts(DbContextOptions<MyContexts> Opts) : base(Opts)
    {
    }

    public virtual DbSet<TEmpre> TEmpr { get; set; }
    public virtual DbSet<TCaUso> TCUso { get; set; }
    public virtual DbSet<TClien> TClie { get; set; }
    public virtual DbSet<VClien> VClie { get; set; }
    public virtual DbSet<TMedid> TMedi { get; set; }
    public virtual DbSet<VMedid> VMedi { get; set; }
    public virtual DbSet<TRecar> TReca { get; set; }
    public virtual DbSet<VRecar> VReca { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder OB)
        => OB.UseSqlite($"Data Source={Path.Combine(PathApp, "EnergiaRDB.db")}");

    protected override void OnModelCreating(ModelBuilder MB)
    {
        // Empresas prestadoras de servicio de energía
        MB.Entity<TEmpre>(En =>
        {
            En.ToTable("TEmpre");
            En.Property(E => E.ID).HasColumnName("ID");
            En.Property(E => E.NDoc).UseCollation("NOCASE").HasColumnName("NDoc").HasMaxLength(125);
            En.Property(E => E.RaSo).UseCollation("NOCASE").HasColumnName("RaSo").HasMaxLength(250);
            En.Property(E => E.Dire).UseCollation("NOCASE").HasColumnName("Dire").HasMaxLength(2500);
            En.Property(E => E.Tele).UseCollation("NOCASE").HasColumnName("Tele").HasMaxLength(2500);
            En.Property(E => E.Mail).UseCollation("NOCASE").HasColumnName("Mail").HasMaxLength(2500);
        });

        // Medidores: Casos de uso
        MB.Entity<TCaUso>(En =>
        {
            En.ToTable("TCaUso");
            En.Property(E => E.ID).HasColumnName("ID");
            En.Property(E => E.CUso).UseCollation("NOCASE").HasColumnName("CUso").HasMaxLength(20);
            En.Property(E => E.CDes).UseCollation("NOCASE").HasColumnName("CDes").HasMaxLength(100);
        });

        // Medidores Clientes/Propietarios
        MB.Entity<TClien>(En =>
        {
            En.ToTable("TClien");
            En.Property(E => E.ID).HasColumnName("ID");
            En.Property(E => E.TDoc).UseCollation("NOCASE").HasColumnName("TDoc").HasMaxLength(20);
            En.Property(E => E.NDoc).UseCollation("NOCASE").HasColumnName("NDoc").HasMaxLength(100);
            En.Property(E => E.Nomb).UseCollation("NOCASE").HasColumnName("Nomb").HasMaxLength(100);
            En.Property(E => E.Sexo).HasColumnName("Sexo").
                HasConversion(V => V ? 1 : 0, V => V == 1).
                HasColumnType("INTEGER").
                IsRequired();
        });

        // Medidores Clientes/Propietarios | Valores inferidos
        MB.Entity<VClien>(En =>
        {
            En.HasNoKey().ToView("VClien");
            En.Property(E => E.ID).HasColumnName("ID");
            En.Property(E => E.TDoc).UseCollation("NOCASE").HasColumnName("TDoc").HasMaxLength(20);
            En.Property(E => E.NDoc).UseCollation("NOCASE").HasColumnName("NDoc").HasMaxLength(100);
            En.Property(E => E.Nomb).UseCollation("NOCASE").HasColumnName("Nomb").HasMaxLength(100);
            En.Property(E => E.Sexo).UseCollation("NOCASE").HasColumnName("Sexo");
        });

        // Medidores
        MB.Entity<TMedid>(En =>
        {
            En.ToTable("TMedid");
            En.Property(E => E.ID).HasColumnName("ID");
            En.Property(E => E.MNIC).UseCollation("NOCASE").HasColumnName("MNIC").HasMaxLength(125);
            En.Property(E => E.Medi).UseCollation("NOCASE").HasColumnName("Medi").HasMaxLength(125);
            En.Property(E => E.Dire).UseCollation("NOCASE").HasColumnName("Dire").HasMaxLength(2500);
            En.Property(E => E.CUso).HasColumnName("CUso");
            En.Property(E => E.Prop).HasColumnName("Prop");

            En.HasOne(D => D.CaUsoNav).
                WithMany(P => P.Medid).
                HasForeignKey(D => D.CUso).
                OnDelete(DeleteBehavior.Restrict);

            En.HasOne(D => D.ClienNav).
                WithMany(P => P.Medid).
                HasForeignKey(D => D.Prop).
                OnDelete(DeleteBehavior.Restrict);
        });

        // Medidores | Valores inferidos
        MB.Entity<VMedid>(En =>
        {
            En.HasNoKey().ToView("VMedid");
            En.Property(E => E.ID).HasColumnName("ID");
            En.Property(E => E.MNIC).UseCollation("NOCASE").HasColumnName("MNIC");
            En.Property(E => E.Medi).UseCollation("NOCASE").HasColumnName("Medi");
            En.Property(E => E.Dire).UseCollation("NOCASE").HasColumnName("Dire");
            En.Property(E => E.CUso).UseCollation("NOCASE").HasColumnName("CUso");
            En.Property(E => E.Docu).UseCollation("NOCASE").HasColumnName("Docu");
            En.Property(E => E.Nomb).UseCollation("NOCASE").HasColumnName("Nomb");
        });

        // Recargas
        MB.Entity<TRecar>(En =>
        {
            En.ToTable("TRecar");
            En.Property(E => E.ID).HasColumnName("ID");
            En.Property(E => E.Empr).HasColumnName("Empr");
            En.Property(E => E.FRec).
                HasDefaultValueSql("DATE('now', 'localtime')").
                HasColumnType("DATE").
                HasColumnName("FRec");
            En.Property(E => E.HRec)
                .HasDefaultValueSql("TIME('now', 'localtime')")
                .HasColumnType("TIME")
                .HasColumnName("HRec");
            En.Property(E => E.CSeg).UseCollation("NOCASE").HasColumnName("CSeg").HasMaxLength(100);
            En.Property(E => E.ITra).UseCollation("NOCASE").HasColumnName("ITra").HasMaxLength(50);
            En.Property(E => E.Apro).UseCollation("NOCASE").HasColumnName("Apro").HasMaxLength(100);
            En.Property(E => E.Medi).HasColumnName("Medi");
            En.Property(E => E.PINR).UseCollation("NOCASE").HasColumnName("PINR").HasMaxLength(100);
            En.Property(E => E.VRec).HasColumnName("VRec");
            En.Property(E => E.Abon).HasColumnName("Abon");
            En.Property(E => E.VSub).HasColumnName("VSub");
            En.Property(E => E.VKWH).HasColumnName("VKWH");
            En.Property(E => E.Comp).HasColumnName("Comp");
            En.Property(E => E.MesK).HasColumnName("MesK");
            En.Property(E => E.MesS).HasColumnName("MesS");

            En.HasOne(D => D.EmpreNav).
                WithMany(P => P.Recar).
                HasForeignKey(D => D.Empr).
                OnDelete(DeleteBehavior.Restrict);

            En.HasOne(D => D.MedidNav).
                WithMany(P => P.Recar).
                HasForeignKey(D => D.Medi).
                OnDelete(DeleteBehavior.Restrict);
        });

        // Recargas | Valores inferidos
        MB.Entity<VRecar>(En =>
        {
            En.HasNoKey().ToView("VRecar");
            En.Property(E => E.ID).HasColumnName("ID");
            En.Property(E => E.RaSo).UseCollation("NOCASE").HasColumnName("RaSo");
            En.Property(E => E.NDoc).UseCollation("NOCASE").HasColumnName("NDoc");
            En.Property(E => E.FRec).HasColumnType("DATE").HasColumnName("FRec");
            En.Property(E => E.HRec).HasColumnType("TIME").HasColumnName("HRec");
            En.Property(E => E.CSeg).UseCollation("NOCASE").HasColumnName("CSeg");
            En.Property(E => E.ITra).UseCollation("NOCASE").HasColumnName("ITra");
            En.Property(E => E.Apro).UseCollation("NOCASE").HasColumnName("Apro");
            En.Property(E => E.Medi).UseCollation("NOCASE").HasColumnName("Medi");
            En.Property(E => E.Dire).UseCollation("NOCASE").HasColumnName("Dire");
            En.Property(E => E.CUso).UseCollation("NOCASE").HasColumnName("CUso");
            En.Property(E => E.Docu).UseCollation("NOCASE").HasColumnName("Docu");
            En.Property(E => E.Nomb).UseCollation("NOCASE").HasColumnName("Nomb");
            En.Property(E => E.PINR).UseCollation("NOCASE").HasColumnName("PINR");

            #region Mapeo INTEGER
            En.Property(E => E.IntVRec).HasColumnName("VRec");
            En.Property(E => E.IntAbon).HasColumnName("Abon");
            En.Property(E => E.IntVSub).HasColumnName("VSub");
            En.Property(E => E.IntVKWH).HasColumnName("VKWH");
            En.Property(E => E.IntComp).HasColumnName("Comp");
            En.Property(E => E.IntMesK).HasColumnName("MesK");
            En.Property(E => E.IntMesS).HasColumnName("MesS");
            #endregion

            #region Ignorar propiedades calculadas (Decimal)
            En.Ignore(E => E.VRec);
            En.Ignore(E => E.Abon);
            En.Ignore(E => E.VAbo);
            En.Ignore(E => E.SRec);
            En.Ignore(E => E.VSub);
            En.Ignore(E => E.Neto);
            En.Ignore(E => E.VKWH);
            En.Ignore(E => E.Comp);
            En.Ignore(E => E.MesK);
            En.Ignore(E => E.MesS);
            #endregion
        });

        OnModelCreatingPartial(MB);
    }

    partial void OnModelCreatingPartial(ModelBuilder MB);
}
