﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlixOne.BookStore.ProductService.Models;
using FlixOne.BookStore.ProductService.Persistence;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FlixOne.BookStore.ProductService.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _ProductRepository;
        public ProductController(IProductRepository ProductRepository)
        {
            _ProductRepository = ProductRepository;
        }

        public IActionResult Get()
        {
            var Productvm = _ProductRepository.GetAll().Select(Product
            => new ProductViewModel
            {
                CategoryId = Product.CategoryId,
                CategoryDescription = Product.Category.Description,
                CategoryName = Product.Category.Name,
                ProductDescription = Product.Description,
                ProductId = Product.Id,
                ProductImage = Product.Image,
                ProductName = Product.Name,
                ProductPrice = Product.Price
            }).ToList();
            return new OkObjectResult(Productvm);
        }
    }
}
