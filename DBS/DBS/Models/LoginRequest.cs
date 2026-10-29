using System.ComponentModel.DataAnnotations;

namespace DBS.Models;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(1)]
    public string Senha { get; set; } = "";
}
