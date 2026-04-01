using DefectClassifier.Models;
using Microsoft.Data.Sqlite;

namespace DefectClassifier.Services;

public class DefectRepository
{
    private readonly string _connectionString;

    public DefectRepository(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Defects (
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                Type      INTEGER NOT NULL DEFAULT 0,
                X         REAL    NOT NULL DEFAULT 0,
                Y         REAL    NOT NULL DEFAULT 0,
                Width     REAL    NOT NULL DEFAULT 1,
                Height    REAL    NOT NULL DEFAULT 1,
                ImagePath TEXT    NOT NULL DEFAULT '',
                Timestamp TEXT    NOT NULL,
                WaferId   TEXT    NOT NULL DEFAULT '',
                Severity  INTEGER NOT NULL DEFAULT 1
            )
            """;
        cmd.ExecuteNonQuery();
    }

    public IReadOnlyList<DefectRecord> GetAll()
    {
        var list = new List<DefectRecord>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id,Type,X,Y,Width,Height,ImagePath,Timestamp,WaferId,Severity FROM Defects ORDER BY Timestamp DESC";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(ReadRecord(reader));
        return list;
    }

    public int Add(DefectRecord record)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Defects (Type,X,Y,Width,Height,ImagePath,Timestamp,WaferId,Severity)
            VALUES ($type,$x,$y,$w,$h,$img,$ts,$wid,$sev);
            SELECT last_insert_rowid();
            """;
        cmd.Parameters.AddWithValue("$type", (int)record.Type);
        cmd.Parameters.AddWithValue("$x", record.X);
        cmd.Parameters.AddWithValue("$y", record.Y);
        cmd.Parameters.AddWithValue("$w", record.Width);
        cmd.Parameters.AddWithValue("$h", record.Height);
        cmd.Parameters.AddWithValue("$img", record.ImagePath);
        cmd.Parameters.AddWithValue("$ts", record.Timestamp.ToString("O"));
        cmd.Parameters.AddWithValue("$wid", record.WaferId);
        cmd.Parameters.AddWithValue("$sev", record.Severity);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(DefectRecord record)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            UPDATE Defects SET Type=$type,X=$x,Y=$y,Width=$w,Height=$h,
            ImagePath=$img,Timestamp=$ts,WaferId=$wid,Severity=$sev
            WHERE Id=$id
            """;
        cmd.Parameters.AddWithValue("$id", record.Id);
        cmd.Parameters.AddWithValue("$type", (int)record.Type);
        cmd.Parameters.AddWithValue("$x", record.X);
        cmd.Parameters.AddWithValue("$y", record.Y);
        cmd.Parameters.AddWithValue("$w", record.Width);
        cmd.Parameters.AddWithValue("$h", record.Height);
        cmd.Parameters.AddWithValue("$img", record.ImagePath);
        cmd.Parameters.AddWithValue("$ts", record.Timestamp.ToString("O"));
        cmd.Parameters.AddWithValue("$wid", record.WaferId);
        cmd.Parameters.AddWithValue("$sev", record.Severity);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Defects WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public int Count()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Defects";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void SeedSampleData()
    {
        if (Count() > 0) return;

        var rng = new Random(42);
        var waferIds = new[] { "W001", "W002", "W003" };
        var types = Enum.GetValues<DefectType>().Where(t => t != DefectType.Unknown).ToArray();
        var baseTime = DateTime.Now.AddDays(-7);

        for (int i = 0; i < 50; i++)
        {
            var type = types[rng.Next(types.Length)];
            double x = Math.Round((rng.NextDouble() - 0.5) * 300, 2);
            double y = Math.Round((rng.NextDouble() - 0.5) * 300, 2);
            double w = Math.Round(rng.NextDouble() * 30 + 2, 2);
            double h = Math.Round(rng.NextDouble() * 30 + 2, 2);
            Add(new DefectRecord
            {
                Type = type,
                X = x,
                Y = y,
                Width = w,
                Height = h,
                ImagePath = string.Empty,
                Timestamp = baseTime.AddHours(i * 3.5),
                WaferId = waferIds[rng.Next(waferIds.Length)],
                Severity = rng.Next(1, 4)
            });
        }
    }

    private static DefectRecord ReadRecord(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Type = (DefectType)r.GetInt32(1),
        X = r.GetDouble(2),
        Y = r.GetDouble(3),
        Width = r.GetDouble(4),
        Height = r.GetDouble(5),
        ImagePath = r.GetString(6),
        Timestamp = DateTime.Parse(r.GetString(7)),
        WaferId = r.GetString(8),
        Severity = r.GetInt32(9)
    };
}
