using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ArabErp.Domain
{
    public class ServiceRegister
    {
        public string CustomerName { get; set; }
        public string VehicleModelName { get; set; }
        public string SERVICE { get; set; }
        public string RegistrationNo { get; set; }
        public string Unit { get; set; }
        public DateTime LASTSERVICE { get; set; }
        public DateTime NEXTESERVICE { get; set; }

    }
}
