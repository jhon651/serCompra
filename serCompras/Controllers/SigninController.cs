using serCompras.Views;
using System;
using System.Data.Entity.Infrastructure;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using serCompras.Models;
using System.Data.SqlClient;


namespace sercompra.Controllers
{
    public class SigninController : Controller
    {
        sercomprasEntities dbContext = new sercomprasEntities();
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(String username, String password)
        {
            var users = new Login();
            try
            {
                users = dbContext.Database.SqlQuery<Login>(
                "SELECT u.id, u.firstname, u.lastname, u.username, u.id_role, r.name role, p.id id_provider, p.legal_name " +
                "FROM[user] u " +
                "INNER JOIN role r ON u.id_role = r.id " +
                "LEFT JOIN provider p ON u.id = p.id_user " +
                "WHERE u.username = '" + username + "' AND u.password = '" + password+"'").FirstOrDefault();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                var msg = "<script language='javascript'>" +
                        "Swal.fire({icon: 'info',title: 'Error de conexion...',text: 'No pudimos procesar su solicitud!',confirmButtonColor: '#2B56BA'})" +
                        "</script>";
                //var msg = "<script language='javascript'>Swal.fire({title:'',text: '" + message + "',type:'" + notificationType + "',allowOutsideClick: false,allowEscapeKey: false,allowEnterKey: false})" + "</script>";
                TempData["notification"] = msg;
                return RedirectToAction("Index");

            }

            if (users !=null)
            {
                Session["Id"] = users.id;
                Session["Username"] = users.username;
                Session["Id_provider"] = users.id_provider;
                Session["Provider_name"] = users.legal_name;
                Session["Fullname"] = (users.firstname + " " + users.lastname);
                Session["Firstname"] = users.firstname;
                Session["Lastname"] = users.lastname;
                Session["Role"] = users.role;
                Session["Id_role"] = users.id_role;


                return RedirectToAction("Index", "Admin", "");
            }
            else
            {
                var msg = "<script language='javascript'>" +
                    "Swal.fire({icon: 'error',Title: 'Oops...',text: 'Credenciales Erroneas!',confirmButtonColor: '#2B56BA',footer: '<a href=\"#\">Olvido su contrasena?</a>'})" +
                    "</script>";
                //var msg = "<script language='javascript'>Swal.fire({title:'',text: '" + message + "',type:'" + notificationType + "',allowOutsideClick: false,allowEscapeKey: false,allowEnterKey: false})" + "</script>";
                TempData["notification"] = msg;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();
            Response.AddHeader("Cache-control", "no-store, must-revalidate, private, no-cache");
            Response.AddHeader("Pragma", "no-cache");
            Response.AddHeader("Expires", "0");
            Response.AppendToLog("window.location.reload();");
            Session.Clear();
            Session.RemoveAll();

            return RedirectToAction("Index");
        }

    }
}