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
    [SessionCheck]
    public class AdminController : Controller
    {
        sercomprasEntities dbContext = new sercomprasEntities();

        public ActionResult Index()
        {
            var counters = dbContext.Database.SqlQuery<Counters>(
                        "SELECT " +
                        "(SELECT count(*) from provider) providersCount, " +
                        "(SELECT count(*) from request)	requestCount, " +
                        "(SELECT count(*) from budget) budgetsCount," +
                        "(SELECT count(*) from provider where status = 0) newprovidersCount ").FirstOrDefault();
            return View(counters);
        }

        #region Presupuestos
            public ActionResult Budgets(int? id)
            {
                string param = id != null ? "WHERE b.id_request = " + id : "";
                var budgets = dbContext.Database.SqlQuery<Budgets>(
                        "SELECT b.id, b.id_provider, b.id_request, b.id_user, b.created_at, b.status, b.updated_at, r.date_limit, concat(u.firstname,' ', u.lastname) fullname " +
                        "FROM [budget] b " +
                        "INNER JOIN request r ON b.id_request = r.id " +
                        "LEFT JOIN[user] u ON r.id_user = u.id " + param).ToList();
                return View(budgets);
            }
            public ActionResult Budget(int? id)
            {
                var budget = dbContext.Database.SqlQuery<Budget>(
                        "SELECT b.id, b.id_provider, b.id_request, b.id_user, b.created_at, b.status, b.updated_at, r.date_limit, concat(u.firstname,' ', u.lastname) fullname " +
                        "FROM [budget] b " +
                        "INNER JOIN request r ON b.id_request = r.id " +
                        "LEFT JOIN[user] u ON r.id_user = u.id " +
                        "WHERE b.id = " + id).FirstOrDefault();
                ViewBag.Back = true;
                ViewBag.titlePart = id!=null?"Editar":"Registrar";
                if (id != null)
                {
                    ViewBag.Articles = GetBudgetArticles((int)id);
                }
            return View(budget);
            }
            public ActionResult MakeAQuote(int id)
            {
                request request = (from r in dbContext.requests where r.id == id select r).FirstOrDefault();

                budget budget = new budget
                {
                    id_provider = (int)Session["Id_provider"],
                    id_request = request.id,
                    id_user = (int)Session["Id"],
                    status = 0,
                    created_at = DateTime.Now
                };

                dbContext.budgets.Add(budget);
                dbContext.SaveChanges();

                var listArticles = this.GetArticles(id);
                
                foreach(request_detail item in listArticles) 
                {
                    budget_detail article = new budget_detail
                    {
                        id_budget = budget.id,
                        quantity = item.quantity,
                        description = item.description,
                        status = 0,
                        created_at = DateTime.Now
                    };
                    dbContext.budget_detail.Add(article);
                    dbContext.SaveChanges();
                }

                request.updated_at = DateTime.Now;
                request.status = 3;
                TryUpdateModel(request, "", new string[] {"status" });
                dbContext.SaveChanges();

            return RedirectToAction("Budget", new { id = budget.id });
            }
            public ActionResult AcceptBudget(int Id)
            {
                var budget = dbContext.budgets.Find(Id);
                budget.status = 2;
                TryUpdateModel(budget, "", new string[] { "status" });
                dbContext.SaveChanges();

                var request = dbContext.requests.Find(budget.id_request);
                request.status = 4;
                TryUpdateModel(request, "", new string[] { "status" });
                dbContext.SaveChanges();

                return RedirectToAction("budget",new {id = Id });
            }
            public ActionResult RejectBudget(int Id)
            {
                var budget = dbContext.budgets.Find(Id);
                budget.status = 3;
                TryUpdateModel(budget, "", new string[] { "status" });
                dbContext.SaveChanges();
                return RedirectToAction("budget", new { id = Id });
            }

        #endregion

        #region Solicitudes
        public ActionResult Requests()
            {
                var requests = new List<Request>();
                var role_id = (int)Session["Id_role"];

                if (role_id == 2) {
                    int id_user = (int)Session["Id"];
                    requests = dbContext.Database.SqlQuery<Request>(
                            "SELECT r.id, r.id_user, r.purpose, r.date_limit, r.created_at, r.status, concat(u.firstname,' ', u.lastname) fullname " +
                            "FROM [request] r " +
                            "LEFT JOIN[user] u ON r.id_user = u.id " +
                            "WHERE r.id_user = " + id_user).ToList();
                
                }else if(role_id == 3) {
                   
                    if (Session["Id_provider"] != null)
                    {
                        int id_provider = (int)Session["Id_provider"];
                        requests = dbContext.Database.SqlQuery<Request>(
                            "SELECT r.id, r.id_user, r.purpose, r.date_limit, r.created_at, r.status, concat(u.firstname,' ', u.lastname) fullname " +
                            "FROM [request] r " +
                            "LEFT JOIN request_provider rp ON r.id = rp.id_request " +
                            "LEFT JOIN[user] u ON r.id_user = u.id " +
                            "WHERE rp.id_provider = " + id_provider).ToList();
                    }
                }

                ViewBag.Create = (role_id == 2);
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
                    List<provider> providers = (from p in dbContext.providers where p.status == 2 orderby p.legal_name ascending select p).ToList();
                    ViewBag.ListProviders = providers;
                    ViewBag.Articles = GetArticles((int)id);
                    ViewBag.Providers = GetProviders((int)id);
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
                        request.id_user = (int)Session["id"];
                        dbContext.requests.Add(request);
                        dbContext.SaveChanges();
                        var id = request.id;
                        return RedirectToAction("Request", new { id = id});
                    }
                    else
                    {
                        var requestData = dbContext.requests.Find(request.id);
                        var dataupdate = DateTime.Now;
                        requestData.updated_at = dataupdate;
                        TryUpdateModel(requestData, "", new string[] { "purpose", "date_limit", "updated_at" });
                        dbContext.SaveChanges();
                        return RedirectToAction("Requests");
                    }
                }
                catch (Exception e) { }
                return View();
            }
            public ActionResult DeleteRequest(int id)
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
        #endregion

        #region Lista Articulos
            public List<request_detail> GetArticles(int IdRequest)
            {
                var articles = new List<request_detail>();
                try
                {
                    articles = (from rd in dbContext.request_detail where rd.id_request == IdRequest select rd).ToList();
                }
                catch (Exception) { }
                return articles;
        }
            public List<budget_detail> GetBudgetArticles(int id)
            {
                var articles = new List<budget_detail>();
                try
                {
                    articles = (from rd in dbContext.budget_detail where rd.id_budget == id select rd).ToList();
                }
                catch (Exception) { }
                return articles;
            }
            [HttpPost]
            public ActionResult SetPrice(int id, int price, int idBudget)
            {
                var article = dbContext.budget_detail.Find(id);
                article.price = price;
                TryUpdateModel(article, "", new string[] { "price" });
                dbContext.SaveChanges();

                var budget = dbContext.budgets.Find(idBudget);
                budget.status = 1;
                TryUpdateModel(budget, "", new string[] { "status" });
                dbContext.SaveChanges();


            return RedirectToAction("Budget", new { id = idBudget });
            }
            [HttpPost]
            public ActionResult AddArticle(int id, request_detail article)
            {
                article.created_at = DateTime.Now;
                article.id_request = id;
                dbContext.request_detail.Add(article);
                dbContext.SaveChanges();
                return RedirectToAction("Request", new { id = id });
            }
            public ActionResult DelArticle(int idRequest, int id)
            {
                request_detail article = dbContext.request_detail.Find(id);
                dbContext.request_detail.Remove(article);
                dbContext.SaveChanges();
                return RedirectToAction("Request", new { id = idRequest });
            }
        #endregion

        #region Lista Proveedores
            public List<ProviderList> GetProviders(int IdRequest)
            {
                var providers = dbContext.Database.SqlQuery<ProviderList>(
                    "SELECT rp.id, p.legal_name " +
                    "FROM [request_provider] rp " +
                    "LEFT JOIN [provider] p ON rp.id_provider = p.id " +
                    "WHERE rp.id_request =" + IdRequest).ToList();

                return providers;
            }
            [HttpPost]
            public ActionResult AddProvider(int id, request_provider provider)
            {
                var idProvider = provider.id_provider;
                List<request_provider> providers = (from rp in dbContext.request_provider where rp.id_request == id && rp.id_provider == idProvider select rp).ToList();

                if (providers.Count <= 0) {
                    provider.created_at = DateTime.Now;
                    provider.id_request = id;
                    dbContext.request_provider.Add(provider);
                    dbContext.SaveChanges();
                }

                var request = dbContext.requests.Find(id);
                request.status = 1;
                request.updated_at = DateTime.Now;
                TryUpdateModel(request, "", new string[] { "status" });
                dbContext.SaveChanges();

                return RedirectToAction("Request", new { id = id });
                }
            public ActionResult DelProvider(int idRequest, int id)
            {
                request_provider provider = dbContext.request_provider.Find(id);
                dbContext.request_provider.Remove(provider);
                dbContext.SaveChanges();
                return RedirectToAction("Request", new { id = idRequest });
            }
        #endregion

        #region Lista Usuarios
            public List<user> GetUsersProvider()
            {
                var users = new List<user>();
                try
                {
                    users = (from u in dbContext.users where u.id_role == 3 select u).ToList();
                }
                catch (Exception) { }
                return users;
            }
        #endregion

        #region Proveedores
            public ActionResult Providers()
            {
                var providers = dbContext.Database.SqlQuery<Providers>(
                    "SELECT p.id, p.legal_name, p.rut, p.phone, p.address, concat(u.firstname,' ', u.lastname) fullname, p.status " +
                    "FROM [provider] p " +
                    "LEFT JOIN[user] u ON p.id_user = u.id ").ToList();

                //providers = (from p in dbContext.providers orderby p.created_at descending select p).ToList();
               
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
                ViewBag.Users = GetUsersProvider();

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
                        var role_id = (int)Session["Id_role"];

                        provider.created_at = DateTime.Now;
                        provider.status = 0;
                        dbContext.providers.Add(provider);
                        dbContext.SaveChanges();
                       
                        if (role_id == 3)
                        {
                            Session["Id_provider"] = provider.id;
                            Session["Provider_name"] = provider.legal_name;
                            return RedirectToAction("Profile");
                        }
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
            public ActionResult AddDoc(int Id, byte type,DocumentUpload documentUpload)
            {
            if (documentUpload.Files.Count > 0) { 
                var file = documentUpload.Files[0];

                List<document> existDoc = (from d in dbContext.documents where d.id_provider == Id && d.type == type select d).ToList();

                bool docExists = !(existDoc.Count > 0);
               
                string docTypeName = type == 1 ? "_Documento_de_registro_" : type == 2 ? "_RUT_" : type == 3 ? "_Certificacion_Bancaria_" : "_No Definido_";
                string date = DateTime.Now.ToString().Split(' ')[0].Replace(@"/","");
                string ext = "."+file.FileName.ToString().Split('.').Last();

                if (docExists)
                {

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
                                    string nameAdd = Id + docTypeName + date + ext;

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
                                        type = type,
                                        content = file.ContentType,
                                        name = fileName,
                                        status = 0,
                                        created_at = DateTime.Now
                                    };

                                    dbContext.documents.Add(document);
                                    dbContext.SaveChanges();

                                    var providerData = dbContext.providers.Find(Id);
                                    providerData.status = 1;
                                    TryUpdateModel(providerData, "", new string[] { "status" });
                                    dbContext.SaveChanges();

                                }
                                catch (Exception e)
                                {
                                    ViewBag.Message = "no se cargo el documento";
                                }
                            }
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
            public ActionResult ApproveDoc(int idProvider, int Id)
            {
                var document = dbContext.documents.Find(Id);
                document.status = 1;
                TryUpdateModel(document, "", new string[] { "status" }); 
                dbContext.SaveChanges();

                var num_docs = new List<document>();
                num_docs = (from d in dbContext.documents where d.id_provider == idProvider && d.status == 1 select d).ToList();

                if (num_docs.Count >= 3) {
                    var provider = dbContext.providers.Find(idProvider);
                    provider.status = 2;
                    TryUpdateModel(provider, "", new string[] { "status" });
                    dbContext.SaveChanges();
                }

                return RedirectToAction("Provider", new { id = idProvider });
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
                var usuarios = new List<User>();
                try
                {

                 usuarios = dbContext.Database.SqlQuery<User>(
                    "SELECT u.id, u.firstname, u.email, u.lastname, u.username, u.id_role, u.status, r.name role " +
                    "FROM[user] u " +
                    "LEFT JOIN role r ON u.id_role = r.id ").ToList();

                    //usuarios = (from u in dbContext.users where u.status == 1 select u).ToList();
                }
                catch (Exception e) { }
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
                int idUser = (int)Session["Id"];

                user user = (from u in dbContext.users where u.Id == idUser select u).FirstOrDefault();
                ViewBag.User = user;
              
                if (Session["Id_provider"]!=null)
                {
                    int idProvider = (int)Session["Id_provider"];
                    provider provider = (from p in dbContext.providers where p.id == idProvider select p).FirstOrDefault();
                    ViewBag.Provider = provider;
                    ViewBag.Documents = this.Docs((int)Session["Id_provider"]);
                }
            return View();
            }
        #endregion

    }
    public class SessionCheck : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpSessionStateBase session = filterContext.HttpContext.Session;
            if (session != null && session["Id"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary {
                                { "Controller", "Signin" },
                                { "Action", "Index" }
                                });
            }
        }
    }
}