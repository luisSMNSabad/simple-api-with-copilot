using Microsoft.EntityFrameworkCore;
using SecureApp.Models;
using Dapper;
using SecureApp.Data;
using Microsoft.Data.SqlClient;

namespace SecureApp.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> CreateUserAsync(User user);
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
}

public class UserRepository : IUserRepository
{
    private readonly SecureAppDbContext _context;
    private readonly string _connectionString;

    public UserRepository(SecureAppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // Example using Entity Framework Core (parameterized automatically)  
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    // Example using Dapper with parameterized query  
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = "SELECT * FROM Users WHERE Email = @Email";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    // Example of safe search with LIKE  
    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"  
               SELECT * FROM Users   
               WHERE Username LIKE @Search   
               OR Email LIKE @Search";

        return await connection.QueryAsync<User>(
            sql,
            new { Search = $"%{searchTerm}%" }
        );
    }

    // Example of safe insert  
    public async Task<bool> CreateUserAsync(User user)
    {
        try
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }
}
