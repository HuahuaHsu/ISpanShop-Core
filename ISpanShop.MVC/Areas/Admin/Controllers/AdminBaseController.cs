using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public abstract class AdminBaseController : Controller
    {
    }
}
