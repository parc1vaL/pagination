﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Npgsql;

BenchmarkRunner.Run<Benchmarks>();

public class Benchmarks
{
    private NpgsqlConnection connection = null!;

    [Params(20, 900000)]
    public int Offset { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        this.connection = new NpgsqlConnection("Host=localhost;Username=postgres;Password=supersecret");
        await this.connection.OpenAsync();
    }

    [Benchmark]
    public async Task OffsetPagination()
    {
        using var command = new NpgsqlCommand($@"
SELECT b.""Id"", b.""LastUpdated""
FROM ""Blogs"" AS b
ORDER BY b.""Id""
LIMIT 10 OFFSET {Offset}", connection);

        await command.ExecuteNonQueryAsync();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        this.connection.Dispose();
    }
}