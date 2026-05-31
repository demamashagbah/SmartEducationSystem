using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class Assignment : BaseEntity
    {
        public string Title { get; set; }

        public DateTime DueDate { get; set; }
    }
}
