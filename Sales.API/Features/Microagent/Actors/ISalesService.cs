using Sales.API.Models.Response;

namespace Sales.API.Features.Microagent.Actors
{
    public interface ISalesService
    {
        public Chat GetSales(string prompt);
    }
}
