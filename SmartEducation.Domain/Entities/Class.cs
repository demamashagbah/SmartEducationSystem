using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class Class
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;
    }
}
