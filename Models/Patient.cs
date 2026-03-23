using System;
using System.Collections.Generic;

namespace ClinicSystem_22180011.Models;

public partial class Patient
{
    public int PatientId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public DateTime? LastModified22180011 { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
