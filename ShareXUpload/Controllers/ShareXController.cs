using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using ShareXUpload.Models;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShareXUpload.Controllers {

    [ApiController]
    public class ShareXController : ControllerBase {
        private readonly IConfiguration Configuration;

        public ShareXController(IConfiguration configuration) {
            this.Configuration = configuration;
        }

        [HttpPost, DisableRequestSizeLimit]
        [Route("/Upload")]
        public async Task<string> Put([FromForm] string Secret) {
            if (Secret == this.Configuration["secretKey"] && this.Request.Form.Files.Count > 0) {
                IFormFile RetrievedFile = this.Request.Form.Files[0];
                string FileName = RandomString(8);
                string FileType = RetrievedFile.FileName[(RetrievedFile.FileName.IndexOf('.') + 1)..];

                await SaveFileAsync(RetrievedFile, FileName, FileType).ConfigureAwait(false);

                this.Response.StatusCode = 202;
                return JsonSerializer.Serialize(new ShareXOutput("OK", null, $"{FileName}.{FileType}"));
            }

            this.Response.StatusCode = 403;
            return JsonSerializer.Serialize(new ShareXOutput("ERROR", "Invalid Auth or File"));

            static async Task SaveFileAsync(IFormFile retrievedFile, string fileName, string fileType) {
                await using System.IO.Stream FileUpload = System.IO.File.Create("./Files/" + fileName + $".{fileType}");
                await retrievedFile.CopyToAsync(FileUpload).ConfigureAwait(false);
            }

            static string RandomString(int length) {
                const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
                return new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[new Random().Next(s.Length)]).ToArray());
            }
        }

        [HttpGet]
        [Route("/Files/{FileName}")]
        public async Task<IActionResult> Get(string filename) {
            if (!System.IO.File.Exists("./Files/" + filename)) return this.NotFound();
            {
                string ContentType = GetContentType(filename);
                byte[] FileBytes = await System.IO.File.ReadAllBytesAsync("./Files/" + filename).ConfigureAwait(false);

                return this.File(FileBytes, ContentType);
            }

            static string GetContentType(string filename) {
                FileExtensionContentTypeProvider ContentTypeProvider = new();
                ContentTypeProvider.TryGetContentType("./Files/" + filename, out string ContentType);
                return ContentType;
            }
        }

        [HttpGet]
        [Route("/Admin/Delete/{FileName}")]
        public IActionResult GetAdmin(string fileName, string secretKey) {
            if (!System.IO.File.Exists("./Files/" + fileName)) return this.NotFound();
            else if (secretKey != this.Configuration["secretKey"]) return this.Unauthorized();
            {
                System.IO.File.Delete("./Files/" + fileName);
                return this.Ok();
            }
        }
    }
}