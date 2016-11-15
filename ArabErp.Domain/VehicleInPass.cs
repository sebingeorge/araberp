using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class VehicleInPass
    {
        public int VehicleInPassId { get; set; }
        public string VehicleInPassNo { get; set; }
        public int SaleOrderId { get; set; }
        public string RegistrationNo { get; set; }
        public string ChassisNo { get; set; }
        public DateTime VehicleInPassDate { get; set; }
        public int EmployeeId { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public int SaleOrderItemId { get; set; }
        public string CustomerName { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public string SONODATE { get; set; }
        public string WorkDescr { get; set; }
        public bool InPassId { get; set; }

    }
}
