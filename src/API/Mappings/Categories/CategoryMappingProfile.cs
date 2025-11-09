using Application.Common.Models;
using AutoMapper;
using Domain.Entitites.Categories;
using DTO.Categories;
using DTO.Common;

namespace API.Mappings.Categories
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            #region Create

            CreateMap<CategoryAddRequestDto, Category>()
                .ConstructUsing(dto => new Category(dto.Name, dto.Description));

            #endregion Create

            #region Read

            CreateMap<Category, CategoryResponseDto>()
                .ForMember(dest => dest.RowVersion, opt => opt.MapFrom(src => src.RowVersion))
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            CreateMap<PaginatedResult<Category>, PaginatedResponseDto<CategoryResponseDto>>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            #endregion Read

            #region Update

            CreateMap<CategoryUpdateRequestDto, Category>()
                .ConstructUsing(dto => new Category(dto.Id, dto.Name, dto.Description))
                .AfterMap((dto, entity) => entity.SetConcurrencyToken(dto.RowVersion));

            #endregion Update

        }
    }
}
