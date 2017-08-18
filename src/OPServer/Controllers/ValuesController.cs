using System.Collections.Generic;
using cloudscribe.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OPServer.Controllers
{
    [Authorize]
    [Route("/api/[controller]")]
    [Route("{sitefolder}/api/[controller]")]
    public class ValuesController
    {
        private readonly SiteContext _currentSite;
        public ValuesController(SiteContext currentSite)
        {
            _currentSite = currentSite;
        }
        private static string[] Summaries = new[]
        {
           "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warmmmm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet("")]
        public IEnumerable<string> GetValues() {
            return new[] { _currentSite.SiteName };
        }
    }
}