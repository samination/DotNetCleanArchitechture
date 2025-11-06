using Application.Common.Models;
using Domain.Entitites.Categories;
using MediatR;

namespace Application.Features.Categories.Queries
{
    public record GetAllCategoriesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<Category>>;
}
