# Google-Authentication

1. First of all, Create new project ASP.NET Core Web (Model-View-Controller). Don't forget to enable SSL / Https
2. Once the project has created, then install Microsoft.AspNetCore.Authentication.Google via nuget package manager
3. Register to console google, in orrder to get ClientId and ClientSecret (Follow another tutorial to see in detail)
4. Add google in services (More detail, please see on Startup.cs / Program.cs)
5. Don't forget to add app.Authentication() before app.Authorization() at pipeline / middleware configuration (in order to enable authentication process) 
6. Deatil of code is typed in HomeController

### Code Explanation
Imagine we have a button called "Login using Google". The button's event will redirect to this below method

```
[Route("google-login")]
public async Task GoogleLogin()
{
    await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties()
    {
        RedirectUri = Url.Action("GoogleResponse")
    });
}
 ```
 
 Once above method is executed, then this method will be invoked. In result variable will success in case email and password has validated by google. Claims will store any value that given by google
 
 ```
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
 }
 ```
 Actually we only need call HttpContect.SignOutAsync(), since without define the session we still authenticate. Therfore in GoogleResponse method we don't need set the session
 ```
 [Authorize]
 [Route("google-logout")]
 public async Task<IActionResult> GoogleLogout()
 {
    HttpContext.Session.Clear(); // remove session (actually we don't need session in this authentication process)

    await HttpContext.SignOutAsync(); // remove authentication

    return RedirectToAction("Index");
 }
 ```
