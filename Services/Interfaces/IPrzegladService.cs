using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Services.Interfaces
{
    public interface IPrzegladService
    {
        Task<List<Przeglad>> GetAllWithDetailsAsync();
        Task SaveAsync(Przeglad model);
        Task DeleteAsync(int id);
        Task ZrealizujIArchiwizujAsync(Przeglad zrealizowanyModel);
    }
}