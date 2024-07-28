using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using BoilerplateWebApp.Data;
using BoilerplateWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BoilerplateWebApp.Pages
{
    [Authorize]
    public class FileUploadModel(ApplicationDbContext context, IConfiguration config) : PageModel
    {
        private static AmazonS3Client? S3Client;
        private readonly string UploadedFileBucketName = config["BucketName"]!;
        private readonly string SecretKey = config["SecretKey"]!;
        private readonly string AccessKey = config["AccessKey"]!;
        private readonly string AccountID = config["AccountID"]!;
        public List<UploadedFilesModel> UploadedFilesList = [];

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id.HasValue)
            {
                return await DownloadFile(id.Value);
            }

            // List file
            UploadedFilesList = await context.UploadedFiles.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            InitializeS3Client();

            var uploadedFiles = new List<string>();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    await Console.Out.WriteLineAsync(formFile.FileName);

                    // Upload ke R2 Cloudflare
                    var fileName = Path.GetFileName(formFile.FileName);
                    var fileExtension = Path.GetExtension(fileName);

                    // Membuat unique filename untuk mencegah overwrite file dengan nama yang sama
                    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                    using var memoryStream = new MemoryStream();
                    await formFile.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    var putRequest = new PutObjectRequest
                    {
                        BucketName = UploadedFileBucketName,
                        Key = uniqueFileName,
                        InputStream = memoryStream,
                        ContentType = formFile.ContentType,
                        DisablePayloadSigning = true
                    };

                    try
                    {
                        var response = await S3Client!.PutObjectAsync(putRequest);
                        uploadedFiles.Add(uniqueFileName);

                        var uploadedFile = new UploadedFilesModel
                        {
                            Id = Guid.NewGuid(),
                            FileName = formFile.FileName,
                            FileUrl = uniqueFileName,
                        };

                        context.UploadedFiles.Add(uploadedFile);
                        await context.SaveChangesAsync();

                        Console.WriteLine($"File uploaded successfully: {uniqueFileName}");
                    }
                    catch (AmazonS3Exception ex)
                    {
                        Console.WriteLine($"Error uploading file {uniqueFileName}: {ex.Message}");
                    }

                }
            }

            return Redirect("/FileUpload");
        }

        private void InitializeS3Client()
        {
            var accessKey = AccessKey;
            var secretKey = SecretKey;
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            S3Client = new AmazonS3Client(credentials, new AmazonS3Config
            {
                ServiceURL = $"https://{AccountID}.r2.cloudflarestorage.com",
            });
        }

        private async Task<IActionResult> DownloadFile(Guid id)
        {
            var file = await context.UploadedFiles.FindAsync(id);
            if (file == null)
            {
                return NotFound($"File with ID {id} not found.");
            }

            InitializeS3Client();

            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = UploadedFileBucketName,
                    Key = file.FileUrl // Assuming FileUrl contains the S3 object key
                };

                using var response = await S3Client!.GetObjectAsync(request);
                using var responseStream = response.ResponseStream;
                using var memoryStream = new MemoryStream();
                await responseStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                return File(memoryStream.ToArray(), response.Headers.ContentType, file.FileName);
            }
            catch (AmazonS3Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                return RedirectToPage();
            }
        }
    }
}
