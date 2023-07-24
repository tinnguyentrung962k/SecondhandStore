﻿using System;
using System.Collections.Generic;

namespace SecondhandStore.Models
{
    public partial class ReportImage
    {
        public int ImageId { get; set; }
        public int ReportId { get; set; }
        public string ImageUrl { get; set; } = null!;

        public virtual Report Report { get; set; } = null!;
    }
}
