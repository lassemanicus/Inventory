using System;
using System.Collections.Generic;
using System.Linq;

namespace Inventory
{
    public abstract class Item
    {
        public string Name { get; set; } = "";
        public decimal PricePerUnit { get; set; }
        public override string ToString() => $"{Name} ({PricePerUnit:C} per unit)";
    }

    public class BulkItem : Item
    {
        public string MeasurementUnit { get; set; } = "kg";
        public override string ToString() => $"{Name} ({PricePerUnit:C} per {MeasurementUnit})";
    }

    public class UnitItem : Item
    {
        public decimal Weight { get; set; }
        public override string ToString() => $"{Name} ({PricePerUnit:C} each, {Weight} kg)";
    }

    public class Inventory
    {
        private readonly Dictionary<Item, decimal> _stock = new Dictionary<Item, decimal>();

        public IReadOnlyDictionary<Item, decimal> Stock => _stock;

        public void SetStock(Item item, decimal quantity) => _stock[item] = quantity;
        public decimal GetStock(Item item) => _stock.TryGetValue(item, out var q) ? q : 0m;

        public void DecreaseStock(Item item, decimal quantity)
        {
            var current = GetStock(item);
            var next = current - quantity;
            _stock[item] = next < 0m ? 0m : next;
        }

        public IEnumerable<(Item item, decimal quantity)> LowStockItems(decimal threshold = 5m)
        {
            return _stock.Where(kv => kv.Value < threshold).Select(kv => (kv.Key, kv.Value));
        }
    }

    public class OrderLine
    {
        public Item Item { get; set; }
        public decimal Quantity { get; set; }
        public OrderLine(Item item, decimal quantity)
        {
            Item = item;
            Quantity = quantity;
        }
        public decimal LinePrice() => Item.PricePerUnit * Quantity;
        public override string ToString() => $"{Item.Name} x {Quantity} -> {LinePrice():C}";
    }

    public class Order
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public List<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
        public decimal TotalPrice() => OrderLines.Sum(ol => ol.LinePrice());
        public override string ToString()
        {
            var items = string.Join(", ", OrderLines.Select(l => l.Item.Name + " x" + l.Quantity));
            return $"{Time:HH:mm:ss} | {items} | Total: {TotalPrice():C}";
        }
    }

    public class OrderBook
    {
        private readonly Queue<Order> _queued = new Queue<Order>();
        private readonly List<Order> _processed = new List<Order>();

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
        public string Name { get; set; } = "";
        public List<Order> Orders { get; } = new List<Order>();

        public void CreateOrder(OrderBook book, Order order)
        {
            Orders.Add(order);
            book.QueueOrder(order);
        }
    }
}
