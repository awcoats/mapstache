using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Utf8GridApplication.Controllers
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
