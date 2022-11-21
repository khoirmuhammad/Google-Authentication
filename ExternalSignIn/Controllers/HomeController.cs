using ExternalSignIn.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ExternalSignIn.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // This method need authentication first before accessing
        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Route("google-login")]
        public async Task GoogleLogin()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }

        [Route("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
                return AccessDenied();

            var claims = result.Principal?.Identities.FirstOrDefault()?
                .Claims.Select(claim => new
                {
                    claim.Issuer,
                    claim.OriginalIssuer,
                    claim.Type,
                    claim.Value
                });

            if (claims == null)
                return AccessDenied();

            //(actually we don't need session in this authentication process)
            foreach (var claim in claims)
            {
                string[] typeSplit = claim.Type.Split("/");
                string type = typeSplit[typeSplit.Length - 1];

                HttpContext.Session.SetString(type, claim.Value);
                ViewData[type] = HttpContext.Session.GetString(type);
            }


            return View();
            //return Json(claims);
        }

        [Authorize]
        [Route("google-logout")]
        public async Task<IActionResult> GoogleLogout()
        {
            HttpContext.Session.Clear(); // remove session (actually we don't need session in this authentication process)

            await HttpContext.SignOutAsync(); // remove authentication

            return RedirectToAction("Index");
        }

    }
}