using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem_22180011.Models
{
    public class User : IdentityUser
    {
        [NotMapped]
        public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

        [NotMapped]
        public int? RoleId { get; set; }

        [NotMapped]
        public DateTime? LastModified22180011 { get; set; }

    public virtual Role? Role { get; set; }
    }
}