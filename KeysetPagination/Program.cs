﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MR.EntityFrameworkCore.KeysetPagination;

using var ctx = new BloggingContext();
const int pageSize = 10;
const KeysetPaginationDirection direction = KeysetPaginationDirection.Forward;

// -----------
// Erste Seite
// -----------
var keysetPaginationContext = ctx.Blogs
    .KeysetPaginate(
        blog => blog.Ascending(entity => entity.LastUpdated).Ascending(entity => entity.Id),
        direction);

var blogs = await keysetPaginationContext.Query
    .Take(pageSize)
    .ToListAsync();

// Alternativ:
// blogs = await ctx.Blogs
//     .KeysetPaginateQuery(
//         blog => blog.Ascending(entity => entity.LastUpdated).Ascending(entity => entity.Id),
//         direction)
//     .Take(pageSize)
//     .ToListAsync();

// Äquivalent zu: if (direction == KeysetPaginationDirection.Backward) { blogs.Reverse(); }
// keysetPaginationContext.EnsureCorrectOrder(blogs);

// -----------
// Zweite Seite
// -----------
var reference =
    direction == KeysetPaginationDirection.Forward
        ? blogs[blogs.Count - 1]
        : blogs[0];

keysetPaginationContext = ctx.Blogs
    .KeysetPaginate(
        blog => blog.Ascending(entity => entity.LastUpdated).Ascending(entity => entity.Id),
        direction,
        reference);

blogs = await keysetPaginationContext.Query
    .Take(pageSize)
    .ToListAsync();

// s.o.
// keysetPaginationContext.EnsureCorrectOrder(blogs);

// Gibt es noch eine weitere Seite?
// var hasNext = await keysetPaginationContext.HasNextAsync(blogs);

public class BloggingContext : DbContext
{
    public DbSet<Blog> Blogs => Set<Blog>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder
            .UseNpgsql("Host=localhost;Username=postgres;Password=supersecret")
            .LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted, })
            .EnableSensitiveDataLogging();
}

public class Blog 
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly LastUpdated { get; set; }
}