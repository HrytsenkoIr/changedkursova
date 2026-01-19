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
        // Доступ до appsettings.json
        private readonly IConfiguration _configuration;
        // Логер для запису подій (вхід, вихід, помилки)
        private readonly ILogger<AccountController> _logger;

        public AccountController(IConfiguration configuration, ILogger<AccountController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // LOGIN (GET)
         // щоб можна було навіть незалогіненим користувачам
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            //урл повернення
            ViewData["ReturnUrl"] = returnUrl;
            //показати форму логіну
            return View();
        }

        // LOGIN (POST)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] // захист від CSRF атак
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Підключення до БД
                var connectionString = _configuration.GetConnectionString("OnlineStore");

    //підключ до sql server
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
                //подає параметри з форми
                command.Parameters.AddWithValue("@UserType", model.UserType);
                command.Parameters.AddWithValue("@Password", model.Password);

//читає бд
                using var reader = await command.ExecuteReaderAsync();
//Якщо знайдено
                if (await reader.ReadAsync())
                {
                    var username = reader.GetString(0);
                    var role = reader.GetString(1);

//список інфи
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.Role, role),
                        new Claim("LoginTime", DateTime.UtcNow.ToString("O"))
                    };

//ідентичність кукі
                    var claimsIdentity = new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);
//створення кукі
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTime.UtcNow.AddHours(8) //кукі на 8 годин. може зміню
                        });
//подія в лог
                    _logger.LogInformation(
                        "Користувач {Username} увійшов як {Role}",
                        username, role);
//повідом
                    TempData["Success"] = $"Вітаємо, {role}! Ви успішно увійшли.";

//поверненя не ретурн урл яку я раніше вказала
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }
//якщо логін або пароль неправильні
                ModelState.AddModelError(string.Empty, "Неправильний тип користувача або пароль");
                return View(model);
            }
            catch (Exception ex)
            {
                //логування
                _logger.LogError(ex, "Помилка при вході в систему");
                ModelState.AddModelError(
                    string.Empty,
                    "Сталася помилка при вході. Спробуйте ще раз.");//загальна помилка
                return View(model);
            }
        }

        //  LOGOUT
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout() //тока авторизованим
        {
            var username = User.Identity?.Name;//ім'я щоб був лог про вихід

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);//видалення кукі

            _logger.LogInformation(
                "Користувач {Username} вийшов із системи",//тута лог
                username);

            TempData["Info"] = "Ви успішно вийшли з системи.";//для користувача повідомляшка
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
