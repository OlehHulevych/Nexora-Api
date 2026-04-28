namespace Nexora.Application.Users.Commands.Register;

public class GoogleRegisterUserCommand
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string LoginProviderSubject { get; set; } 
}