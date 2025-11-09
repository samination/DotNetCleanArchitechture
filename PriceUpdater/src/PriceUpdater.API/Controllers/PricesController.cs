using MediatR;
using Microsoft.AspNetCore.Mvc;
using PriceUpdater.Application.Prices.Commands;
using PriceUpdater.Application.Prices.Contracts;

namespace PriceUpdater.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PricesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PricesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PriceResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreatePrice([FromBody] CreatePriceRequest request, CancellationToken cancellationToken)
    {
        PriceResponse response = await _mediator.Send(new CreatePriceCommand(request.ProductId, request.Amount, request.CreatedAtUtc), cancellationToken);

        return CreatedAtAction(nameof(GetPrice), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PriceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrice(Guid id, CancellationToken cancellationToken)
    {
        return await Task.FromResult<IActionResult>(NotFound());
    }
}


