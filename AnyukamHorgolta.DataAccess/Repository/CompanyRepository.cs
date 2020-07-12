using AnyukamHorgolta.DataAccess.Data;
using AnyukamHorgolta.DataAccess.Repository.IRepository;
using AnyukamHorgolta.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnyukamHorgolta.DataAccess.Repository
{
    class CompanyRepository : RepositoryAsync<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _db;

        public CompanyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Company company)
        {
            _db.Update(company);
        }
    }
}
