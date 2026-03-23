using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicSystem_22180011.Models;

public partial class Doctor
{
    public int DoctorId { get; set; }


    [Display(Name = "Име на лекар")]
    [Required(ErrorMessage = "Името е задължително")]
    public string FullName { get; set; }

    [Display(Name = "Специалност")]
    public string? Specialty { get; set; }

    [ConcurrencyCheck]
    public DateTime? LastModified_22180011 { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
