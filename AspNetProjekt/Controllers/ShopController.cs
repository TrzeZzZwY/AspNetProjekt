﻿using AspNetProjekt.Models;
using AspNetProjekt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AspNetProjekt.Controllers
{
    public class ShopController : Controller
    {
        private readonly IItemService _itemService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ShopController(IItemService itemService, ICategoryService categoryService, IWebHostEnvironment hostEnvironment)
        {
            _itemService = itemService;
            _categoryService = categoryService;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View(_itemService.FindAll());
        }

        public IActionResult CreateItem(ItemDto? itemDto)
        {
            if (itemDto is null)
                itemDto = new ItemDto();

            var categories = _categoryService.FindAll().ToList();
            itemDto.categoriesList = new List<SelectListItem>();
            foreach (var category in categories)
            {
                itemDto.categoriesList.Add(new SelectListItem(category.CategoryName, category.CategoryId.ToString()));
            }

            return View("CreateItem", itemDto);
        }

        [HttpPost]
        public IActionResult CreateItemForm([FromForm] ItemDto? itemDto)
        {
            if (itemDto is null)
                return CreateItem(itemDto);

            if (!ModelState.IsValid)
                return CreateItem(itemDto);
            Item item = itemDto.ConvertTo();

            string wwwrootPath = _hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(itemDto.ImageFile.FileName);
            string extension = Path.GetExtension(itemDto.ImageFile.FileName);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;

            item.ItemImageName = fileName;

            string path = Path.Combine(wwwrootPath + "/image", fileName);
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                itemDto.ImageFile.CopyTo(fileStream);
            }

            var response = _itemService.Save(item);
            return View("Index", _itemService.FindAll());
        }
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory([FromForm] CategoryDto? categoryDto)
        {
            if (categoryDto is null)
                return View();

            if (!ModelState.IsValid)
                return View(categoryDto);

            Category category = categoryDto.ConvertTo();
            var response = _categoryService.Save(category);
            return View("Index", _itemService.FindAll());
        }
    }
}