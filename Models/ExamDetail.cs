using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem_22180011.Models;

public partial class ExamDetail
{
    [Key]
    public int DetailId { get; set; }

    public int? AppointId { get; set; }

    public string? Diagnosis { get; set; }

    public string? Prescription { get; set; }

    [Column("LastModified22180011")]
    public DateTime? LastModified22180011 { get; set; }

    [Required(ErrorMessage = "Типът на прегледа е задължителен.")]
    [Display(Name = "Тип преглед")]
    public string PaymentType { get; set; } = "Платен"; 
    public virtual Appointment? Appoint { get; set; }
}
