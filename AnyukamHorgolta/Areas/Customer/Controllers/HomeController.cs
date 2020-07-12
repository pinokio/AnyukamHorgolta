using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AnyukamHorgolta.Models;
using AnyukamHorgolta.Models.ViewModels;
using AnyukamHorgolta.DataAccess.Repository.IRepository;
using System.Security.Claims;
using AnyukamHorgolta.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace AnyukamHorgolta.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;


        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Product> productList = await _unitOfWork.Product.GetAllAsync(includeProperties: "Category");

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null) 
            {
                var count = (await _unitOfWork.ShoppingCart.GetAllAsync(c => c.ApplicationUserId == claim.Value)).ToList().Count();

                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);
            }
            
            return View(productList);

        }
        
        public async Task<IActionResult> Details(int id)
        {
            var productFromDb = await _unitOfWork.Product.GetFirstOrDefaultAsync(u => u.Id == id, includeProperties: "Category");
            ShoppingCart cartObj = new ShoppingCart()
            {
                Product = productFromDb,
                ProductId = productFromDb.Id
            };
            return View(cartObj);
        }
        
        [HttpPost] 
        [ValidateAntiForgeryToken]
        [Authorize] 
        public async Task<IActionResult> Details(ShoppingCart CartObject)
        {
            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = await _unitOfWork.ShoppingCart.GetFirstOrDefaultAsync(
                    u => u.ApplicationUserId == CartObject.ApplicationUserId && u.ProductId == CartObject.ProductId
                    , includeProperties: "Product"
                    );

                if (cartFromDb == null)
                {
                    await _unitOfWork.ShoppingCart.AddAsync(CartObject);
                }
                else
                {
                    cartFromDb.Count += CartObject.Count;
                   
                }
                _unitOfWork.Save();

                var count = (await _unitOfWork.ShoppingCart.GetAllAsync(c => c.ApplicationUserId == CartObject.ApplicationUserId)).ToList().Count();

                
                HttpContext.Session.SetInt32(SD.ssShoppingCart, count); 

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var productFromDb = await _unitOfWork.Product.GetFirstOrDefaultAsync(u => u.Id == CartObject.ProductId, includeProperties: "Category");
                ShoppingCart cartObj = new ShoppingCart()
                {
                    Product = productFromDb,
                    ProductId = productFromDb.Id
                };
                return View(cartObj);
            }
        }
        


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
