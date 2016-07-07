using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class ModuleVsUser
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public int UserId { get; set; }
        public int isPermission { get; set; }
    }
}
