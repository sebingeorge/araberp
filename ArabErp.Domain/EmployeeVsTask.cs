using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class EmployeeVsTask
    {
    public int    EmployeeVsTaskId { get; set; }
    public int EmployeeId                  { get; set; }
    public string JobCardTaskName { get; set; }
    [Required]
    public int JobCardTaskMasterId         { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int OrganizationId              { get; set; }
    public int isActive { get; set; }

    }
}
