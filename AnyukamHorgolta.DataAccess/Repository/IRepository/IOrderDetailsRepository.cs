using AnyukamHorgolta.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnyukamHorgolta.DataAccess.Repository.IRepository
{
    public interface IOrderDetailsRepository : IRepositoryAsync<OrderDetails>
    {
        void Update(OrderDetails obj);
    }
}
