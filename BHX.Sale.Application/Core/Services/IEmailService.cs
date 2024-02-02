using BHX.Sale.Application.Core.Models;

namespace BHX.Sale.Application.Core.Services
{
    public interface IEmailService
    {
        void SendEmail(Email email);
    }
}