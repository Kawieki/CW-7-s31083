using System.Data;
using Microsoft.Data.SqlClient;
using TripSQLClient.Exceptions;
using TripSQLClient.Models;
using TripSQLClient.Models.DTOs;

namespace TripSQLClient.Services;

public interface IDbService
{
    public Task<IEnumerable<CountryTripGetDTO>> GetTripsDetailsAsync();
    public Task<IEnumerable<ClientTripDTO>> GetClientTripsDetailsByIdAsync(int id);
    public Task<Client> CreateClientAsync(ClientCreateDTO client);
    
}

public class DbService(IConfiguration config) : IDbService
{
    private readonly string? _connectionString = config.GetConnectionString("Default");
    
    public async Task<IEnumerable<CountryTripGetDTO>> GetTripsDetailsAsync()
    {
        var result = new List<CountryTripGetDTO>();
        
        await using var connection = new SqlConnection(_connectionString);
        const string sql = @"SELECT 
                            t.IdTrip, 
                            t.Name, 
                            t.Description, 
                            t.DateFrom, 
                            t.DateTo, 
                            t.MaxPeople,
                            c.Name AS CountryName
                        FROM Trip t
                        JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
                        JOIN Country c ON ct.IdCountry = c.IdCountry";
        
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new CountryTripGetDTO
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5),
                CountryName = reader.GetString(6)
            });
        }

        return result;
    }

    public async Task<IEnumerable<ClientTripDTO>> GetClientTripsDetailsByIdAsync(int id)
    {
        var result = new List<ClientTripDTO>();

        await using var connection = new SqlConnection(_connectionString);
        const string sql = @"SELECT
                        t.Name,
                        t.Description,
                        t.DateFrom,
                        t.DateTo,
                        t.MaxPeople,
                        ct.RegisteredAt,
                        ct.PaymentDate
                    FROM Trip t
                    JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
                    JOIN Client c ON c.IdClient = ct.IdClient
                    WHERE c.IdClient = @id";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        if (!reader.HasRows)
        {
            throw new NotFoundException($"Client with id: {id} does not exist or does not have trips");
        }
        

        while (await reader.ReadAsync())
        {
            result.Add(new ClientTripDTO
            {
                Name = reader.GetString(0),
                Description = reader.GetString(1),
                DateFrom = reader.GetDateTime(2),
                DateTo = reader.GetDateTime(3),
                MaxPeople = reader.GetInt32(4),
                RegisteredAt = reader.GetInt32(5),
                PaymentDate = reader.IsDBNull(6) ? null : reader.GetInt32(6)
            });
        }

        return result;
    }

    public async Task<Client> CreateClientAsync(ClientCreateDTO client)
    {
        await using var connection = new SqlConnection(_connectionString);
        const string sql = "insert into Client (FirstName, LastName, Email, Telephone, Pesel) values (@FirstName, @LastName, @Email, @Telephone, @Pesel); Select scope_identity()";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.Telephone);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);
        
        await connection.OpenAsync();
        var id = Convert.ToInt32(await command.ExecuteScalarAsync());

        return new Client
        {
            IdClient = id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Telephone = client.Telephone,
            Pesel = client.Pesel
        };
    }
    
    
}