using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class ModulePermission
    {
        public bool Admin { get; set; }
        public bool Purchase { get; set; }
        public bool Sales { get; set; }
        public bool Project { get; set; }
        public bool Transportation { get; set; }
        public bool Finance { get; set; }
        public bool MISReports { get; set; }
    }
}
