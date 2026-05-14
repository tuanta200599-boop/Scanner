using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Scanner.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
       
    }
}
