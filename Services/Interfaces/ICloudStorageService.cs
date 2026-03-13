namespace ApiBackend.Services.Interfaces
{
    public interface ICloudStorageService
    {
        /// <summary>
        /// Fotoğrafı Azure Blob Storage'a yükler, ham blob URL döner.
        /// </summary>
        Task<string> UploadImageAsync(IFormFile image);

        /// <summary>
        /// Ham blob URL'inden 24 saatlik SAS URL üretir.
        /// </summary>
        string GenerateSasUrl(string blobUrl, TimeSpan expiry);
    }
}
