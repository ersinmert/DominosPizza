using System.Threading.Tasks;
using Dominos.Business.BasketService;
using Dominos.Common.DTO.Input;
using Microsoft.AspNetCore.Mvc;

namespace Dominos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketController : BaseController
    {
        public BasketController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        private readonly IBasketService _basketService;

        [HttpGet]
        [Route("get")]
        public async Task<IActionResult> GetCustomerBasket(int? customerId, string basketKey)
        {
            return HttpEntity(await _basketService.GetCustomerBasket(customerId, basketKey));
        }

        [HttpPost]
        [Route("add-product")]
        public async Task<IActionResult> AddProductToBasket(EditProductToBasketInputDTO input)
        {
            return HttpEntity(await _basketService.AddProductToBasket(input));
        }

        [HttpPost]
        [Route("decrease-product")]
        public async Task<IActionResult> DecreseProductFromBasket(EditProductToBasketInputDTO input)
        {
            return HttpEntity(await _basketService.DecreaseProductFromBasket(input));
        }

        [HttpPost]
        [Route("delete-product")]
        public async Task<IActionResult> DeletProductFromBasket(EditProductToBasketInputDTO input)
        {
            return HttpEntity(await _basketService.DeleteProductFromBasket(input));
        }
    }
}