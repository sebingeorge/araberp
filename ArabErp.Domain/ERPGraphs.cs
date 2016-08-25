using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class ERPGraphs
    {
        public int GraphId { get; set; }
        public string GraphName { get; set; }
        public int HasPermission { get; set; }
        public int UserId { get; set; }
    }
}
