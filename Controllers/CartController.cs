using Abysalto.StefanParch.Api.DTOs;
using Abysalto.StefanParch.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Abysalto.StefanParch.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Produces("application/json")]
public sealed class CartController(ICartService cartService) : ControllerBase
{
    /// <summary>
    /// Creates a cart for a user, or returns the existing cart when one already exists.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartDto>> CreateCartAsync(
        CreateCartRequest request,
        CancellationToken cancellationToken)
    {
        var result = await cartService.CreateCartAsync(request.UserId, cancellationToken);

        if (!result.Created)
        {
            return Ok(result.Cart);
        }

        return CreatedAtAction(
            nameof(GetCartAsync),
            new { userId = result.Cart.UserId },
            result.Cart);
    }

    /// <summary>
    /// Gets a user's cart, creating one when it does not exist.
    /// </summary>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDto>> GetCartAsync(
        [FromRoute] UserCartRouteRequest route,
        CancellationToken cancellationToken)
    {
        var cart = await cartService.GetOrCreateCartAsync(route.UserId, cancellationToken);

        return Ok(cart);
    }

    /// <summary>
    /// Adds a product to a user's cart, increasing quantity when the product already exists.
    /// </summary>
    [HttpPost("{userId:guid}/items")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartDto>> AddItemAsync(
        [FromRoute] UserCartRouteRequest route,
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var cart = await cartService.AddItemAsync(route.UserId, request, cancellationToken);

        return Ok(cart);
    }

    /// <summary>
    /// Replaces the quantity for an existing product in a user's cart.
    /// </summary>
    [HttpPut("{userId:guid}/items/{productId:guid}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> UpdateQuantityAsync(
        [FromRoute] CartItemRouteRequest route,
        UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken)
    {
        var cart = await cartService.UpdateQuantityAsync(
            route.UserId,
            route.ProductId,
            request,
            cancellationToken);

        return cart is null
            ? NotFoundProblem("Cart item was not found.")
            : Ok(cart);
    }

    /// <summary>
    /// Removes a product from a user's cart.
    /// </summary>
    [HttpDelete("{userId:guid}/items/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItemAsync(
        [FromRoute] CartItemRouteRequest route,
        CancellationToken cancellationToken)
    {
        var removed = await cartService.RemoveItemAsync(route.UserId, route.ProductId, cancellationToken);

        return removed
            ? NoContent()
            : NotFoundProblem("Cart item was not found.");
    }

    /// <summary>
    /// Gets the total monetary value of a user's cart.
    /// </summary>
    [HttpGet("{userId:guid}/total")]
    [ProducesResponseType(typeof(CartTotalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartTotalDto>> GetTotalAsync(
        [FromRoute] UserCartRouteRequest route,
        CancellationToken cancellationToken)
    {
        var total = await cartService.GetTotalAsync(route.UserId, cancellationToken);

        return total is null
            ? NotFoundProblem("Cart was not found.")
            : Ok(total);
    }

    /// <summary>
    /// Requests checkout for a user's cart.
    /// </summary>
    [HttpPost("{userId:guid}/checkout")]
    [ProducesResponseType(typeof(CheckoutRequestResult), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CheckoutRequestResult>> RequestCheckoutAsync(
        [FromRoute] UserCartRouteRequest route,
        CancellationToken cancellationToken)
    {
        var checkoutRequest = await cartService.RequestCheckoutAsync(route.UserId, cancellationToken);

        return checkoutRequest is null
            ? NotFoundProblem("Cart was not found.")
            : Accepted(checkoutRequest);
    }

    private ActionResult NotFoundProblem(string detail)
    {
        return NotFound(new ProblemDetails
        {
            Type = "https://httpstatuses.com/404",
            Title = "Resource not found.",
            Status = StatusCodes.Status404NotFound,
            Detail = detail,
            Instance = HttpContext.Request.Path
        });
    }
}
