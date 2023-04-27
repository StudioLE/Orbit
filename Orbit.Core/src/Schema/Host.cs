using System.ComponentModel.DataAnnotations;
using Orbit.Core.Providers;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core.Schema;

public sealed class Host : IEntity, IHasValidationAttributes
{
    private const string DefaultName = "host";
    private const int DefaultNumber = 1;

    [NameSchema]
    public string Name { get; set; } = string.Empty;

    [Range(1,64)]
    public int Number { get; set; }

    [Required]
    public string Address { get; set; } = string.Empty;

    public SshConnection Ssh { get; set; } = new();

    public void Review(EntityProvider provider)
    {
        if (!Name.IsNullOrEmpty())
        {
            Host? host = provider.Host.Get(Name);
            if (host is not null)
                Number = host.Number;
        }

        if (Number == default)
        {
            // TODO: Change to GetAllClusterIds
            int[] numbers = provider
                .Host
                .GetAll()
                .Select(x => x.Number)
                .ToArray();
            int finalNumber = numbers.Any()
                    ? numbers.Max()
                    : DefaultNumber - 1;
            Number = finalNumber  + 1;
        }

        // TODO: GetName from host file!
        if (Name.IsNullOrEmpty())
            Name = $"{DefaultName}-{Number:00}";
    }
}
