using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class LoginHistory
    {
        public DateTime LoginDate { get; set; }
        public string UserName { get; set; }
        public string LoginTime { get; set; }
        public string LogoutTime { get; set; }
    }
}
