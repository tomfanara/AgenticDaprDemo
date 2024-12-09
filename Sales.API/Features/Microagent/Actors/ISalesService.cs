using Sales.API.Models.Response;

namespace Sales.API.Features.Microagent.Actors
{
    public interface ISalesService
    {
        public Task<Chat> GetSales(string prompt);
    }
}
