using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
namespace ArabErp.Domain
{
   public  class QCParameters
    {

       public int QCParamId { get; set; }
        [Required]
       public string QCParamName { get; set; }
       
        public string QCParaName { get; set; }
        [Required]
        public int QCParaId { get; set; }
        [Required]
        public string QCRefNo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrganizationId { get; set; }

          }
}
