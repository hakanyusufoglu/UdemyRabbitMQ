using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace UdemyRabbitMQWeb.ExcelCreate.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string Email,string Password)
        {
            //bu emaile ait kullanıcı var mı?
            var hasUser=await _userManager.FindByEmailAsync(Email);
            
            if(hasUser == null) { return View();}
            
            //kullanıcı varsa giriş yaptır, kalıcı olsun ve 3 kez girince bloke olsun mu false  yapıyoruz
            var signInResult=await _signInManager.PasswordSignInAsync(hasUser, Password,true,false);

            if(!signInResult.Succeeded)
            {
                return View();
            }
            return RedirectToAction(nameof(HomeController.Index),"Home");
        }
    }
}
