﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OidcWebApp_B2C.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index(string message)
        {
            ViewBag.Message = message;
            return View("Error");
        }
    }
}