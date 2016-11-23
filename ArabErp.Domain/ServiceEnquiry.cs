using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class ServiceEnquiry
    {
        public int SaleOrderId { get; set; }
        public int isService { get; set; }
        public int isProjectBased { get; set; }
        public int ServiceEnquiryId { get; set; }
        public string ServiceEnquiryRefNo { get; set; }
        public DateTime ServiceEnquiryDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string ContactPerson { get; set; }
        public string Telephone { get; set; }
        public string VehicleMake { get; set; }
        public string VehicleRegNo { get; set; }
        public string VehicleChassisNo { get; set; }
        public string VehicleKm { get; set; }
        public string BoxMake { get; set; }
        public string BoxNo { get; set; }
        public string BoxSize { get; set; }
        public string FreezerMake { get; set; }
        public string FreezerModel { get; set; }
        public string FreezerSerialNo { get; set; }
        public string FreezerHours { get; set; }
        public string TailLiftMake { get; set; }
        public string TailLiftModel { get; set; }
        public string TailLiftSerialNo { get; set; }
        public string Complaints { get; set; }
    }
}
