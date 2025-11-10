using API.Mappings.Categories;
using Application.Common.Models;
using Application.Features.Categories.Handlers.Read;
using Application.Features.Categories.Queries;
using Application.Services.Categories;
using AutoMapper;
using CrudApi.Application.UnitTests.Mocks;
using Domain.Entitites.Categories;
using Shouldly;

namespace CrudApi.Application.UnitTests.Categories.Queries
{
    public class GetAllCategoriesRequestHandlerTests
    {
        private readonly Mock<ICategoryService> _mockCategoryService;

        public GetAllCategoriesRequestHandlerTests()
        {
            _mockCategoryService = MockCategoryService.GetCategoryService();
        }

        [Fact]
        public async Task GetCategoriesTest()
        {
            var handler = new GetAllCategoriesHandler(_mockCategoryService.Object);

            var result = await handler.Handle(new GetAllCategoriesQuery(1, 20), CancellationToken.None);

            result.ShouldBeOfType<PaginatedResult<Category>>();

            result.TotalCount.ShouldBe(2);
            result.Items.Count.ShouldBe(2);
        }
    }
}
