using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class WorkDescription
    {

        public WorkDescription()
        {
            WorkVsItems = new List<WorkVsItem>();
            WorkVsTasks = new List<WorkVsTask>();
    }
        public int WorkDescriptionId { get; set; }
        public string WorkDescriptionRefNo { get; set; }
        public string VehicleModelName { get; set; }
        public string FreezerUnitName { get; set; }
        public string BoxName { get; set; }
        public int? SlNo { get; set; }
        public int? VehicleModelId { get; set; }
        public int? FreezerUnitId { get; set; }
        public int? BoxId { get; set; }
        public string WorkDescr { get; set; }
        public string WorkDescrShortName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public bool isNewInstallation { get; set; }
        public bool isRepair { get; set; }
        public bool isSubAssembly { get; set; }

        public bool isProjectBased { get; set; }
        public decimal MinimumAmount { get; set; }

        public List<WorkVsItem> WorkVsItems { get; set; }
        public List<WorkVsTask> WorkVsTasks { get; set; }
    }
}
