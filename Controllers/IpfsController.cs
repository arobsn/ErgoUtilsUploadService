using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ErgoUtilsUploadService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IpfsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public IpfsController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var client = new RestClient("https://api.nft.storage");
            var request = CreateRequestFrom(file);

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                await HitGateway(GetCid(response));
            }

            return ResponseFrom(response);
        }

        private RestRequest CreateRequestFrom(IFormFile file)
        {
            var request = new RestRequest("upload", Method.POST);
            request.AddHeader("Authorization", $"Bearer {_config["ApiKey"]}");

            using var stream = file.OpenReadStream();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            request.AddParameter(file.Name, ms.ToArray(), ParameterType.RequestBody);

            return request;
        }

        private static string GetCid(IRestResponse response)
        {
            var obj = ParseResponse(response.Content);
            return obj["value"]["cid"].ToString();
        }

        /// <summary>
        /// This method sends a GET request to the gateway in order to
        /// get imediate caching and cut down on the perceived wait time,
        /// as suggested at https://github.com/anon-real/ErgoUtils/pull/2#issuecomment-917662151
        /// </summary>
        private static async Task HitGateway(string cid)
        {
            if (string.IsNullOrWhiteSpace(cid))
            {
                return;
            }

            var gatewayUrl = $"https://cloudflare-ipfs.com/ipfs/{cid}";
            var client = new RestClient(gatewayUrl) { Timeout = 0 };
            await client.ExecuteAsync(new RestRequest(Method.GET));
        }

        [HttpGet("check/{cid}")]
        public async Task<IActionResult> Check([FromRoute] string cid)
        {
            var client = new RestClient("https://api.nft.storage");
            var request = new RestRequest(Method.GET);
            request.AddUrlSegment("check", cid);
            request.AddHeader("Authorization", $"Bearer {_config["ApiKey"]}");

            var response = await client.ExecuteAsync(request);
            return ResponseFrom(response);
        }

        private ObjectResult ResponseFrom(IRestResponse response)
        {
            return ResponseFrom((int)response.StatusCode, ParseResponse(response.Content));
        }

        private ObjectResult ResponseFrom(int statusCode, object value)
        {
            return StatusCode(statusCode, value);
        }

        private static JObject ParseResponse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return JObject.Parse(value);
        }
    }
}
