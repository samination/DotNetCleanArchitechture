using Application.Features.Orders.Commands;
using Application.Features.Orders.Queries;
using AutoMapper;
using Domain.Entitites.Orders;
using DTO.Orders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Orders
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public OrderController(IMapper mapper, IMediator mediator)
        {
            _mapper = mapper;
            _mediator = mediator;
        }

        // POST: api/Order/add
        [HttpPost("add")]
        public async Task<IActionResult> AddOrderAsync(OrderCreateRequestDto orderToCreate, CancellationToken ct)
        {
            Order order = _mapper.Map<Order>(orderToCreate);

            order = await _mediator.Send(new CreateOrderCommand(order), ct);

            OrderResponseDto mapped = _mapper.Map<OrderResponseDto>(order);

            return Ok(mapped);
        }

        // GET: api/Order/get/{id}
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetOrderByIdAsync(Guid id, CancellationToken ct)
        {
            Order order = await _mediator.Send(new GetOrderByIdQuery(id), ct);

            OrderResponseDto mapped = _mapper.Map<OrderResponseDto>(order);

            return Ok(mapped);
        }

        // POST: api/Order/pay/{id}
        [HttpPost("pay/{id}")]
        public async Task<IActionResult> PayOrderAsync(Guid id, CancellationToken ct)
        {
            Order order = await _mediator.Send(new PayOrderCommand(id), ct);

            OrderResponseDto mapped = _mapper.Map<OrderResponseDto>(order);

            return Ok(mapped);
        }
    }
}

