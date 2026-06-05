using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace ArcadeProject.Controllers;

public class CultureController : Controller
{
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Set(string culture, string returnUrl = "/")
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );
        return LocalRedirect(returnUrl);
    }
}