using SenkaKichi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Web;
using SenkaKichi.DbModels;

namespace SenkaKichi
{
    public abstract class ControllerBase : Controller
    {
        protected SenkaRepository _repository;

        protected ApplicationUserManager userManager {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        protected ApplicationSignInManager signInManager {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationSignInManager>(); }
        }

        protected SenkaContext db {
            get { return HttpContext.GetOwinContext().GetUserManager<SenkaContext>(); }
        }

        protected SenkaRepository repository {
            get {
                if (_repository == null) {
                    _repository = new SenkaRepository(db);
                }
                return _repository;
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (signInManager != null) {
                    signInManager.Dispose();
                }
                if (userManager != null) {
                    userManager.Dispose();
                }
                if (db != null) {
                    db.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
