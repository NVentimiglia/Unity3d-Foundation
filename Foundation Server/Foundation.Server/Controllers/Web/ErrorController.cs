using System.Web.Mvc;

namespace Foundation.Server.Controllers
{
    public class ErrorController : Controller
    {
        //[OutputCache(Duration = 86400, VaryByParam = "none")]
        public ActionResult Index()
        {
            ViewBag.Title = "500 : Internal Error";

            ViewBag.Description = "Sorry, an internal error has occurred. Please try again.";

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = false, message = ViewBag.Title }, JsonRequestBehavior.AllowGet);
            }
            return View("Error");
        }


        //  [OutputCache(Duration = 86400, VaryByParam = "none")]
        public ActionResult NoAccess()
        {
            ViewBag.Title = "Access Denied";
            ViewBag.Description = "Sorry, this resource is restricted.";
            
            if (Request.IsAjaxRequest())
            {
                return Json(new { success = false, message = ViewBag.Title }, JsonRequestBehavior.AllowGet);
            }
            return View("Error");
        }

        //  [OutputCache(Duration = 86400, VaryByParam = "none")]
        public ActionResult NotFound()
        {
            ViewBag.Title = "404 : Not Found";
            ViewBag.Description = "Sorry, the page was not found.";

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = false, message = ViewBag.Title }, JsonRequestBehavior.AllowGet);
            }
            return View("Error");
        }
    }
}
