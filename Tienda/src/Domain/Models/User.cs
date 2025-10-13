using Microsoft.AspNetCore.Identity;
using Tienda.src.Domain.Models;

namespace Tienda.src.Domain.Models
{
    public enum Gender
    {
        Masculino,
        Femenino,
        Otro
    }
    public class User : IdentityUser<int>
    {

        /// <summary>
        /// Identificador único del usuario chileno.
        /// </summary>
        public required string Rut { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public required string FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        public required string LastName { get; set; }

        /// <summary>
        /// Género del usuario.
        /// </summary>
        public required Gender Gender { get; set; }

        /// <summary>
        /// Fecha de nacimiento del usuario.
        /// </summary>
        public required DateTime BirthDate { get; set; }
    
        /// <summary>
        /// Fecha de registro del usuario.
        /// </summary>
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de actualización del usuario.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}