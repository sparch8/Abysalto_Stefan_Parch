using Abysalto.StefanParch.Api.DTOs;
using Abysalto.StefanParch.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Abysalto.StefanParch.Api.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/cart")]
public sealed class CartsController(ICartService cartService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDto>> GetOrCreateCartAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var cart = await cartService.GetOrCreateCartAsync(userId, cancellationToken);

        return Ok(cart);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartDto>> AddItemAsync(
        Guid userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var cart = await cartService.AddItemAsync(userId, request, cancellationToken);

        return Ok(cart);
    }

    [HttpDelete("items/{cartItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItemAsync(
        Guid userId,
        Guid cartItemId,
        CancellationToken cancellationToken)
    {
        var removed = await cartService.RemoveItemAsync(userId, cartItemId, cancellationToken);

        return removed ? NoContent() : NotFound();
    }

    [HttpPatch("items/{cartItemId:guid}/quantity")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> UpdateQuantityAsync(
        Guid userId,
        Guid cartItemId,
        UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken)
    {
        var cart = await cartService.UpdateQuantityAsync(
            userId,
            cartItemId,
            request,
            cancellationToken);

        return cart is null ? NotFound() : Ok(cart);
    }
}
