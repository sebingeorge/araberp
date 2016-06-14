using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabErp.Web.Models
{
    public class PagedPendingGRNViewModel
    {
        public IEnumerable<ArabErp.Domain.GRN> GRNs { get; set; }
        public Pager Pager { get; set; }
    }
}