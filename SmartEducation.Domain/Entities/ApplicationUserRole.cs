using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class ApplicationUserRole
     : IdentityUserRole<Guid>
    {
        public ApplicationUser User { get; set; }
            = default!;

        public ApplicationRole Role { get; set; }
            = default!;
    }
}
