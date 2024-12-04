using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;
using ContosoSuitesWebAPI.Entities;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ContosoSuitesWebAPI.Services;

/// <summary>
/// The database service for querying the Contoso Suites database.
/// </summary>
public class DatabaseService : IDatabaseService
{
    [KernelFunction]
    [Description("Get all bookings missing hotel rooms.")]
    public async Task<IEnumerable<Booking>> GetBookingsMissingHotelRooms()
    {
        var sql = """
            SELECT
                b.BookingID,
                b.CustomerID,
                b.HotelID,
                b.StayBeginDate,
                b.StayEndDate,
                b.NumberOfGuests
            FROM dbo.Booking b
            WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM dbo.BookingHotelRoom h
                    WHERE
                        b.BookingID = h.BookingID
                );
            """;
        using var conn = new SqlConnection(
            connectionString: Environment.GetEnvironmentVariable("SQLAZURECONNSTR_ContosoSuites")!
        );
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        var bookings = new List<Booking>();
        while (await reader.ReadAsync())
        {
            bookings.Add(new Booking
            {
                BookingID = reader.GetInt32(0),
                CustomerID = reader.GetInt32(1),
                HotelID = reader.GetInt32(2),
                StayBeginDate = reader.GetDateTime(3),
                StayEndDate = reader.GetDateTime(4),
                NumberOfGuests = reader.GetInt32(5)
            });
        }
        conn.Close();

        return bookings;
    }

    [KernelFunction]
    [Description("Get all bookings with multiple hotel rooms.")]
    public async Task<IEnumerable<Booking>> GetBookingsWithMultipleHotelRooms()
    {
        var sql = """
            SELECT
                b.BookingID,
                b.CustomerID,
                b.HotelID,
                b.StayBeginDate,
                b.StayEndDate,
                b.NumberOfGuests
            FROM dbo.Booking b
            WHERE
                (
                    SELECT COUNT(1)
                    FROM dbo.BookingHotelRoom h
                    WHERE
                        b.BookingID = h.BookingID
                ) > 1;
            """;
        using var conn = new SqlConnection(
            connectionString: Environment.GetEnvironmentVariable("SQLAZURECONNSTR_ContosoSuites")!
        );
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        var bookings = new List<Booking>();
        while (await reader.ReadAsync())
        {
            bookings.Add(new Booking
            {
                BookingID = reader.GetInt32(0),
                CustomerID = reader.GetInt32(1),
                HotelID = reader.GetInt32(2),
                StayBeginDate = reader.GetDateTime(3),
                StayEndDate = reader.GetDateTime(4),
                NumberOfGuests = reader.GetInt32(5)
            });
        }
        conn.Close();

        return bookings;
    }

    [KernelFunction]
    [Description("Get bookings for a specific hotel after a specified date.")]
    public async Task<IEnumerable<Booking>> GetBookingsByHotelAndMinimumDate(
        [Description("The ID of the hotel")] int hotelId, 
        [Description("The minimum date for bookings")] DateTime dt)
    {
        var sql = "SELECT BookingID, CustomerID, HotelID, StayBeginDate, StayEndDate, NumberOfGuests FROM dbo.Booking WHERE HotelID = @HotelID AND StayBeginDate >= @StayBeginDate";
        using var conn = new SqlConnection(
            connectionString: Environment.GetEnvironmentVariable("SQLAZURECONNSTR_ContosoSuites")!
        );
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@HotelID", hotelId);
        cmd.Parameters.AddWithValue("@StayBeginDate", dt);
        using var reader = await cmd.ExecuteReaderAsync();
        var bookings = new List<Booking>();
        while (await reader.ReadAsync())
        {
            bookings.Add(new Booking
            {
                BookingID = reader.GetInt32(0),
                CustomerID = reader.GetInt32(1),
                HotelID = reader.GetInt32(2),
                StayBeginDate = reader.GetDateTime(3),
                StayEndDate = reader.GetDateTime(4),
                NumberOfGuests = reader.GetInt32(5)
            });
        }
        conn.Close();

        return bookings;
    }
}
