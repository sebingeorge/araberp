using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class Item
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemGroupId { get; set; }
        public int ItemSubGroupId { get; set; }
        public int OrganizationId { get; set; }
    }
}
