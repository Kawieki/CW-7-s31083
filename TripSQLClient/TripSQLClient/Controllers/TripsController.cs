using Microsoft.AspNetCore.Mvc;
using TripSQLClient.Services;

namespace TripSQLClient.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class TripsController(IDbService dbService) : ControllerBase
{

    // Endpoint zwracający wszystkie wycieczki
    [HttpGet]
    public async Task<IActionResult> GetAllTrips()
    {
        return Ok(await dbService.GetTripsDetailsAsync()); // Zwracamy wszystkie wycieczki z bazy
    }
}