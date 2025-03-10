using System;
using System.Collections.Generic;

namespace PizzaBot.Models;

public partial class User
{
    public long Id { get; set; }

    public string Role { get; set; } = null!;

    public virtual ICollection<Order> OrderClients { get; set; } = new List<Order>();

    public virtual ICollection<Order> OrderCouriers { get; set; } = new List<Order>();
}
