using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem_22180011.Models;

public partial class Patient
{
    public int PatientId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;


    [Required(ErrorMessage = "Телефонният номер е задължителен.")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Телефонният номер трябва да бъде точно 10 цифри.")]
    [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Невалиден формат. Трябва да започва с 0 и да съдържа общо 10 цифри.")]
    public string Phone { get; set; }

    public string Email { get; set; }

    [Required(ErrorMessage = "ЕГН-то е задължително.")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "ЕГН трябва да бъде точно 10 символа.")]
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Невалиден формат на ЕГН (само цифри).")]
    public string EGN { get; set; }

    public DateTime LastModified22180011 { get; set; }
    public string? UserId { get; set; } 
    public int? ChosenDoctorId { get; set; } 

    public virtual Doctor? ChosenDoctor { get; set; } 

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}