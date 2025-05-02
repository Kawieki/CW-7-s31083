using System.ComponentModel.DataAnnotations;

namespace TripSQLClient.Models.DTOs;


public class ClientCreateDTO
{
    [Length(1, 120)]
    public string FirstName { get; set; }
    
    [Length(1, 120)]
    public string LastName { get; set; }
    
    [Length(1, 120)]
    public string Email { get; set; }
    
    [Length(1, 120)]
    public string Telephone { get; set; }
    
    [Length(1, 120)]
    public string Pesel { get; set; }
}