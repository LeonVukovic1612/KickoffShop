using System.ComponentModel.DataAnnotations;

namespace KickoffShop.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ime je obavezno.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Email je obavezan.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Unesite ispravnu email adresu.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Lozinka je obavezna.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).{7,}$",
        ErrorMessage = "Lozinka mora imati najmanje 7 znakova, jedno veliko slovo i jedan broj.")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Potvrda lozinke je obavezna.")]
    [Compare("Password", ErrorMessage = "Lozinke se ne podudaraju.")]
    public string ConfirmPassword { get; set; } = "";
}
