using System.ComponentModel.DataAnnotations;

namespace Orbit.Core.Utils.DataAnnotations;

/// <summary>
/// Specifies that an object has properties with <see cref="ValidationAttribute"/> which can
/// be validated by <see cref="Validator"/>.
/// </summary>
public interface IHasValidationAttributes
{
}
