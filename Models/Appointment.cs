using System;
using System.Collections.Generic;

namespace ClinicSystem_22180011.Models;

public partial class Appointment
{
    public int AppointId { get; set; }

    public int? PatientId { get; set; }

    public int? DoctorId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public string? Status { get; set; }

    public DateTime? LastModified22180011 { get; set; }

    public virtual Doctor? Doctor { get; set; }

    public virtual ICollection<ExamDetail> ExamDetails { get; set; } = new List<ExamDetail>();

    public virtual Patient? Patient { get; set; }
}
