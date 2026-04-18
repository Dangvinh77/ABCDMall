namespace ABCDMall.Modules.Users.Application.DTOs.Auth;

public sealed class UserAccountMutationResponseDto
{
    public string Message { get; set; } = string.Empty;

    public bool EmailSent { get; set; }
}
