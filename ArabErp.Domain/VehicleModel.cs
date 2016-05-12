using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class VehicleModel
    {
        public int VehicleModelId { get; set; }
        public string VehicleModelRefNo { get; set; }
        public string VehicleModelName { get; set; }
        public string VehicleModelDescription { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
