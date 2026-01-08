using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace ReportSystem.Controllers;

public class HelloWorldController : Controller
{

    // GET: /HelloWorld/
    public IActionResult Index()
    {
        return View();
    }

    // GET: /HelloWorld/Welcome/ 
    public IActionResult Welcome(string name = "Guest", int numTimes = 1)
    {
        SetWelcomeViewData(name, numTimes);
        return View();
    }

    // A method to set the "Welcome" ViewData
    private void SetWelcomeViewData(string name, int numTimes)
    {
        ViewData["Message"] = "Hello " + HtmlEncoder.Default.Encode(name);
        ViewData["NumTimes"] = numTimes;
    }
}