using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class ApplicationRole
     : IdentityRole<Guid>
    {
        public ICollection<ApplicationUserRole>
            UserRoles
        { get; set; }
            = new List<ApplicationUserRole>();
    }
}
