using Dominos.Business.OrderService;
using Dominos.Common.Classes;
using Dominos.Common.DTO.Input;
using Dominos.Common.DTO.Output;
using Dominos.Common.Helpers;
using Dominos.Data.Models;
using Dominos.Data.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Dominos.Business.BasketService
{
    public class BasketService : IBasketService
    {
        public BasketService(IRepository<Basket> basketRepository,
            IRepository<BasketDetail> basketDetailRepository,
            IRepository<Product> productRepository,
            IOrderService orderService)
        {
            _basketRepository = basketRepository;
            _basketDetailRepository = basketDetailRepository;
            _productRepository = productRepository;
            _orderService = orderService;
        }

        private readonly IRepository<Basket> _basketRepository;
        private readonly IRepository<BasketDetail> _basketDetailRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IOrderService _orderService;

        public async Task<ResponseEntity<BasketOutputDTO>> GetCustomerBasket(int? customerId, string basketKey)
        {
            var response = new ResponseEntity<BasketOutputDTO>();
            try
            {
                var basketDetails = await GetBasketDetails(customerId, basketKey);
                if (basketDetails == null || !basketDetails.Any())
                {
                    return response;
                }

                var productsPrice = basketDetails.Sum(x => x.Price * x.Quantity);
                var discountAmount = default(double);
                if (customerId != null)
                {
                    var customerOrders = await _orderService.GetCustomerOrderList(customerId.Value);
                    discountAmount = customerOrders?.Result?.Any() == true ? discountAmount : productsPrice * 5 / 100;
                    discountAmount = Math.Round(discountAmount, 2);
                }
                var totalPrice = productsPrice - discountAmount;
                response.Result = new BasketOutputDTO
                {
                    BasketDetails = basketDetails,
                    DiscountPrice = discountAmount,
                    TotalPrice = totalPrice
                };
            }
            catch (Exception ex)
            {
                response.Exception = ex;
                response.ErrorMessage = ex.Message;
                response.HttpCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        private async Task<List<BasketDetailOutputDTO>> GetBasketDetails(int? customerId, string basketKey)
        {
            return await (from b in _basketRepository.Table
                                                .WhereIf(customerId != null, x => x.CustomerId == customerId.Value)
                                                .WhereIf(customerId == null, x => x.BasketKey == basketKey)
                          join bd in _basketDetailRepository.Table on b.Id equals bd.BasketId
                          join p in _productRepository.Table on bd.ProductId equals p.Id
                          where p.IsActive
                          select new BasketDetailOutputDTO
                          {
                              ProductId = p.Id,
                              ProductName = p.ProductName,
                              Price = p.Price,
                              Quantity = bd.Quantity
                          }).AsNoTracking().ToListAsync();
        }

        public async Task<ResponseEntity<bool>> AddProductToBasket(EditProductToBasketInputDTO input)
        {
            var response = new ResponseEntity<bool>();
            try
            {
                var basketDetail = await (from b in _basketRepository.Table
                                          join bd in _basketDetailRepository.Table on b.Id equals bd.BasketId
                                          where bd.ProductId == input.ProductId && (b.CustomerId == input.CustomerId || b.BasketKey == input.BasketKey)
                                          select bd).FirstOrDefaultAsync();
                if (basketDetail != null)
                {
                    basketDetail.Quantity++;
                    await _basketDetailRepository.UpdateAsync(basketDetail);
                    response.Result = true;
                    return response;
                }
                var basket = await _basketRepository.FirstAsync(x => x.CustomerId == input.CustomerId || x.BasketKey == input.BasketKey);
                if (basket == null)
                {
                    basket = new Basket
                    {
                        BasketKey = input.BasketKey,
                        CustomerId = input.CustomerId
                    };
                    await _basketRepository.InsertAsync(basket);
                }

                await _basketDetailRepository.InsertAsync(new BasketDetail
                {
                    BasketId = basket.Id,
                    ProductId = input.ProductId,
                    Quantity = (int)decimal.One
                });

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Exception = ex;
                response.ErrorMessage = ex.Message;
                response.HttpCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        public async Task<ResponseEntity<bool>> DecreaseProductFromBasket(EditProductToBasketInputDTO input)
        {
            var response = new ResponseEntity<bool>();
            try
            {
                var basketDetail = await (from b in _basketRepository.Table
                                          join bd in _basketDetailRepository.Table on b.Id equals bd.BasketId
                                          where bd.ProductId == input.ProductId && (b.CustomerId == input.CustomerId || b.BasketKey == input.BasketKey)
                                          select bd).FirstOrDefaultAsync();
                if (basketDetail != null && basketDetail.Quantity > (int)decimal.One)
                {
                    basketDetail.Quantity--;
                    await _basketDetailRepository.UpdateAsync(basketDetail);
                }
                else if (basketDetail != null && basketDetail.Quantity == (int)decimal.One)
                {
                    await _basketDetailRepository.DeleteAsync(basketDetail);
                }
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Exception = ex;
                response.ErrorMessage = ex.Message;
                response.HttpCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        public async Task<ResponseEntity<bool>> DeleteProductFromBasket(EditProductToBasketInputDTO input)
        {
            var response = new ResponseEntity<bool>();
            try
            {
                var basketDetail = await (from b in _basketRepository.Table
                                          join bd in _basketDetailRepository.Table on b.Id equals bd.BasketId
                                          where bd.ProductId == input.ProductId && (b.CustomerId == input.CustomerId || b.BasketKey == input.BasketKey)
                                          select bd).FirstOrDefaultAsync();
                if (basketDetail != null)
                {
                    await _basketDetailRepository.DeleteAsync(basketDetail);
                }
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Exception = ex;
                response.ErrorMessage = ex.Message;
                response.HttpCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }
    }
}