using System;
using System.Collections.Generic;
using System.Text;

namespace AnyukamHorgolta.Models.ViewModels
{
    public class OrderVM
    {
        public IEnumerable<OrderHeader> OrderHeaders { get; set; }
        //public PagingInfo PagingInfo { get; set; }
    }
}
