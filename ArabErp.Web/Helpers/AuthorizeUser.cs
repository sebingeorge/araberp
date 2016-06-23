using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Security;

namespace ArabErp.Web.Helpers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AuthorizeUser : FilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            HttpCookie userCookie = (HttpCookie)HttpContext.Current.Session["user"];
            if (!SkipAuthorization(filterContext))
            {
                bool validUser = ValidateAuthorizationCookie(filterContext, userCookie);
                if (!validUser)
                {
                    filterContext.Result = new HttpUnauthorizedResult();
                }
            }
        }
        private static bool SkipAuthorization(AuthenticationContext filterContext)
        {
            Contract.Assert(filterContext != null);

            return filterContext.ActionDescriptor.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any()
                   || filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any();
        }
        public bool ValidateAuthorizationCookie(AuthenticationContext context, HttpCookie cookie)
        {
            if (cookie == null)
                return false;

            var authCookie = context.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                var ticketInfo = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticketInfo != null)
                {
                    if (ticketInfo.Expired == false && ticketInfo.Name == ticketInfo.UserData)
                    {
                        var userCookie = context.HttpContext.Request.Cookies["userCookie"];
                        if (userCookie != null && userCookie["SecId"] == cookie["SecId"] && userCookie["UserName"] == cookie["UserName"])
                        {
                            var _userdata = ticketInfo.UserData.Split('|');
                            if (_userdata.Length == userCookie.Values.AllKeys.Length)
                            {
                                if (userCookie["UserId"] != null && userCookie["UserId"] == _userdata[0]
                                    && userCookie["UserName"] != null && userCookie["UserName"] == _userdata[1]
                                    && userCookie["UserEmail"] != null && userCookie["UserEmail"] == _userdata[3]
                                    && userCookie["UserRole"] != null && userCookie["UserRole"] == _userdata[4])
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            if (filterContext.Result is HttpUnauthorizedResult || filterContext.Result == null)
            {
                filterContext.Result = new RedirectToRouteResult("Default",
                    new System.Web.Routing.RouteValueDictionary{
                        {"controller", "Account"},
                        {"action", "Login"},
                        {"returnUrl", filterContext.HttpContext.Request.RawUrl}
                    });
            }
        }
    }
}