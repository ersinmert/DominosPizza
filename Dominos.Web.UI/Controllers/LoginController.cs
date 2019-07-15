using Dominos.Common.Configuration;
using Dominos.Web.UI.Business;
using Dominos.Web.UI.Business.Helper.Login;
using Dominos.Web.UI.Business.Helper.Register;
using Dominos.Web.UI.Models.Login;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dominos.Web.UI.Controllers
{
    public class LoginController : Controller
    {
        public LoginController(IOptions<DominosConfig> options, SessionHelper session, CookieHelper cookie)
        {
            _config = options.Value;
            _session = session;
            _cookie = cookie;
        }

        private readonly DominosConfig _config;
        private readonly SessionHelper _session;
        private readonly CookieHelper _cookie;

        public IActionResult Login()
        {
            if (_session.Customer != null)
            {
                return RedirectToAction("Index", "Home");
            }
            var model = new LoginViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model, LoginSubmits submit)
        {
            var instance = new LoginInstance(this, ModelState, submit, _config, _session, _cookie);
            instance.Provider.Execute(model);

            return View(model);
        }

        public IActionResult Register()
        {
            if (_session.Customer != null)
            {
                return RedirectToAction("Index", "Home");
            }
            var model = new RegisterViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model, RegisterSubmits submit)
        {
            var instance = new RegisterInstance(this, ModelState, submit, _config, _session, _cookie);
            instance.Provider.Execute(model);

            return View(model);
        }
    }
}