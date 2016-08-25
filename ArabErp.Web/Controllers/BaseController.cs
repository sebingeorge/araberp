using ArabErp.DAL;
using ArabErp.Domain;
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
        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            try
            {
                HttpCookie usr = Request.Cookies["userCookie"] as HttpCookie;
                int Id = Convert.ToInt32(usr["UserId"]);

                IEnumerable<ModuleVsUser> modules;
                if(Session["menupermissions"] == null)
                {
                    UserRepository repo = new UserRepository();
                    modules = repo.GetModulePermissions(Id);
                    Session["menupermissions"] = modules;
                }
                else
                {
                    modules = (IEnumerable<ModuleVsUser>)Session["menupermissions"];
                    Session["menupermissions"] = modules;
                }
                
                ModulePermission permission = new ModulePermission();
                
                foreach (var item in modules)
                {
                    if (item.ModuleName == "Admin")
                    {
                        permission.Admin = true;
                    }
                    else if (item.ModuleName == "Purchase")
                    {
                        permission.Purchase = true;
                    }
                    else if (item.ModuleName == "Sales")
                    {
                        permission.Sales = true;
                    }
                    else if (item.ModuleName == "Project")
                    {
                        permission.Project = true;
                    }
                    else if (item.ModuleName == "Transportation")
                    {
                        permission.Transportation = true;
                    }
                    else if (item.ModuleName == "Finance")
                    {
                        permission.Finance = true;
                    }
                    else if (item.ModuleName == "MIS Repots")
                    {
                        permission.MISReports = true;
                    }
                }
                ViewBag.ModulePermissions = permission;                
            }
            catch
            {
                ModulePermission permission = new ModulePermission();
                ViewBag.ModulePermissions = permission;
            }
            return base.BeginExecuteCore(callback, state);
        }
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
                HttpCookie usr = (HttpCookie)Session["user"];
                return usr["UserName"].ToString();
            }
            set
            {

            }
        }
        public int OrganizationId {
            get
            {
                HttpCookie usr = (HttpCookie)Session["user"];
                int Id = Convert.ToInt32(usr["Organization"]);
                return Id;
            }
            set
            {

            }
        }
        public DateTime FYStartdate
        {
            get
            {
                FinancialYearRepository repo = new FinancialYearRepository();
                return repo.GetFinStartDate(OrganizationId);

            }
            set
            {

            }
        }
        public DateTime FYEnddate
        {
            get
            {
                FinancialYearRepository repo = new FinancialYearRepository();
                return repo.GetFinEndDate(OrganizationId);

            }
            set
            {

            }
        }

    }
}