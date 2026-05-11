using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem_22180011.Models;

public partial class Patient
{
    public int PatientId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Phone { get; set; }
    
    public DateTime? LastModified22180011 { get; set; }

    public string? UserId { get; set; }

    public int? ChosenDoctorId { get; set; }

    [ForeignKey("ChosenDoctorId")]
    public virtual Doctor? ChosenDoctor { get; set; }
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
