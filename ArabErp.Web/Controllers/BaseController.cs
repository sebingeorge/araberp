﻿using ArabErp.Web.Helpers;
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
    }
}