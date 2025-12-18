using Tienda.src.Domain.Models;
using Tienda.src.Interfaces;

namespace Tienda.src.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public Task<Product> CreateAsync(Product product)
        {
            return _productRepository.CreateAsync(product);
        }

        public Task DeleteAsync(int id)
        {
            return _productRepository.DeleteAsync(id);
        }

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            return _productRepository.GetAllAsync();
        }

        public Task<Product?> GetByIdAsync(int id)
        {
            return _productRepository.GetByIdAsync(id);
        }

        public Task<Product> UpdateAsync(Product product)
        {
            return _productRepository.UpdateAsync(product);
        }
    }
}