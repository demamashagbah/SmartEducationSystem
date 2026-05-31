using SmartEducation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Domain.Entities
{
    public class Announcement : BaseEntity
    {
        public string Title { get; set; }

        public string Content { get; set; }
    }
}
