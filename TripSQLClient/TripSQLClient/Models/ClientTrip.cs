namespace TripSQLClient.Models;

public class ClientTrip
{
    public int ClientId { get; set; }
    public int TripId { get; set; }
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}