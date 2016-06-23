﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class Consumption
    {
        public int ConsumptionId { get; set; }
        public string ConsumptionNo { get; set; }
        public DateTime ConsumptionDate { get; set; }
        public int JobCardId { get; set; }
        public string SpecialRemarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public List<ConsumptionItem> ConsumptionItems { get; set; }
    }
}