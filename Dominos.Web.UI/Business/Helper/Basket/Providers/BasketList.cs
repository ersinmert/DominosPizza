using Dominos.Web.UI.Models.Basket;

namespace Dominos.Web.UI.Business.Helper.Basket.Providers
{
    public class BasketList : BaseProvider, IProvider<BasketViewModel>
    {
        public void Execute(BasketViewModel model)
        {
            FillBasketList(model);
        }
    }
}