using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabErp.Domain
{
    public class JobCardTask
    {
        public int JobCardTaskId { get; set; }
        public int JobCardId { get; set; }
        public string Task { get; set; }
        public string Employee { get; set; }


    }
}