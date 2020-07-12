using AnyukamHorgolta.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnyukamHorgolta.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepositoryAsync<ShoppingCart>
    {
        void Update(ShoppingCart obj);
    }
}
