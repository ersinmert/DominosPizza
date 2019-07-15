using Dominos.Common.Configuration;
using Dominos.Web.UI.Business;
using Dominos.Web.UI.Business.Helper.Home;
using Dominos.Web.UI.Models.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dominos.Web.UI.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(IOptions<DominosConfig> options, SessionHelper session, CookieHelper cookie)
        {
            _config = options.Value;
            _session = session;
            _cookie = cookie;
        }

        private readonly DominosConfig _config;
        private readonly SessionHelper _session;
        private readonly CookieHelper _cookie;

        public IActionResult Index()
        {
            var model = new HomeViewModel();

            var instance = new HomeInstance(this, ModelState, HomeSubmits.List, _config, _session, _cookie);
            instance.Provider.Execute(model);

            return View(model);
        }

        [HttpPost]
        public IActionResult Index(HomeViewModel model, HomeSubmits submit)
        {
            var instance = new HomeInstance(this, ModelState, submit, _config, _session, _cookie);
            instance.Provider.Execute(model);

            return RedirectToAction("Index", "Basket");
        }
    }
}