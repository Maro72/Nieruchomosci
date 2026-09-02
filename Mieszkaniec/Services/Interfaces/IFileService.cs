using Microsoft.AspNetCore.Components.Forms;
using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Services.Interfaces
{
    public interface IFileService
    {
        // Upload pliku na serwer i zapis metadanych w DB
        Task<Zalacznik> UploadAsync(IBrowserFile file, int przegladId);

        // Fizyczne usuwanie pliku i rekordu z bazy
        Task<bool> DeleteAsync(int zalacznikId);

        Task<List<Zalacznik>> GetZalacznikiDlaPrzegladuAsync(int przegladId);
    }
}