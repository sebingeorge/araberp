using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class QuerySheetItem
  {
        public int QuerySheetId { get; set; }
        public int QuerySheetItemId { get; set; }
        public string RoomDetails { get; set; }
        public string ExternalRoomDimension { get; set; }
        public string ColdRoomArea { get; set; }
        public string ColdRoomLocation { get; set; }
        public string TemperatureRequired { get; set; }
        public string PanelThicknessANDSpec { get; set; }
        public string DoorSizeTypeAndNumberOfDoor { get; set; }
        public string FloorDetails { get; set; }
        public string ProductDetails { get; set; }
        public string Kilowatt { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
        public string ProductIncomingTemperature { get; set; }
        public string PipeLength { get; set; }
        public string Refrigerant { get; set; }
        public string EletricalPowerAvailability { get; set; }
        public string CondenserUnit { get; set; }
        public string EvaporatorUnit { get; set; }
        public decimal Cost { get; set; }
        public decimal Quantity { get; set; }
        public decimal DoorQty { get; set; }
        public string Door { get; set; }
        public string Type { get; set; }
    }
}
