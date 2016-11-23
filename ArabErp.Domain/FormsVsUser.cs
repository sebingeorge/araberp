using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class FormsVsUser
    {
        public int FormId { get; set; }
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string FormName { get; set; }
        public bool hasPermission { get; set; }
    }
}
