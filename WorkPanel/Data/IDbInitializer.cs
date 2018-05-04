using System.Threading.Tasks;

namespace WorkPanel.Data
{
    public interface IDbInitializer
    {
        Task Initialize();
    }
}
