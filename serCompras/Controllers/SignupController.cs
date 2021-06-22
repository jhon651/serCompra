using serCompras.Views;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;


namespace sercompra.Controllers
{
    public class SignupController : Controller
    {
        sercomprasEntities dbContext = new sercomprasEntities();

        // GET: Admin
        // Metodo que nos retornara la Vista Index
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Signup(user user)
        {
            try
            {
                user.created_at = DateTime.Now;
                user.status = 1;
                user.id_role = 3;
                dbContext.users.Add(user);
                dbContext.SaveChanges();
            }
            catch (Exception e) {}
            return RedirectToAction("Index", "Signin", "");
        }
    }
}