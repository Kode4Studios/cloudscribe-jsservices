using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OPServer.Controllers
{
    public static class RequestExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.Headers != null)
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";
            return false;
        }
    }

    [Route("Error")]
    public class ErrorController : Controller
    {
        [Route("/Default/{statusCode?}")]
        public IActionResult Oops(int? statusCode)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ErrorCode: " + statusCode?.ToString());
            Console.ResetColor();
            
            if (Request.IsAjaxRequest())
                return Content("Ajax Error");
            else
                return Content("Html Error");
        }

    }
}