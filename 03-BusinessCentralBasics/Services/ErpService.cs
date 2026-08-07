namespace MiniErp.Services;

using MiniErp.Models;

/// <summary>
/// 📝 BUSINESS CENTRAL DERSİ - ERP SERVİS KATMANI:
/// 
/// BC'deki Codeunit'in C# karşılığı.
/// Tüm iş mantığı burada: sipariş oluşturma, sevkiyat (posting),
/// faturalama ve stok güncelleme.
/// 
/// BC'deki "Post" kavramı çok önemli:
/// - Post = Belgeyi kesinleştir ve muhasebe kayıtları oluştur
/// - Geri alınamaz (Credit Memo ile ters kayıt yapılır)
/// - Stok, müşteri bakiyesi, GL (Genel Muhasebe) hep post ile güncellenir
/// </summary>
public class ErpService
{
    private readonly List<Customer> _customers;
    private readonly List<Item> _items;
    private readonly List<SalesOrder> _orders = new();
    private readonly List<PostedInvoice> _invoices = new();
    private int _nextOrderId = 1;
    private int _nextInvoiceId = 1;

    public ErpService()
    {
        // Seed data — Müşteriler
        _customers = new List<Customer>
        {
            new() { Id = 1, Number = "C-10000", Name = "TechStore Berlin GmbH", Email = "info@techstore-berlin.de", City = "Berlin", Country = "DE", CreditLimit = 50000 },
            new() { Id = 2, Number = "C-20000", Name = "Office Solutions München", Email = "order@officesol.de", City = "München", Country = "DE", CreditLimit = 30000 },
            new() { Id = 3, Number = "C-30000", Name = "Digital Hub Hamburg", Email = "purchase@digitalhub.de", City = "Hamburg", Country = "DE", CreditLimit = 25000 },
        };

        // Seed data — Ürünler (Items)
        _items = new List<Item>
        {
            new() { Id = 1, Number = "ITEM-1000", Description = "Wireless Mouse Pro", UnitPrice = 29.99m, Inventory = 150, UnitOfMeasure = "PCS" },
            new() { Id = 2, Number = "ITEM-1001", Description = "Mechanical Keyboard RGB", UnitPrice = 89.90m, Inventory = 75, UnitOfMeasure = "PCS" },
            new() { Id = 3, Number = "ITEM-1002", Description = "USB-C Hub 7-Port", UnitPrice = 45.50m, Inventory = 200, UnitOfMeasure = "PCS" },
            new() { Id = 4, Number = "ITEM-1003", Description = "Monitor Stand Aluminum", UnitPrice = 55.00m, Inventory = 40, UnitOfMeasure = "PCS" },
            new() { Id = 5, Number = "ITEM-1004", Description = "Webcam 4K Ultra", UnitPrice = 120.00m, Inventory = 60, UnitOfMeasure = "PCS" },
            new() { Id = 6, Number = "ITEM-1005", Description = "Ethernet Cable CAT7 5m", UnitPrice = 12.99m, Inventory = 500, UnitOfMeasure = "PCS" },
        };
    }

    // ===================== CUSTOMERS =====================
    public List<Customer> GetCustomers() => _customers.OrderBy(c => c.Name).ToList();
    public Customer? GetCustomer(int id) => _customers.FirstOrDefault(c => c.Id == id);

    // ===================== ITEMS =====================
    public List<Item> GetItems() => _items.OrderBy(i => i.Description).ToList();
    public Item? GetItem(int id) => _items.FirstOrDefault(i => i.Id == id);

    // ===================== SALES ORDERS =====================
    public List<SalesOrder> GetOrders() => _orders.OrderByDescending(o => o.OrderDate).ToList();
    public SalesOrder? GetOrder(int id) => _orders.FirstOrDefault(o => o.Id == id);

    /// <summary>
    /// Yeni satış siparişi oluştur
    /// BC'de: Sales Order Card → New
    /// </summary>
    public SalesOrder CreateOrder(int customerId)
    {
        var customer = GetCustomer(customerId);
        if (customer == null) throw new Exception("Customer not found!");

        var order = new SalesOrder
        {
            Id = _nextOrderId++,
            OrderNumber = $"SO-{_nextOrderId:D4}",
            CustomerId = customerId,
            CustomerName = customer.Name,
            OrderDate = DateTime.Today,
            Status = OrderStatus.Open
        };
        _orders.Add(order);
        return order;
    }

    /// <summary>
    /// Siparişe satır ekle
    /// BC'de: Sales Lines subpage
    /// </summary>
    public void AddOrderLine(int orderId, int itemId, int quantity)
    {
        var order = GetOrder(orderId);
        if (order == null) throw new Exception("Order not found!");
        if (order.Status != OrderStatus.Open)
            throw new Exception("Cannot modify a released order!");

        var item = GetItem(itemId);
        if (item == null) throw new Exception("Item not found!");

        if (item.Inventory < quantity)
            throw new Exception($"Insufficient stock! Available: {item.Inventory}, Requested: {quantity}");

        var line = new SalesOrderLine
        {
            LineNo = (order.Lines.Count + 1) * 10000,  // BC convention: 10000, 20000, 30000...
            ItemId = itemId,
            ItemNumber = item.Number,
            Description = item.Description,
            Quantity = quantity,
            UnitPrice = item.UnitPrice
        };
        order.Lines.Add(line);
    }

    /// <summary>
    /// 📝 BC DERSİ - RELEASE:
    /// Siparişi "Release" et → Düzenleme kilitleni, sevkiyata hazır.
    /// BC'de Release fonksiyonu siparişteki tüm kontrolleri yapar
    /// (stok yeterliliği, kredi limiti, vb.)
    /// </summary>
    public void ReleaseOrder(int orderId)
    {
        var order = GetOrder(orderId);
        if (order == null) throw new Exception("Order not found!");
        if (!order.Lines.Any()) throw new Exception("Cannot release an empty order!");
        if (order.Status != OrderStatus.Open)
            throw new Exception("Order is already released!");

        // Kredi limiti kontrolü
        var customer = GetCustomer(order.CustomerId);
        if (customer != null && customer.Balance + order.TotalAmount > customer.CreditLimit)
            throw new Exception($"Credit limit exceeded! Limit: €{customer.CreditLimit:N2}, Current Balance: €{customer.Balance:N2}");

        order.Status = OrderStatus.Released;
    }

    /// <summary>
    /// 📝 BC DERSİ - POST SHIPMENT:
    /// Sevkiyatı "Post" et → Stok düşer, sevkiyat belgesi oluşur.
    /// BC'de bu işlem geri alınamaz!
    /// </summary>
    public void PostShipment(int orderId)
    {
        var order = GetOrder(orderId);
        if (order == null) throw new Exception("Order not found!");
        if (order.Status != OrderStatus.Released && order.Status != OrderStatus.PartiallyShipped)
            throw new Exception("Order must be released before shipping!");

        foreach (var line in order.Lines)
        {
            if (line.QuantityRemaining <= 0) continue;

            var item = GetItem(line.ItemId);
            if (item == null) continue;

            // Stok düş
            int qtyToShip = Math.Min(line.QuantityRemaining, item.Inventory);
            item.Inventory -= qtyToShip;
            line.QuantityShipped += qtyToShip;
        }

        // Durum güncelle
        order.ShipmentDate = DateTime.Today;
        order.Status = order.Lines.All(l => l.QuantityRemaining == 0)
            ? OrderStatus.Shipped
            : OrderStatus.PartiallyShipped;
    }

    /// <summary>
    /// 📝 BC DERSİ - POST INVOICE:
    /// Faturayı "Post" et → Müşteri bakiyesi artar, fatura belgesi oluşur.
    /// </summary>
    public PostedInvoice PostInvoice(int orderId)
    {
        var order = GetOrder(orderId);
        if (order == null) throw new Exception("Order not found!");
        if (order.Status != OrderStatus.Shipped && order.Status != OrderStatus.PartiallyShipped)
            throw new Exception("Order must be shipped before invoicing!");

        // Fatura oluştur
        var invoice = new PostedInvoice
        {
            Id = _nextInvoiceId++,
            InvoiceNumber = $"INV-{_nextInvoiceId:D4}",
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            PostingDate = DateTime.Today,
            TotalAmount = order.TotalAmount,
            IsPaid = false
        };
        _invoices.Add(invoice);

        // Müşteri bakiyesi güncelle
        var customer = GetCustomer(order.CustomerId);
        if (customer != null)
            customer.Balance += order.TotalAmount;

        // Faturalanan miktarları güncelle
        foreach (var line in order.Lines)
            line.QuantityInvoiced = line.QuantityShipped;

        order.Status = OrderStatus.Invoiced;
        return invoice;
    }

    // ===================== INVOICES =====================
    public List<PostedInvoice> GetInvoices() => _invoices.OrderByDescending(i => i.PostingDate).ToList();

    // ===================== DASHBOARD STATS =====================
    public DashboardStats GetDashboardStats() => new()
    {
        TotalCustomers = _customers.Count,
        TotalItems = _items.Count,
        TotalOrders = _orders.Count,
        OpenOrders = _orders.Count(o => o.Status == OrderStatus.Open || o.Status == OrderStatus.Released),
        TotalInvoices = _invoices.Count,
        TotalRevenue = _invoices.Sum(i => i.TotalAmount),
        UnpaidInvoices = _invoices.Count(i => !i.IsPaid),
        LowStockItems = _items.Count(i => i.Inventory < 20),
        TotalInventoryValue = _items.Sum(i => i.Inventory * i.UnitPrice)
    };
}

public class DashboardStats
{
    public int TotalCustomers { get; set; }
    public int TotalItems { get; set; }
    public int TotalOrders { get; set; }
    public int OpenOrders { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalRevenue { get; set; }
    public int UnpaidInvoices { get; set; }
    public int LowStockItems { get; set; }
    public decimal TotalInventoryValue { get; set; }
}
