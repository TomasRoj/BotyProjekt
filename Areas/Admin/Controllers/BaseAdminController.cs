using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BotyProjekt.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class BaseAdminController : Controller
    {
    }
}
