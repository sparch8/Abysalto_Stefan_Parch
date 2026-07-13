using System.Net;
using System.Net.Http.Json;
using Abysalto.StefanParch.Api.DTOs;
using Abysalto.StefanParch.Api.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Abysalto.StefanParch.Api.Tests;

public sealed class CartEndpointTests
{
    private static readonly Guid UserId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid CartId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid ProductId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private static readonly Guid CartItemId = Guid.Parse("40000000-0000-0000-0000-000000000001");
    private static readonly DateTimeOffset CreatedAt = new(2026, 7, 13, 19, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset RequestedAt = new(2026, 7, 13, 19, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateCart_ReturnsCreated_WhenCartIsNew()
    {
        await using var application = new CartApiApplication(service =>
        {
            service.CreateCartResult = new CartCreationResult(SampleCart(), Created: true);
        });

        using var client = application.CreateClient();

        var response = await client.PostAsJsonAsync("/api/cart", new CreateCartRequest(UserId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith($"/api/cart/{UserId}", response.Headers.Location?.ToString());

        var cart = await response.Content.ReadFromJsonAsync<CartDto>();
        AssertCart(cart);
    }

    [Fact]
    public async Task CreateCart_ReturnsOk_WhenCartAlreadyExists()
    {
        await using var application = new CartApiApplication(service =>
        {
            service.CreateCartResult = new CartCreationResult(SampleCart(), Created: false);
        });

        using var client = application.CreateClient();

        var response = await client.PostAsJsonAsync("/api/cart", new CreateCartRequest(UserId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cart = await response.Content.ReadFromJsonAsync<CartDto>();
        AssertCart(cart);
    }

    [Fact]
    public async Task CreateCart_ReturnsBadRequest_WhenUserIdIsEmpty()
    {
        await using var application = new CartApiApplication();
        using var client = application.CreateClient();

        var response = await client.PostAsJsonAsync("/api/cart", new CreateCartRequest(Guid.Empty));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCart_ReturnsOk()
    {
        await using var application = new CartApiApplication(service =>
        {
            service.GetOrCreateCartResult = SampleCart();
        });

        using var client = application.CreateClient();

        var response = await client.GetAsync($"/api/cart/{UserId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cart = await response.Content.ReadFromJsonAsync<CartDto>();
        AssertCart(cart);
    }

    [Fact]
    public async Task AddItem_ReturnsOk()
    {
        AddCartItemRequest? capturedRequest = null;
        await using var application = new CartApiApplication(service =>
        {
            service.AddItemHandler = (_, request) =>
            {
                capturedRequest = request;
                return SampleCart();
            };
        });

        using var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/cart/{UserId}/items",
            new AddCartItemRequest(ProductId, 2, 12.50m));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(ProductId, capturedRequest?.ProductId);
        Assert.Equal(2, capturedRequest?.Quantity);
        Assert.Equal(12.50m, capturedRequest?.UnitPrice);

        var cart = await response.Content.ReadFromJsonAsync<CartDto>();
        AssertCart(cart);
    }

    [Fact]
    public async Task AddItem_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        await using var application = new CartApiApplication();
        using var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/cart/{UserId}/items",
            new AddCartItemRequest(Guid.Empty, 0, 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateQuantity_ReturnsOk_WhenItemExists()
    {
        UpdateCartItemQuantityRequest? capturedRequest = null;
        await using var application = new CartApiApplication(service =>
        {
            service.UpdateQuantityHandler = (_, _, request) =>
            {
                capturedRequest = request;
                return SampleCart(quantity: 5);
            };
        });

        using var client = application.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"/api/cart/{UserId}/items/{ProductId}",
            new UpdateCartItemQuantityRequest(5));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(5, capturedRequest?.Quantity);

        var cart = await response.Content.ReadFromJsonAsync<CartDto>();
        AssertCart(cart, quantity: 5, total: 62.50m);
    }

    [Fact]
    public async Task UpdateQuantity_ReturnsNotFound_WhenItemDoesNotExist()
    {
        await using var application = new CartApiApplication(service =>
        {
            service.UpdateQuantityHandler = (_, _, _) => null;
        });

        using var client = application.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"/api/cart/{UserId}/items/{ProductId}",
            new UpdateCartItemQuantityRequest(5));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Cart item was not found.", problem?.Detail);
    }

    [Fact]
    public async Task UpdateQuantity_ReturnsBadRequest_WhenQuantityIsInvalid()
    {
        await using var application = new CartApiApplication();
        using var client = application.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"/api/cart/{UserId}/items/{ProductId}",
            new UpdateCartItemQuantityRequest(0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RemoveItem_ReturnsNoContent_WhenItemExists()
    {
        await using var application = new CartApiApplication(service =>
        {
            service.RemoveItemResult = true;
        });

        using var client = application.CreateClient();

        var response = await client.DeleteAsync($"/api/cart/{UserId}/items/{ProductId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RemoveItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        await using var application = new CartApiApplication(service =>
        {
            service.RemoveItemResult = false;
        });

        using var client = application.CreateClient();

        var response = await client.DeleteAsync($"/api/cart/{UserId}/items/{ProductId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Cart item was not found.", problem?.Detail);
    }

    [Fact]
    public async Task GetTotal_ReturnsOk_WhenCartExists()
    {
        await using var application = new CartApiApplication(service =>
        {
            service.GetTotalResult = new CartTotalDto(UserId, 25.00m);
        });

        using var client = application.CreateClient();

        var response = await client.GetAsync($"/api/cart/{UserId}/total");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var total = await response.Content.ReadFromJsonAsync<CartTotalDto>();
        Assert.Equal(UserId, total?.UserId);
        Assert.Equal(25.00m, total?.Total);
    }

    [Fact]
    public async Task GetTotal_ReturnsNotFound_WhenCartDoesNotExist()
    {
        await using var application = new CartApiApplication(service =>
        {
            service.GetTotalResult = null;
        });

        using var client = application.CreateClient();

        var response = await client.GetAsync($"/api/cart/{UserId}/total");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Cart was not found.", problem?.Detail);
    }

    [Fact]
    public async Task RequestCheckout_ReturnsAccepted_WhenCartExists()
    {
        await using var application = new CartApiApplication(service =>
        {
            service.RequestCheckoutResult = new CheckoutRequestResult(UserId, CartId, 25.00m, RequestedAt);
        });

        using var client = application.CreateClient();

        var response = await client.PostAsync($"/api/cart/{UserId}/checkout", content: null);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var checkout = await response.Content.ReadFromJsonAsync<CheckoutRequestResult>();
        Assert.Equal(UserId, checkout?.UserId);
        Assert.Equal(CartId, checkout?.CartId);
        Assert.Equal(25.00m, checkout?.Total);
        Assert.Equal(RequestedAt, checkout?.RequestedAt);
    }

    [Fact]
    public async Task RequestCheckout_ReturnsNotFound_WhenCartDoesNotExist()
    {
        await using var application = new CartApiApplication(service =>
        {
            service.RequestCheckoutResult = null;
        });

        using var client = application.CreateClient();

        var response = await client.PostAsync($"/api/cart/{UserId}/checkout", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Cart was not found.", problem?.Detail);
    }

    private static CartDto SampleCart(int quantity = 2)
    {
        var item = new CartItemDto(
            CartItemId,
            ProductId,
            quantity,
            12.50m,
            quantity * 12.50m);

        return new CartDto(
            CartId,
            UserId,
            CreatedAt,
            new[] { item },
            item.LineTotal);
    }

    private static void AssertCart(CartDto? cart, int quantity = 2, decimal total = 25.00m)
    {
        Assert.NotNull(cart);
        Assert.Equal(CartId, cart.Id);
        Assert.Equal(UserId, cart.UserId);
        Assert.Equal(CreatedAt, cart.CreatedAt);
        Assert.Equal(total, cart.Total);

        var item = Assert.Single(cart.Items);
        Assert.Equal(CartItemId, item.Id);
        Assert.Equal(ProductId, item.ProductId);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(12.50m, item.UnitPrice);
        Assert.Equal(total, item.LineTotal);
    }

    private sealed class CartApiApplication : WebApplicationFactory<Program>
    {
        private readonly Action<FakeCartService>? configureService;

        public CartApiApplication(Action<FakeCartService>? configureService = null)
        {
            this.configureService = configureService;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ICartService>();

                var fakeCartService = new FakeCartService();
                configureService?.Invoke(fakeCartService);
                services.AddSingleton<ICartService>(fakeCartService);
            });
        }
    }

    private sealed class FakeCartService : ICartService
    {
        public CartCreationResult CreateCartResult { get; set; } =
            new(SampleCart(), Created: true);

        public CartDto GetOrCreateCartResult { get; set; } = SampleCart();

        public Func<Guid, AddCartItemRequest, CartDto> AddItemHandler { get; set; } =
            (_, _) => SampleCart();

        public bool RemoveItemResult { get; set; } = true;

        public Func<Guid, Guid, UpdateCartItemQuantityRequest, CartDto?> UpdateQuantityHandler { get; set; } =
            (_, _, _) => SampleCart();

        public CartTotalDto? GetTotalResult { get; set; } = new(UserId, 25.00m);

        public CheckoutRequestResult? RequestCheckoutResult { get; set; } =
            new(UserId, CartId, 25.00m, RequestedAt);

        public Task<CartCreationResult> CreateCartAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateCartResult);
        }

        public Task<CartDto> GetOrCreateCartAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetOrCreateCartResult);
        }

        public Task<CartDto> AddItemAsync(
            Guid userId,
            AddCartItemRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(AddItemHandler(userId, request));
        }

        public Task<bool> RemoveItemAsync(
            Guid userId,
            Guid productId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RemoveItemResult);
        }

        public Task<CartDto?> UpdateQuantityAsync(
            Guid userId,
            Guid productId,
            UpdateCartItemQuantityRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(UpdateQuantityHandler(userId, productId, request));
        }

        public Task<CartTotalDto?> GetTotalAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetTotalResult);
        }

        public Task<CheckoutRequestResult?> RequestCheckoutAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RequestCheckoutResult);
        }
    }
}
