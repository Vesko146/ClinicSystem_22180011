using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;


namespace ClinicSystem_22180011.Models;

public partial class Clinic22180011Context : DbContext
{
    public Clinic22180011Context()
    {
    }

    public Clinic22180011Context(DbContextOptions<Clinic22180011Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<ExamDetail> ExamDetails { get; set; }

    public virtual DbSet<Log22180011> Log22180011s { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<ViewDoctorSchedule> ViewDoctorSchedules { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-OASI1O7\\MSSQLSERVER01;Database=Clinic_22180011;Trusted_Connection=True; TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointId).HasName("PK__Appointm__DCC1C95971A93C2F");

            entity.ToTable("Appointments", "22180011", tb => tb.HasTrigger("trg_LogAppointments"));

            entity.Property(e => e.AppointId).HasColumnName("AppointID");
            entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
            entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
            entity.Property(e => e.LastModified22180011)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LastModified_22180011");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK__Appointme__Docto__412EB0B6");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK__Appointme__Patie__403A8C7D");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId).HasName("PK__Doctors__2DC00EDFD9124C4F");

            entity.ToTable("Doctors", "22180011", tb => tb.HasTrigger("trg_LogDoctors"));

            entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.LastModified_22180011)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LastModified_22180011");
            entity.Property(e => e.Specialty).HasMaxLength(100);
        });

        modelBuilder.Entity<ExamDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__ExamDeta__135C314D1BC0F75F");

            entity.ToTable("ExamDetails", "22180011", tb => tb.HasTrigger("trg_LogExamDetails"));

            entity.Property(e => e.DetailId).HasColumnName("DetailID");
            entity.Property(e => e.AppointId).HasColumnName("AppointID");
            entity.Property(e => e.LastModified22180011)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LastModified_22180011");

            entity.HasOne(d => d.Appoint).WithMany(p => p.ExamDetails)
                .HasForeignKey(d => d.AppointId)
                .HasConstraintName("FK__ExamDetai__Appoi__44FF419A");
        });

        modelBuilder.Entity<Log22180011>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__log_2218__5E5499A83F1D9785");

            entity.ToTable("log_22180011", "22180011");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.OperationTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OperationType).HasMaxLength(10);
            entity.Property(e => e.TableName).HasMaxLength(100);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patients__970EC346CD79073D");

            entity.ToTable("Patients", "22180011", tb => tb.HasTrigger("trg_LogPatients"));

            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastModified22180011)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LastModified_22180011");
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A369964A3");

            entity.ToTable("Roles", "22180011");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACFB44F69C");

            entity.ToTable("Users", "22180011");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.LastModified22180011)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LastModified_22180011");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Users__RoleID__4E88ABD4");
        });

        modelBuilder.Entity<ViewDoctorSchedule>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("View_DoctorSchedule", "22180011");

            entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
            entity.Property(e => e.DoctorName).HasMaxLength(100);
            entity.Property(e => e.PatientName).HasMaxLength(101);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
