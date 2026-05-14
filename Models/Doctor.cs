using System;
using System.Collections.Generic;

namespace ClinicSystem_22180011.Models;

public partial class Doctor
{
    public int DoctorId { get; set; }

    public string FullName { get; set; } = null!;

    public DateTime LastModified22180011 { get; set; }

    public string? UserId { get; set; }

    public string? Specialty { get; set; }

    public string? Biography { get; set; }

    public string? ScheduleGroup { get; set; }


    public virtual ICollection<DoctorComment> DoctorComments { get; set; } = new List<DoctorComment>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
