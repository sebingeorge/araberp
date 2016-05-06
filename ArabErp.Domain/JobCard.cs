using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabErp.Domain
{
    public class JobCard
    {
        public int JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public List<JobCardTask> JobCardTasks { get; set; }
    }
}