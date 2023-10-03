
using Helpers.AuthorizationHelpers;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [ServiceFilter(typeof(AdminLoginAuthorization))]
    public class BaseController : Controller
    {
        
    }
}
