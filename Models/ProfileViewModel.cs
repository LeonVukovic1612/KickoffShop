using System.ComponentModel.DataAnnotations;

namespace KickoffShop.Models;

public class ProfileViewModel
{
    [Required(ErrorMessage = "Ime i prezime je obavezno.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Email je obavezan.")]
    [EmailAddress(ErrorMessage = "Unesite ispravnu email adresu.")]
    public string Email { get; set; } = "";

    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public bool Saved { get; set; }
    public List<WishlistItem> Wishlist { get; set; } = [];
}
