using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlineStoreSystem.ViewModels;
using System.Security.Claims;

namespace OnlineStoreSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IConfiguration configuration, ILogger<AccountController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // LOGIN (GET)
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // LOGIN (POST)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var connectionString = _configuration.GetConnectionString("OnlineStore");

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT dp.name,
                           CASE 
                               WHEN dp.name = 'AdminUser' THEN 'Admin'
                               WHEN dp.name = 'ManagerUser' THEN 'Manager'
                               WHEN dp.name = 'WorkerUser' THEN 'Worker'
                               ELSE 'Unknown'
                           END AS Role
                    FROM sys.database_principals dp
                    JOIN sys.sql_logins sl ON sl.name = dp.name
                    WHERE dp.name = @UserType
                      AND PWDCOMPARE(@Password, sl.password_hash) = 1;
                ";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserType", model.UserType);
                command.Parameters.AddWithValue("@Password", model.Password);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var username = reader.GetString(0);
                    var role = reader.GetString(1);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.Role, role),
                        new Claim("LoginTime", DateTime.UtcNow.ToString("O"))
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTime.UtcNow.AddHours(8)
                        });

                    _logger.LogInformation(
                        "Користувач {Username} увійшов як {Role}",
                        username, role);

                    TempData["Success"] = $"Вітаємо, {role}! Ви успішно увійшли.";

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Неправильний тип користувача або пароль");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при вході в систему");
                ModelState.AddModelError(
                    string.Empty,
                    "Сталася помилка при вході. Спробуйте ще раз.");
                return View(model);
            }
        }

        //  LOGOUT
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name;

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation(
                "Користувач {Username} вийшов із системи",
                username);

            TempData["Info"] = "Ви успішно вийшли з системи.";
            return RedirectToAction("Index", "Home");
        }

        //ACCESS DENIED
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
