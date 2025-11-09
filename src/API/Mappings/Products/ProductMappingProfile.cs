using AutoMapper;
using Domain.Entitites.Products;
using DTO.Products;

namespace API.Mappings.Products
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            #region Create

            CreateMap<ProductCreateRequestDto, Product>()
                .ConstructUsing(dto => new Product(dto.Name, dto.Description, dto.Price, dto.Stock, dto.CategoryId));

            #endregion Create

            #region Read

            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.RowVersion, opt => opt.MapFrom(src => src.RowVersion))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));

            #endregion Read

            #region Update

            CreateMap<ProductUpdateRequestDto, Product>()
                .ConstructUsing(dto => new Product(dto.Id, dto.Name, dto.Description, dto.Price, dto.Stock, dto.CategoryId))
                .AfterMap((dto, entity) => entity.SetConcurrencyToken(dto.RowVersion));

            #endregion Update
        }


    }
}
