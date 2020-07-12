using AnyukamHorgolta.DataAccess.Data;
using AnyukamHorgolta.DataAccess.Repository.IRepository;
using AnyukamHorgolta.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnyukamHorgolta.DataAccess.Repository
{
    public class OrderDetailsRepository : RepositoryAsync<OrderDetails>, IOrderDetailsRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderDetailsRepository(ApplicationDbContext db) : base (db)
        {
            _db = db;
        }
        public void Update(OrderDetails obj)
        {
            _db.Update(obj);
        }
    }
}
