using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace ErgoUtilsUploadService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetVersionInfo()
        {
            return Ok(new { CommitHash = GetGitHash() });
        }

        private static string GetGitHash()
        {
            var hash = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            if (string.IsNullOrWhiteSpace(hash) || hash.Length < 8)
            {
                return null;
            }

            return hash[^8..];
        }
    }
}
