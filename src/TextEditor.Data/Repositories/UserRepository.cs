using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Core.Models;
using TextEditor.Core.Repositories;

namespace TextEditor.Data.Repositories;

// TODO: Sprint 1 — Dev E (в паре с Dev B).
public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> GetByLoginAsync(string login, CancellationToken ct = default)
        => throw new NotImplementedException("Ольга");

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotImplementedException("Ольга");

    public Task<User> AddAsync(User user, CancellationToken ct = default)
        => throw new NotImplementedException("Ольга");
}