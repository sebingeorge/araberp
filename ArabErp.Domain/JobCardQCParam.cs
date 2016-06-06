using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class JobCardQCParam
    {
        public int JobCardQCParamId { get; set; }
        public int QCParamId { get; set; }
        public int JobCardQCId { get; set; }
        public string QCParamName { get; set; }
        public string QCParamValue { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
    }
}
