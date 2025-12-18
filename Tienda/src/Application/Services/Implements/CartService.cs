using Tienda.src.Application.DTO.CartDTO;
using Tienda.src.Application.Services.Interfaces;

namespace Tienda.src.Application.Services.Implements
{
    /// <summary>
    /// Implementación temporal del servicio de carrito.
    /// </summary>
    public class CartService : ICartService
    {
        public Task<CartDTO> AddItemAsync(string buyerId, int productId, int quantity, int? userId = null)
        {
            throw new NotImplementedException();
        }

        public Task AssociateWithUserAsync(string buyerId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<CartDTO> CheckoutAsync(string buyerId, int? userId)
        {
            throw new NotImplementedException();
        }

        public Task<CartDTO> ClearAsync(string buyerId, int? userId = null)
        {
            throw new NotImplementedException();
        }

        public Task<CartDTO> CreateOrGetAsync(string buyerId, int? userId = null)
        {
            throw new NotImplementedException();
        }

        public Task<CartDTO> RemoveItemAsync(string buyerId, int productId, int? userId = null)
        {
            throw new NotImplementedException();
        }

        public Task<CartDTO> UpdateItemQuantityAsync(string buyerId, int productId, int quantity, int? userId = null)
        {
            throw new NotImplementedException();
        }
    }
}