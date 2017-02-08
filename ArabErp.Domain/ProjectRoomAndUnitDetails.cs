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



        //public string RoomDetails { get; set; }
        public string FreezerTemperature { get; set; }
        public string FreezerDimension { get; set; }
        public string FreezerCondensingUnit { get; set; }
        public string FreezerEvaporator { get; set; }
        public string FreezerRefrigerant { get; set; }
        public string FreezerQuantity { get; set; }
        public string SerialNo { get;set; }
    }
}