using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AnyukamHorgolta.DataAccess.Repository.IRepository;
using AnyukamHorgolta.Models;
using AnyukamHorgolta.Models.ViewModels;
using AnyukamHorgolta.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace AnyukamHorgolta.Areas.Sales.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderDetailsVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            OrderVM = new OrderDetailsVM
            {
                OrderHeader = await _unitOfWork.OrderHeader.GetFirstOrDefaultAsync(u => u.Id == id, includeProperties: "ApplicationUser"),
                OrderDetails = await _unitOfWork.OrderDetails.GetAllAsync(o => o.OrderId == id, includeProperties: "Product")
            };
            return View(OrderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Details")]
        public async Task<IActionResult> Details(string stripeToken)
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetFirstOrDefaultAsync(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");

            if (stripeToken != null)
            {
                //process the payment
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 10),
                    Currency = "huf",
                    Description = "Order ID : " + orderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.Id == null)
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    orderHeader.TransactionId = charge.Id;
                }
                if (charge.Status.ToLower() == "succeeded")
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    orderHeader.PaymentDate = DateTime.Now;
                }

                _unitOfWork.Save();
            }

            return RedirectToAction("Details", "Order", new { id = orderHeader.Id });
        }

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> StartProcessing(int id)
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetFirstOrDefaultAsync(u => u.Id == id);
            orderHeader.OrderStatus = SD.StatusProcessing;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> ShipOrder()
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetFirstOrDefaultAsync(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> CancelOrder(int id)
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetFirstOrDefaultAsync(u => u.Id == id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 10),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = orderHeader.TransactionId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                orderHeader.OrderStatus = SD.StatusRefunded;
                orderHeader.PaymentStatus = SD.StatusRefunded;
            }
            else
            {
                orderHeader.OrderStatus = SD.StatusCancelled;
                orderHeader.PaymentStatus = SD.StatusCancelled;
            }

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        #region API Calls

        [HttpGet]
        public async Task<IActionResult> GetOrderList(string status)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeaderList;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaderList = await _unitOfWork.OrderHeader.GetAllAsync(includeProperties: "ApplicationUser");
            }
            else
            {
                orderHeaderList = await _unitOfWork.OrderHeader.GetAllAsync(c => c.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
            }

            orderHeaderList = GetStatus(orderHeaderList, status);

            return Json(new { data = orderHeaderList });
        }

        private IEnumerable<OrderHeader> GetStatus(IEnumerable<OrderHeader> orderHeaderList, string status) =>
            status switch
            {
                "pending" => orderHeaderList.Where(o => o.PaymentStatus == SD.PaymentStatusDelayedPayment),
                "inprocess" => orderHeaderList.Where(o => o.OrderStatus == SD.StatusApproved || o.OrderStatus == SD.StatusProcessing),
                "completed" => orderHeaderList.Where(o => o.OrderStatus == SD.StatusShipped),
                "rejected" => orderHeaderList.Where(o => o.OrderStatus == SD.StatusCancelled || o.OrderStatus == SD.StatusRefunded || o.OrderStatus == SD.PaymentStatusRejected),
                _ => orderHeaderList,
            };


        #endregion
    }
}