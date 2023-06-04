using Domain.Entities;

namespace Application.Interfaces.Utility;

public interface IEmailSender
{
    public Task SendAccountConfirmationEmail(User user);
}