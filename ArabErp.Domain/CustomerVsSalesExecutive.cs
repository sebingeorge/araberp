using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{



   public class CustomerVsSalesExecutive
    {
        public int CustomerVsSalesExecutiveId { get; set; }
        public int CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string CustomerName { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public string CusCategoryName { get; set; }
        public string CustomerAddress { get; set; }
    }
 
}
