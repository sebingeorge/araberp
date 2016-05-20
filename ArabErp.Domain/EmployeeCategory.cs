using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class EmployeeCategory
    {
        public int EmpCategoryId { get; set; }
        public string EmpCategoryRefNo { get; set; }
        public string EmpCategoryName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
