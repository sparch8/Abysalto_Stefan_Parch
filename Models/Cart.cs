namespace Abysalto.StefanParch.Api.Models;

public sealed class Cart
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<CartItem> Items { get; } = new List<CartItem>();
}
