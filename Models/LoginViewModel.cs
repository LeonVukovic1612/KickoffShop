using System.ComponentModel.DataAnnotations;

namespace KickoffShop.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email je obavezan.")]
    [EmailAddress(ErrorMessage = "Neispravan format emaila.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Lozinka je obavezna.")]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}
