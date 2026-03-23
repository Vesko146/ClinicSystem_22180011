using System;
using System.Collections.Generic;

namespace ClinicSystem_22180011.Models;

public partial class ViewDoctorSchedule
{
    public string DoctorName { get; set; } = null!;

    public string? PatientName { get; set; }

    public DateTime AppointmentDate { get; set; }

    public string? Status { get; set; }
}
