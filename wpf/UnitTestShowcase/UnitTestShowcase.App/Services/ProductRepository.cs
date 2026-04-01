using Microsoft.Data.Sqlite;
using UnitTestShowcase.App.Models;

namespace UnitTestShowcase.App.Services;

public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Categories (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT NOT NULL DEFAULT ''
            );
            CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Price REAL NOT NULL,
                Stock INTEGER NOT NULL DEFAULT 0,
                CategoryId INTEGER NOT NULL,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
            );
            INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (1, 'Electronics', 'Electronic devices');
            INSERT OR IGNORE INTO Categories (Id, Name, Description) VALUES (2, 'Books', 'Books and magazines');
            """;
        cmd.ExecuteNonQuery();
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        var products = new List<Product>();
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT p.Id, p.Name, p.Price, p.Stock, p.CategoryId, c.Name, c.Description
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            """;
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = (decimal)reader.GetDouble(2),
                Stock = reader.GetInt32(3),
                CategoryId = reader.GetInt32(4),
                Category = new Category
                {
                    Id = reader.GetInt32(4),
                    Name = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Description = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                }
            });
        }
        return products;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT p.Id, p.Name, p.Price, p.Stock, p.CategoryId, c.Name, c.Description
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            WHERE p.Id = $id
            """;
        cmd.Parameters.AddWithValue("$id", id);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = (decimal)reader.GetDouble(2),
                Stock = reader.GetInt32(3),
                CategoryId = reader.GetInt32(4),
                Category = new Category
                {
                    Id = reader.GetInt32(4),
                    Name = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Description = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                }
            };
        }
        return null;
    }

    public async Task<Product> AddAsync(Product product)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Products (Name, Price, Stock, CategoryId)
            VALUES ($name, $price, $stock, $categoryId);
            SELECT last_insert_rowid();
            """;
        cmd.Parameters.AddWithValue("$name", product.Name);
        cmd.Parameters.AddWithValue("$price", (double)product.Price);
        cmd.Parameters.AddWithValue("$stock", product.Stock);
        cmd.Parameters.AddWithValue("$categoryId", product.CategoryId);
        var id = (long)(await cmd.ExecuteScalarAsync())!;
        product.Id = (int)id;
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE Products SET Name = $name, Price = $price, Stock = $stock, CategoryId = $categoryId
            WHERE Id = $id
            """;
        cmd.Parameters.AddWithValue("$name", product.Name);
        cmd.Parameters.AddWithValue("$price", (double)product.Price);
        cmd.Parameters.AddWithValue("$stock", product.Stock);
        cmd.Parameters.AddWithValue("$categoryId", product.CategoryId);
        cmd.Parameters.AddWithValue("$id", product.Id);
        await cmd.ExecuteNonQueryAsync();
        return product;
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Products WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        await cmd.ExecuteNonQueryAsync();
    }
}
