namespace Tienda.src.Domain.Models
{
    public class Discount
    {
        public int Id { get; set; }
        public required string Code { get; set; }
        public int Percentage { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}