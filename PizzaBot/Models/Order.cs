using System;
using System.Collections.Generic;

namespace PizzaBot.Models;

public partial class Order
{
    public int Id { get; set; }

    public long? ClientId { get; set; }

    public long? CourierId { get; set; }

    public string? Address { get; set; }

    public decimal ProductsCost { get; set; }

    public decimal DeliveryCost { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? PerformedAt { get; set; }

    public virtual User? Client { get; set; }

    public virtual User? Courier { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
