using AutoMapper;
using Domain.Entitites.Orders;
using DTO.Orders;

namespace API.Mappings.Orders
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<OrderCreateRequestDto, Order>();
            CreateMap<Order, OrderResponseDto>();
        }
    }
}

