using Application.Features.Categories.Commands;
using Application.Features.Categories.Handlers.Create;
using Application.Services.Categories;
using CrudApi.Application.UnitTests.Mocks;
using Domain.Entitites.Categories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace CrudApi.Application.UnitTests.Categories.Commands
{
    public class CreateCategoryCommandHandlerTest
    {
        private readonly Mock<ICategoryService> _mockService;
        private readonly Category _category;
        private readonly IMediator _mediator;

        public CreateCategoryCommandHandlerTest()
        {
            _mockService = MockCategoryService.GetCategoryService();

        _category = new Category("Underwear", "This category will contain only underwear");

            // Set up MediatR with the handler
            var services = new ServiceCollection();
            services.AddMediatR(typeof(AddCategoryHandler).Assembly);
            services.AddSingleton(_mockService.Object);

            var serviceProvider = services.BuildServiceProvider();
            _mediator = serviceProvider.GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task CreateNewCategory()
        {
            // Add the category using MediatR (matching the application flow)
            var result = await _mediator.Send(new AddCategoryCommand(_category), CancellationToken.None);

            // Get all categories from the mocked service
            var categories = await _mockService.Object.GetCategoriesAsync(1, 10, CancellationToken.None);

            // Make sure that the result we got in return from the command handler is of type Category
            result.ShouldBeOfType<Category>();

            // Make sure that we now have 3 categories as we started with 2 categories in the mock service
            categories.TotalCount.ShouldBe(3);
            categories.Items.Count.ShouldBe(3);
        }
    }
}
