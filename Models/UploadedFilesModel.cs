namespace BoilerplateWebApp.Models
{
    public class UploadedFilesModel
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
    }
}
