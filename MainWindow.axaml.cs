sing Avalonia.Controls;
_inventory.SetStock(screws, 500);
_inventory.SetStock(pump, 5);
_inventory.SetStock(oil, 200); // kg


// Predefined queued orders
var customer = new Customer { Name = "ACME Robotics" };


var o1 = new Order();
o1.OrderLines.Add(new OrderLine(screws, 100));
o1.OrderLines.Add(new OrderLine(oil, 20));


var o2 = new Order();
o2.OrderLines.Add(new OrderLine(pump, 1));


var o3 = new Order();
o3.OrderLines.Add(new OrderLine(screws, 50));
o3.OrderLines.Add(new OrderLine(oil, 15));


customer.CreateOrder(_orderBook, o1);
customer.CreateOrder(_orderBook, o2);
customer.CreateOrder(_orderBook, o3);
}


private void ProcessNextOrder()
{
    var processed = _orderBook.ProcessNextOrder();
    if (processed is null) return;


// Update inventory stock
    foreach (var line in processed.OrderLines)
        _inventory.DecreaseStock(line.Item, line.Quantity);


// UI updates
    RefreshQueues();
    ProcessedOrders.Add(processed.ToString());
    UpdateRevenue();


// Enable/disable command
    (ProcessNextOrderCommand as RelayCommand)?.RaiseCanExecuteChanged();
}


private void RefreshQueues()
{
    QueuedOrders.Clear();
    foreach (var q in _orderBook.QueuedOrders)
        QueuedOrders.Add(q.ToString());
}


private void UpdateRevenue()
{
    TotalRevenue = _orderBook.TotalRevenue();
// Notify binding system by resetting DataContext (simplest for sample)
    DataContext = null; DataContext = this;
}
}


// Simple ICommand implementation
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    { _execute = execute; _canExecute = canExecute; }
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
}