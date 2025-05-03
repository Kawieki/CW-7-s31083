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
    public Task<ClientTrip> CreateClientTripByIdAsync(int idClient, int tripId);
    public Task RemoveClientTripByIdAsync(int id, int tripId);
}

public class DbService(IConfiguration config) : IDbService
{
    private readonly string? _connectionString = config.GetConnectionString("Default");
    
    public async Task<IEnumerable<CountryTripGetDTO>> GetTripsDetailsAsync()
    {
        var result = new List<CountryTripGetDTO>();
        
        // Zapytanie SQL pobierające wszystkie wycieczki z informacjami o kraju, nazwie i maksymalnej liczbie uczestników
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

        return result; // Zwracamy listę wszystkich wycieczek
    }
    
    public async Task<IEnumerable<ClientTripDTO>> GetClientTripsDetailsByIdAsync(int id)
    {
        var result = new List<ClientTripDTO>();
        await using var connection = new SqlConnection(_connectionString);
        
        // Zapytanie SQL pobierające wszystkie wycieczki powiązane z danym klientem.
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

        return result; // zwracamy listę wycieczek dla danego klienta
    }

    public async Task<Client> CreateClientAsync(ClientCreateDTO client)
    {
        await using var connection = new SqlConnection(_connectionString);
        
        //Zapytanie SQL tworzące nowego klienta
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
        }; // zwracamy nowo utworzonego klienta
    }
    
    private async Task<bool> ClientExistsAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        //Zapytanie SQL ktore sprawdza czy dany klient istnieje w bazie danych
        const string sql = "SELECT 1 FROM Client WHERE IdClient = @id";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return result != null; //zwracamy wartosc logiczna ktora informuje o istnieniu klienta (true), lub nie istnieniu (false)
    }

    private async Task<bool> TripExistsAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        //Zapytanie SQL ktore sprawdza czy dana wycieczka istnieje w bazie danych
        const string sql = "SELECT 1 FROM Trip WHERE IdTrip = @id";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return result != null; //zwracamy wartosc logiczna ktora informuje o istnieniu wycieczki (true), lub nie istnieniu (false)
    }

    private async Task<bool> CheckTripLimitAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        
        //Zapytanie SQL ktore sprawdza czy na dana wycieczke jest możlwiość przypisana klienta (czy limit osób nie bedzie przekroczony)
        const string sql = @"SELECT COUNT(ct.IdTrip) 
                             FROM Client_Trip ct 
                             INNER JOIN Trip t ON ct.IdTrip = t.IdTrip
                             WHERE t.IdTrip = @id
                             GROUP BY t.MaxPeople
                             HAVING COUNT(ct.IdTrip) < t.MaxPeople";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        return result != null;  //zwracamy wartosc logiczna ktora informuje o nieprzekroczeniu limitu osob (true), lub przekroczeniu (false)
    }

    public async Task<ClientTrip> CreateClientTripByIdAsync(int idClient, int idTrip)
    {
        if (!await ClientExistsAsync(idClient))
            throw new NotFoundException($"Client with id: {idClient} does not exist");

        if (!await TripExistsAsync(idTrip))
            throw new NotFoundException($"Trip with id: {idTrip} does not exist");

        if (!await CheckTripLimitAsync(idTrip))
            throw new MaxCapacityReachedException("Trip has reached maximum capacity");

        await using var connection = new SqlConnection(_connectionString);
        // Zapytanie SQL do zapisania klienta na wycieczkę
        const string sql = @"
        INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
        VALUES (@IdClient, @IdTrip, @RegisteredAt, @PaymentDate);";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdClient", idClient);
        command.Parameters.AddWithValue("@IdTrip", idTrip);
        command.Parameters.AddWithValue("@RegisteredAt", int.Parse(DateTime.UtcNow.ToString("yyyyMMdd")));
        command.Parameters.AddWithValue("@PaymentDate", DBNull.Value); 

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();

        return new ClientTrip
        {
            ClientId = idClient,
            TripId = idTrip,
            RegisteredAt = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd")),
            PaymentDate = null
        }; //zwracamy nowo przypisana wycieczke dla klienta
    }

    public async Task RemoveClientTripByIdAsync(int id, int idTrip)
    {
        await using var connection = new SqlConnection(_connectionString);
        //Zapytanie SQL które usuwa podana wycieczke podanemu klientowi
        const string sql = "delete from Client_Trip where IdClient = @id and IdTrip = @idTrip";
        await using var command2 = new SqlCommand(sql, connection);
        command2.Parameters.AddWithValue("@id", id);
        command2.Parameters.AddWithValue("@idTrip", idTrip);
        await connection.OpenAsync();
        var numOfRows = await command2.ExecuteNonQueryAsync();

        if (numOfRows == 0)
        {
            throw new NotFoundException($"Client with id: {id} does not have trip: {idTrip}");
        }
    }
}