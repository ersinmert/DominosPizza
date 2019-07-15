using Dominos.Common.Classes;
using Dominos.Common.DTO.Output;
using Dominos.Data.Models;
using Dominos.Data.Repository;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Dominos.Common.Constants;

namespace Dominos.Business.ProductService
{
    public class ProductService : IProductService
    {
        public ProductService(
            IRepository<Product> productRepository,
            IRepository<ProductType> productTypeRepository,
            IMemoryCache cache)
        {
            _productRepository = productRepository;
            _productTypeRepository = productTypeRepository;
            _cache = cache;
        }

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductType> _productTypeRepository;
        private readonly IMemoryCache _cache;

        public async Task<ResponseEntity<List<ProductOutputDTO>>> GetPizzas()
        {
            var response = new ResponseEntity<List<ProductOutputDTO>>();
            try
            {
                var products = await GetProductsFromCache();
                response.Result = products.Where(x => x.Type == "Pizza").ToList();
            }
            catch (Exception ex)
            {
                response.Exception = ex;
                response.ErrorMessage = ex.Message;
                response.HttpCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        private async Task<List<ProductOutputDTO>> GetProductsFromCache()
        {
            var products = _cache.Get<List<ProductOutputDTO>>(CacheKey.Products);
            if (products == null || !products.Any())
            {
                products = await (from p in _productRepository.Table
                                  join pt in _productTypeRepository.Table on p.ProductTypeId equals pt.Id
                                  where p.IsActive
                                  select new ProductOutputDTO
                                  {
                                      ProductId = p.Id,
                                      ProductName = p.ProductName,
                                      Description = p.Description,
                                      Price = p.Price,
                                      ImagePath = p.ImagePath,
                                      TypeId = pt.Id,
                                      Type = pt.Name
                                  }).AsNoTracking().ToListAsync();

                if (products != null && products.Any())
                {
                    _cache.Set(CacheKey.Products, products);
                }
            }
            return products.Select(x => x.Clone()).ToList();
        }
    }
}
