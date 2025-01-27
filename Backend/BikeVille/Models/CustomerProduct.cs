using System;
using System.Collections.Generic;

namespace BikeVille.Models;

public partial class CustomerProduct
{
    public int CustomerId { get; set; }

    public int ProductId { get; set; }

    public int? Quantity { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime AddDate { get; set; }

    public bool InCart { get; set; }

    public bool Purchased { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
