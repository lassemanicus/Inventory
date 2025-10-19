using System;
    => _stock.Where(kv => kv.Value < threshold).Select(kv => (kv.Key, kv.Value));
}

public class OrderLine
{
    public Item Item { get; set; }
    public decimal Quantity { get; set; }
    public OrderLine(Item item, decimal quantity)
    {
        Item = item; Quantity = quantity;
    }
    public decimal LinePrice() => Item.PricePerUnit * Quantity;
    public override string ToString() => $"{Item.Name} x {Quantity} -> {LinePrice():C}";
}

public class Order
{
    public DateTime Time { get; set; } = DateTime.Now;
    public List<OrderLine> OrderLines { get; set; } = new();
    public decimal TotalPrice() => OrderLines.Sum(ol => ol.LinePrice());
    public override string ToString()
        => $"{Time:HH:mm:ss} | {string.Join(", ", OrderLines.Select(l => l.Item.Name + " x" + l.Quantity))} | Total: {TotalPrice():C}";
}

public class OrderBook
{
    private readonly Queue<Order> _queued = new();
    private readonly List<Order> _processed = new();
    
    public IEnumerable<Order> QueuedOrders => _queued;
    public IEnumerable<Order> ProcessedOrders => _processed;
    
    public void QueueOrder(Order order) => _queued.Enqueue(order);
    
    public Order? ProcessNextOrder()
    {
        if (_queued.Count == 0) return null;
        var next = _queued.Dequeue();
        _processed.Add(next);
        return next;
    }


    public decimal TotalRevenue() => _processed.Sum(o => o.TotalPrice());
}

public class Customer
{
    public string Name { get; set; } = string.Empty;
    public List<Order> Orders { get; } = new();


    public void CreateOrder(OrderBook book, Order order)
    {
        Orders.Add(order);
        book.QueueOrder(order);
    }
}
}