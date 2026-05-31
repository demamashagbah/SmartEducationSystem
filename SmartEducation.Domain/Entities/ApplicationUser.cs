
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class ApplicationUser
     : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = default!;

        public string LastName { get; set; } = default!;

        public bool IsActive { get; set; } = true;

        // Navigation Property
        public ICollection<ApplicationUserRole>
            UserRoles
        { get; set; }
            = new List<ApplicationUserRole>();
    }
}