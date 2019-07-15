using Dominos.Common.Configuration;
using Dominos.Web.UI.Business.Helper.Home;
using Dominos.Web.UI.Models.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dominos.Web.UI.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(IOptions<DominosConfig> options)
        {
            _config = options.Value;
        }

        private readonly DominosConfig _config;

        public IActionResult Index()
        {
            var model = new HomeViewModel();

            var instance = new HomeInstance(this, ModelState, HomeSubmits.List, _config);
            instance.Provider.Execute(model);

            return View(model);
        }
    }
}