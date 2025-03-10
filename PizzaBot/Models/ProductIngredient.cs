using System;
using System.Collections.Generic;

namespace PizzaBot.Models;

public partial class ProductIngredient
{
    public int ProductId { get; set; }

    public int IngredientId { get; set; }

    public int Amount { get; set; }

    public virtual Ingredient Ingredient { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
