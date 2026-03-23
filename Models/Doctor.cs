using System;
using System.Collections.Generic;

namespace ClinicSystem_22180011.Models;

public partial class Doctor
{
    public int DoctorId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Specialty { get; set; }

    public DateTime? LastModified22180011 { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
