using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;


namespace sercompra.Controllers
{
    public class SigninController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login()
        {
            return RedirectToAction("Index", "Admin", "");
        }

    }
}