using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace TextEditor.Data.Entities;

public sealed class UserEntity
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;

    public UserSettingsEntity? Settings { get; set; }
    public ICollection<DocumentRegistryEntity> Documents { get; set; } = new List<DocumentRegistryEntity>();
}
