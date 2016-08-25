using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class ERPAlerts
    {
        public int AlertId { get; set; }
        public string AlertName { get; set; }
        public int HasPermission { get; set; }
        public int UserId { get; set; }
    }
}
