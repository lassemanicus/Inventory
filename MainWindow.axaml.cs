using Avalonia.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Inventory
{
    public partial class MainWindow : Window
    {
        
        public ObservableCollection<string> QueuedOrders { get; } = new();
        public ObservableCollection<string> ProcessedOrders { get; } = new();

        
        public decimal TotalRevenue { get; private set; }
        public ICommand ProcessNextOrderCommand { get; }

       
        private readonly Inventory _inventory = new();
        private readonly OrderBook _orderBook = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            SeedData();

            ProcessNextOrderCommand = new RelayCommand(
                _ => ProcessNextOrder(),
                _ => _orderBook.QueuedOrders.Any());

            RefreshQueues();
            UpdateRevenue();
        }

        private void SeedData()
        {
            
            var screws = new UnitItem { Name = "Screw", PricePerUnit = 0.25m, Weight = 0.01m };
            var pump   = new UnitItem { Name = "Hydraulic Pump", PricePerUnit = 1200m, Weight = 15m };
            var oil    = new BulkItem { Name = "Hydraulic Oil", PricePerUnit = 3.5m, MeasurementUnit = "kg" };

           
            _inventory.SetStock(screws, 500);
            _inventory.SetStock(pump, 5);
            _inventory.SetStock(oil, 200);

            
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

            foreach (var line in processed.OrderLines)
                _inventory.DecreaseStock(line.Item, line.Quantity);

            RefreshQueues();
            ProcessedOrders.Add(processed.ToString());
            UpdateRevenue();

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
            DataContext = null; DataContext = this;
        }
    }

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

    public class Person
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName  { get; set; } = string.Empty;
    }
}
