using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLineProject.Models;
using BaseLineProject.Data;
namespace BaseLineProject.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
        Task SendWelcomeEmailAsync(WelcomeRequest request);
        Task SendVerifyEmailAsync(WelcomeRequest request);
        Task SendWelcomePaymentAsync(WelcomeRequest request);

    }
}
