using ArabErp.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    [AuthorizeUser]
    public class BaseController : Controller
    {
        public int UserID { 
            get
            {
                HttpCookie usr = (HttpCookie)Session["user"];
                int Id = Convert.ToInt32(usr["UserId"]);
                return Id;
            }
            set
            {
            }
        }
        public string UserName
        {
            get
            {
                return Request.Cookies["user"]["UserName"];
            }
            set
            {

            }
        }
        public int OrganizationId {
            get
            {
                int Id = Convert.ToInt32(Request.Cookies["user"]["Organization"]);
                return Id;
            }
            set
            {

            }
        }
    }
}