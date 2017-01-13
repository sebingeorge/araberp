using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class ProjectRoomAndUnitDetails
    {
        public string RoomDetails { get; set; }
        public string ExternalRoomDimension { get; set; }
        public string Refrigerant { get; set; }
        public decimal Quantity { get; set; }
        public string CondensingUnit { get; set; }
        public string Evaporator { get; set; }
        public string TemperatureRequired { get; set; }
    }
}
