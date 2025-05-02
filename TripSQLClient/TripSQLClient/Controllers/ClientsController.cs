using Microsoft.AspNetCore.Mvc;
using TripSQLClient.Exceptions;
using TripSQLClient.Models.DTOs;
using TripSQLClient.Services;

namespace TripSQLClient.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ClientsController(IDbService dbService): ControllerBase
{
    [HttpGet("{id:int}/trips")]
    public async Task<IActionResult> GetClientTripsById([FromRoute] int id)
    {
        try
        {
            return Ok(await dbService.GetClientTripsDetailsByIdAsync(id));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] ClientCreateDTO body)
    {
        var client = await dbService.CreateClientAsync(body);
        return Created($"/api/clients/{client.IdClient}", client.IdClient);
    }
    
}