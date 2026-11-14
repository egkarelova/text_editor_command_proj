using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Core.Models;

namespace TextEditor.Core.Repositories;

public interface IUserRepository
{
    Task<User?> GetByLoginAsync(string login, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User> AddAsync(User user, CancellationToken ct = default);
    Task<User?> GetGuestAsync(CancellationToken ct = default);
}