using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShareXUpload.Controllers {

    [ApiController]
    public class ShareXController : ControllerBase {
        private readonly ILogger<ShareXController> logger;
        private readonly IConfiguration configuration;

        public ShareXController(IConfiguration configuration, ILogger<ShareXController> logger) {
            this.logger = logger;
            this.configuration = configuration;
        }

        [HttpPost, DisableRequestSizeLimit]
        [Route("/Upload")]
        public async Task<string> Put([FromForm] ShareXInput Item) {
            if (Item.Secret == configuration["SecretKey"] && this.Request.Form.Files.Count > 0) {
                IFormFile RetrivedFile = this.Request.Form.Files[0];
                string Filename = RandomString(8);
                string Filetype = RetrivedFile.FileName[(RetrivedFile.FileName.IndexOf('.') + 1)..];

                using (Stream FileUpload = System.IO.File.Create("./Files/" + Filename + $".{Filetype}")) {
                    await RetrivedFile.CopyToAsync(FileUpload).ConfigureAwait(false);
                }

                this.Response.StatusCode = 202;
                return JsonSerializer.Serialize(new ShareXOutput("OK", null, $"{Filename}.{Filetype}"));
            }

            this.Response.StatusCode = 403;
            return JsonSerializer.Serialize(new ShareXOutput("ERROR", "Invalid Auth or File"));

            static string RandomString(int length) {
                const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
                return new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[new Random().Next(s.Length)]).ToArray());
            }
        }

        [HttpGet]
        [Route("/Files/{Filename}")]
        public async Task<IActionResult> Get(string Filename) {
            if (System.IO.File.Exists("./Files/" + Filename)) {
                FileExtensionContentTypeProvider ContentTypeProvider = new();
                ContentTypeProvider.TryGetContentType("./Files/" + Filename, out string ContentType);

                byte[] FileBytes = await System.IO.File.ReadAllBytesAsync("./Files/" + Filename);
                return File(FileBytes, ContentType);
            }
            return this.NotFound(Filename);
        }
    }
}