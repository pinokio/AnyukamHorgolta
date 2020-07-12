using AnyukamHorgolta.DataAccess.Data;
using AnyukamHorgolta.DataAccess.Repository.IRepository;
using AnyukamHorgolta.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnyukamHorgolta.DataAccess.Repository
{
    public class OrderHeaderRepository : RepositoryAsync<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderHeaderRepository(ApplicationDbContext db) : base (db)
        {
            _db = db;
        }
        public void Update(OrderHeader obj)
        {
            _db.Update(obj);
        }
    }
}
