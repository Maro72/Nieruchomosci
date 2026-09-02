using System.Collections.Generic;
using System.Threading.Tasks;
using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Services.Interfaces
{
    public interface IUzytkownikService
    {
        Task<List<Uzytkownik>> PobierzUzytkownikowAsync();
        Task<List<Rola>> PobierzRoleAsync();
        Task<List<Uprawnienie>> PobierzUprawnieniaAsync();

        Task ZapiszUzytkownikaAsync(
            Uzytkownik uzytkownik,
            IEnumerable<Rola> wybraneRole,
            IEnumerable<Uprawnienie> wybraneUprawnienia);

        Task UsunUzytkownikaAsync(int id);
    }
}