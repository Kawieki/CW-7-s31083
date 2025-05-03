using System.ComponentModel.DataAnnotations;

namespace TripSQLClient.Models.DTOs;


public class ClientCreateDTO
{
    [Length(1, 120)]
    public string FirstName { get; set; }
    
    [Length(1, 120)]
    public string LastName { get; set; }
    
    [Length(1, 120)]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Nieprawidłowy adres e-mail.")]
    public string Email { get; set; }
    
    // mogłbym dodać walidacje również do tego ale nie jestem pewien czy chodzi o polskie numery, w zadaniu jest ze atrybut przyjmuje nvarchar(120)
    [Length(1, 120)]
    public string Telephone { get; set; }
    
    // dodałem walidacje do tego ponieważ pesel składa sie z 11 cyfr, mimo że w zadaniu jest ze atrybut przyjmuje nvarchar(120)
    [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL musi składać się z dokładnie 11 cyfr.")]
    public string Pesel { get; set; }
}