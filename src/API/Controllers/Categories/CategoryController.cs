using Application.Features.Categories.Commands;
using Application.Features.Categories.Queries;
using AutoMapper;
using Domain.Entitites.Categories;
using DTO.Categories;
using DTO.Common;
using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Categories
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CategoryController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetAllCategories([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        { 
            var categories = await _mediator.Send(new GetAllCategoriesQuery(pageNumber, pageSize), ct);
            var categoryDtos = _mapper.Map<PaginatedResponseDto<CategoryResponseDto>>(categories);
            return Ok(categoryDtos);
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken ct)
        { 
            var category = await _mediator.Send(new GetCategoryByIdQuery(id), ct);
            if (category is null)
            {
                return NotFound();
            }

            var categoryDto = _mapper.Map<CategoryResponseDto>(category);
            return Ok(categoryDto);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddCategory([FromBody] CategoryAddRequestDto categoryToAdd, CancellationToken ct)
        { 
            // Map the DTO to a domain model
            Category mappedCategoryToAdd = _mapper.Map<Category>(categoryToAdd);

            // Add the category to the application database
            Category addedCategory = await _mediator.Send(new AddCategoryCommand(mappedCategoryToAdd), ct);

            // Map created category to response DTO
            CategoryResponseDto mappedCategoryToReturn = _mapper.Map<CategoryResponseDto>(addedCategory);

            // Return the mapped category
            return Ok(mappedCategoryToReturn);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryUpdateRequestDto categoryToUpdate, CancellationToken ct)
        {
            // Map the DTO to a domain model
            Category mappedCategoryToUpdate = _mapper.Map<Category>(categoryToUpdate);

            // Update the category in the application database
            Category updatedCategory = await _mediator.Send(new UpdateCategoryCommand(mappedCategoryToUpdate), ct);

            // Map updated category to response DTO
            CategoryResponseDto mappedCategoryToReturn = _mapper.Map<CategoryResponseDto>(updatedCategory);

            // Return the mapped category
            return Ok(mappedCategoryToReturn);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct)
        { 
            // Delete the category from the application database
            await _mediator.Send(new DeleteCategoryByIdCommand(id), ct);

            // Return a success message
            return Ok("Category deleted successfully");
        }
    }
}
