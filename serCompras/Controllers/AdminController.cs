using serCompras.Views;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using serCompras.Models;
using System.IO;

namespace sercompra.Controllers
{
    public class AdminController : Controller
    {
        sercomprasEntities dbContext = new sercomprasEntities();

        public ActionResult Index()
        {
            return View();
        }

        #region Presupuestos
            public ActionResult Budgets()
            {
                return View();
            }

            public ActionResult Budget(int? id)
            {
                ViewBag.titlePart = id!=null?"Editar":"Registrar";
                return View();
            }
        #endregion

        #region Solicitudes
            public ActionResult Requests()
            {
                var requests = new List<request>();
                try
                {
                    requests = (from p in dbContext.requests orderby p.created_at descending select p).ToList();
                }
                catch (Exception) { }

                ViewBag.Create = true;
                ViewBag.Text = "Solicitud";
                ViewBag.URL = "/Admin/request";

                return View(requests);
            }

           
            public ActionResult Request(int? id)
            {
                request request = (from r in dbContext.requests where r.id == id select r).FirstOrDefault();
                ViewBag.Back = true;
                ViewBag.URL = "/Admin/Requests";
                ViewBag.titlePart = id != null ? "Editar" : "Registrar";
                ViewBag.idProvider = id;
                if (id != null)
                {
                    //ViewBag.Articles = Docs(provider.id);
                }
                return View(request);
            }

            [HttpPost]
            public ActionResult Request(request request)
            {
                try
                {
                    if (request.id == 0)
                    {
                        request.created_at = DateTime.Now;
                        request.status = 0;
                        dbContext.requests.Add(request);
                        dbContext.SaveChanges();
                        var id = request.id;
                        return RedirectToAction("Request", id);

                    }
                    else
                    {
                        var requestData = dbContext.providers.Find(request.id);
                        requestData.updated_at = DateTime.Now;
                        TryUpdateModel(requestData, "", new string[] { "purpose", "date_limit" });
                        dbContext.SaveChanges();
                        return RedirectToAction("Requests");

                    }
                }
                catch (Exception e) { }
                return View();
            }
            public ActionResult Deleterequest(int id)
            {
                try
                {
                    request request = dbContext.requests.Find(id);
                    dbContext.requests.Remove(request);
                    dbContext.SaveChanges();
                }
                catch (Exception e)
                { }
                return RedirectToAction("requests");
            }
            public List<document> Items(int Id)
            {
                var documents = new List<document>();
                try
                {
                    documents = (from d in dbContext.documents where d.id_provider == Id select d).ToList();
                }
                catch (Exception) { }
                return documents;
            }


        #endregion

        #region Proveedores
        public ActionResult Providers()
            {
                var providers = new List<provider>();
                try
                {
                    providers = (from p in dbContext.providers orderby p.created_at descending select p).ToList();
                }
                catch (Exception) { }
                ViewBag.Create = true;
                ViewBag.Text = "Proveedor";
                ViewBag.URL = "/Admin/Provider";
                return View(providers);
            }
            public ActionResult Provider(int? id)
            {
                provider provider = (from p in dbContext.providers where p.id == id select p).FirstOrDefault();
                ViewBag.Back = true;
                ViewBag.URL = "/Admin/Providers";
                ViewBag.titlePart = id != null ? "Editar" : "Registrar";
                   ViewBag.idProvider = id;
                if (id != null) {
                    ViewBag.Documents = Docs(provider.id);
                }
                return View(provider);
            }
            [HttpPost]
            public ActionResult Provider(provider provider)
            {
                try
                {
                    if (provider.id == 0)
                    {
                        provider.created_at = DateTime.Now;
                        provider.status = 0;
                        dbContext.providers.Add(provider);
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        var providerData = dbContext.providers.Find(provider.id);
                        providerData.updated_at = DateTime.Now;
                        TryUpdateModel(providerData, "", new string[] { "legal_name", "rut", "phone", "address", "email", "updated_at", "status" });
                        dbContext.SaveChanges();
                    }
                    return RedirectToAction("providers");
                }
                catch (Exception e) { }
                return View();
            }
            public ActionResult DeleteProvider(int id)
            {
                try
                {
                    provider provider = dbContext.providers.Find(id);
                    dbContext.providers.Remove(provider);
                    dbContext.SaveChanges();
                }
                catch (Exception e)
                { }
                return RedirectToAction("providers");
            }
            public List<document> Docs(int Id)
            {
                var documents = new List<document>();
                try
                {
                    documents = (from d in dbContext.documents where  d.id_provider == Id select d).ToList();
                }
                catch (Exception) { }
                return documents;
            }
            [HttpPost]
            public ActionResult AddDoc(int Id, DocumentUpload documentUpload)
            {
            if (documentUpload.Files.Count > 0) { 
                var file = documentUpload.Files[0];
                if (ModelState.IsValid)
                {
                    if (file == null)
                    {
                        ModelState.AddModelError("File", "Please Upload Your file");
                    }
                    else if (file.ContentLength > 0)
                    {
                        int MaxContentLength = 1024 * 1024 * 10; //10 MB
                   
                        if (file.ContentLength > MaxContentLength)
                        {
                            ModelState.AddModelError("File", "EL archivo es muy grande, el tamaño maximo permitido es: " + MaxContentLength + " MB");
                        }
                        else
                        {
                            try
                            {
                                //TO:DO
                                string nameAdd = Id + "_Document_"+file.FileName;

                                var fileName = Path.GetFileName(nameAdd);
                                var path = Path.Combine(Server.MapPath("~/Content/repository/"), fileName);
                                file.SaveAs(path);
                                ModelState.Clear();
                                ViewBag.Message = "Documento guardado satisfactoriamente";

                                document document = new document
                                {
                                    id_provider = Id,
                                    path = "~/Content/repository/" + fileName,
                                    size = file.ContentLength,
                                    content = file.ContentType,
                                    name = fileName,
                                    status = 0,
                                    created_at = DateTime.Now
                                };

                                dbContext.documents.Add(document);

                                var providerData = dbContext.providers.Find(Id);
                                providerData.status = 3;
                                TryUpdateModel(providerData, "", new string[] { "status" });
                                dbContext.SaveChanges();

                            }
                            catch (Exception e)
                            {
                                ViewBag.Message = "no se cargo el documento";
                            }


                            dbContext.SaveChanges();
                        }
                    }
                }
            }
            return RedirectToAction("Profile");
            }
            public ActionResult DeleteDoc(int Id)
            {
                var document = dbContext.documents.Find(Id);
                dbContext.documents.Remove(document);
                dbContext.SaveChanges();
                return RedirectToAction("Profile");
            }
            public ActionResult ApproveDoc(int Id)
            {
                var document = dbContext.documents.Find(Id);
                document.status = 1;
                TryUpdateModel(document, "", new string[] { "status" }); 
                dbContext.SaveChanges();
                return RedirectToAction("Profile");
            }
            public ActionResult RejectDoc(int Id)
            {
                var document = dbContext.documents.Find(Id);
                document.status = 2;
                TryUpdateModel(document, "", new string[] { "status" });
                dbContext.SaveChanges();
                return RedirectToAction("Profile");
            }
        #endregion

        #region Usuarios
        public ActionResult Users()
            {
                var usuarios = new List<user>();
                try
                {
                    usuarios = (from u in dbContext.users where u.status == 1 select u).ToList();
                }
                catch (Exception) { }
                ViewBag.Create = true;
                ViewBag.Text = "Usuario";
                ViewBag.URL = "/Admin/User";
                return View(usuarios);


            }
            public ActionResult User(int? id)
                {
                    user user = (from u in dbContext.users where u.Id == id select u).FirstOrDefault();
                    ViewBag.Back = true;
                    ViewBag.URL = "/Admin/Users";
                    ViewBag.titlePart = id != null ? "Editar" : "Registrar";
            
                    return View(user);
                }
            [HttpPost]
            public ActionResult User(user user)
            {
                try
                {
                    if (user.Id == 0)
                    {
                        user.created_at = DateTime.Now;
                        user.status = 1;
                        dbContext.users.Add(user);
                        dbContext.SaveChanges();
                    }
                    else 
                    {
                        var userData = dbContext.users.Find(user.Id);
                        userData.update_at = DateTime.Now;
                        TryUpdateModel(userData, "", new string[] { "firstname", "lastname", "email", "phone", "username", "id_role", "update_at" ,"status" });
                        dbContext.SaveChanges();
                        if (user.password != null && user.password != "")
                        {
                            TryUpdateModel(userData, "", new string[] { "password" });
                            dbContext.SaveChanges();
                        }

                    }
                    return RedirectToAction("Users");
                }
                catch (Exception e){}
                return View();
            }
            public ActionResult DeleteUser(int id)
            {
                try
                {
                    user usuario = dbContext.users.Find(id);
                    dbContext.users.Remove(usuario);
                    dbContext.SaveChanges();
                }
                catch (Exception e)
                {}
                return RedirectToAction("Users");
            }
            public ActionResult Profile() 
            {
                
                ViewBag.User = new user();
                ViewBag.Provider = new provider();
                ViewBag.Documents = this.Docs(1);
                return View();
            }
        #endregion

    }
}