using System;
using System.Collections.Generic;

namespace ClinicSystem_22180011.Models;

public partial class ExamDetail
{
    public int DetailId { get; set; }

    public int? AppointId { get; set; }

    public string? Diagnosis { get; set; }

    public string? Prescription { get; set; }

    public DateTime? LastModified22180011 { get; set; }

    public virtual Appointment? Appoint { get; set; }
}
