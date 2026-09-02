using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;

namespace Mieszkaniec.Services.Implementations
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IDbContextFactory<MieszkaniecDbContext> _dbFactory;
        private const long MaxFileSize = 1024 * 1024 * 10; // Limit 10MB

        public FileService(IWebHostEnvironment env, IDbContextFactory<MieszkaniecDbContext> dbFactory)
        {
            _env = env;
            _dbFactory = dbFactory;
        }

        public async Task<Zalacznik> UploadAsync(IBrowserFile file, int przegladId)
        {
            // 1. Przygotowanie ścieżki (wwwroot/uploads/przeglad_{id})
            var folderName = Path.Combine("uploads", $"przeglad_{przegladId}");
            var pathToSave = Path.Combine(_env.WebRootPath, folderName);

            if (!Directory.Exists(pathToSave))
                Directory.CreateDirectory(pathToSave);

            // 2. Generowanie unikalnej nazwy pliku (aby uniknąć nadpisywania)
            var trustedFileName = Path.GetRandomFileName(); // lub Guid.NewGuid().ToString()
            var extension = Path.GetExtension(file.Name);
            var finalFileName = $"{trustedFileName}{extension}";
            var fullPath = Path.Combine(pathToSave, finalFileName);

            // 3. Fizyczny zapis pliku na dysku
            await using FileStream fs = new(fullPath, FileMode.Create);
            await file.OpenReadStream(MaxFileSize).CopyToAsync(fs);

            // 4. Zapis metadanych do bazy MySQL
            var zalacznik = new Zalacznik
            {
                PrzegladId = przegladId,
                NazwaPliku = file.Name, // Oryginalna nazwa dla użytkownika
                SciezkaMagazyn = Path.Combine(folderName, finalFileName), // Relatywna ścieżka do bazy
                RozmiarKB = (int)(file.Size / 1024),
                DataDodania = DateTime.Now
            };

            using var context = _dbFactory.CreateDbContext();
            context.Zalaczniki.Add(zalacznik);
            await context.SaveChangesAsync();

            return zalacznik;
        }

        public async Task<bool> DeleteAsync(int zalacznikId)
        {
            using var context = _dbFactory.CreateDbContext();
            var zalacznik = await context.Zalaczniki.FindAsync(zalacznikId);

            if (zalacznik == null) return false;

            // 1. Usunięcie fizyczne z dysku
            var fullPath = Path.Combine(_env.WebRootPath, zalacznik.SciezkaMagazyn);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            // 2. Usunięcie rekordu z bazy
            context.Zalaczniki.Remove(zalacznik);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<List<Zalacznik>> GetZalacznikiDlaPrzegladuAsync(int przegladId)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Zalaczniki
                .Where(z => z.PrzegladId == przegladId)
                .ToListAsync();
        }
    }
}