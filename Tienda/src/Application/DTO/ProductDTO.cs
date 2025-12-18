using Tienda.src.Domain.Models;

namespace Tienda.src.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required int Price { get; set; }
        public int Discount { get; set; }
        public required int Stock { get; set; }
        public required Status Status { get; set; }
        public bool IsAvailable { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
    }
}