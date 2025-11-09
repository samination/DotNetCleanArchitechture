using System.Collections.Generic;
using System.Linq;
using Application.Common.Models;
using Application.Services.Categories;
using Domain.Entitites.Categories;
using Moq;

namespace CrudApi.Application.UnitTests.Mocks
{
    public static class MockCategoryService
    {
        /// <summary>
        /// Mock the Category Service
        /// </summary>
        /// <returns></returns>
        public static Mock<ICategoryService> GetCategoryService()
        {
            // Create new list of categories and add the categories
            List<Category> categories = new List<Category>
            {
                new Category("T-Shirts", "This category contains only t-shirts."),
                new Category("Shorts", "This category will only contain shorts.")
            };

            // Initialize new Mock of type ICategoryService
            var mockCategoryService = new Mock<ICategoryService>();

            // Setup method to test Get Categories (The get all service)
            mockCategoryService
                .Setup(s => s.GetCategoriesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int pageNumber, int pageSize, CancellationToken _) =>
                {
                    var pagedItems = categories
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    return new PaginatedResult<Category>(pagedItems, categories.Count, pageNumber, pageSize);
                });

            // Configuration of what happens when we call the Create new Category method
            mockCategoryService
                .Setup(s => s.CreateCategoryAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category category, CancellationToken _) =>
                {
                    categories.Add(category);
                    return category;
                });

            // Return the mocked service for categories
            return mockCategoryService;
        }
    }
}