﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PrintDescription
    {
        public int PrintDescriptionId { get; set; }
        public int DeliveryChallanId { get; set; }
        public int SerialNo { get; set; }
        public string Description { get; set; }
        public string UoM { get; set; }
        public int Quantity { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
