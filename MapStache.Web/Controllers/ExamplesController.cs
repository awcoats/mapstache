using System.Web.Mvc;

namespace MapStache.Web.Controllers
{
    public class ExamplesController : Controller
    {
        //
        // GET: /Examples/

        public ActionResult States()
        {
            return View();
        }
        public ActionResult Zips()
        {
            return View();
        }
        public ActionResult Wms()
        {
            return View();
        }
        public ActionResult Tiles()
        {
            return View();
        }
    }
}
