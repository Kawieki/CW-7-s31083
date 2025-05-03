using Microsoft.AspNetCore.Mvc;
using TripSQLClient.Exceptions;
using TripSQLClient.Models.DTOs;
using TripSQLClient.Services;

namespace TripSQLClient.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ClientsController(IDbService dbService): ControllerBase
{
    
    // Endpoint zwracający wszystkie wycieczki, w których bierze udział klient
    [HttpGet("{id:int}/trips")]
    public async Task<IActionResult> GetClientTripsById([FromRoute] int id)
    {
        try
        {
            return Ok(await dbService.GetClientTripsDetailsByIdAsync(id)); // Zwracamy szczegóły wycieczek klienta
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message); // Jeśli klient nie istnieje lub nie ma zapisanych wycieczek
        }
    }

    // Endpoint tworzący nowego klienta w systemie
    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] ClientCreateDTO body)
    {
        var client = await dbService.CreateClientAsync(body); // Tworzymy klienta w bazie
        return Created($"/api/clients/{client.IdClient}", client.IdClient); // Zwracamy kod 201 i ID klienta
    }

    // Endpoint rejestrujący klienta na wycieczkę
    [HttpPut("{id:int}/trips/{tripId:int}")]
    public async Task<IActionResult> CreateClientTripById([FromRoute] int id, [FromRoute] int tripId)
    {
        try
        {
            var clientTrip = await dbService.CreateClientTripByIdAsync(id, tripId); // Tworzymy wpis w tabeli Client_Trip
            return Created($"/api/clients/{id}/trips/{tripId}", clientTrip); // Zwracamy kod 201 i dane klienta na wycieczce
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message); // Jeśli klient lub wycieczka nie istnieje
        }
        catch (MaxCapacityReachedException e)
        {
            return BadRequest(e.Message); // Jeśli przekroczono maksymalną liczbę uczestników
        }
    }
    
    // Endpoint usuwający zapis klienta na wycieczkę
    [HttpDelete("{id:int}/trips/{tripId:int}")]
    public async Task<IActionResult> DeleteClientTripById([FromRoute] int id, [FromRoute] int tripId)
    {
        try
        {
            await dbService.RemoveClientTripByIdAsync(id, tripId); // Usuwamy wpis z tabeli Client_Trip
            return NoContent(); // Zwracamy status 204, brak treści
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message); // Jeśli klient nie jest zapisany na wycieczke
        }
    }
    
}