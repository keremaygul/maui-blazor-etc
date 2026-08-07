namespace StockTracker.Services;

using StockTracker.Models;

/// <summary>
/// Stok yönetim servisi — CRUD işlemleri + barkod simülasyonu.
/// 
/// 📝 BLAZOR DERSİ - SERVİS & DI (Dependency Injection):
/// 
/// Blazor'da servisler ASP.NET Core'daki gibi DI ile yönetilir.
/// MauiProgram.cs'de "builder.Services.AddSingleton<StockService>()" ile kayıt edilir.
/// 
/// Servis Lifetime'ları:
/// - AddSingleton: Uygulama boyunca tek instance (biz bunu kullanıyoruz)
/// - AddScoped: Her "scope" için bir instance (web'de her request, MAUI'de dikkatli kullan)
/// - AddTransient: Her inject edildiğinde yeni instance
/// 
/// MAUI'de genelde Singleton kullanılır çünkü uygulama tek bir süreçte çalışır.
/// </summary>
public class StockService
{
    // In-memory veri deposu (gerçek projede DB olur)
    private readonly List<Product> _products;
    private int _nextId;

    public StockService()
    {
        // Başlangıç verileri — depo senaryosu
        _products = new List<Product>
        {
            new() { Id = 1, Name = "Wireless Mouse", Barcode = "4012345678901", Category = "Electronics", Quantity = 45, Price = 29.99m, Location = "A-01-03", MinimumStock = 10 },
            new() { Id = 2, Name = "USB-C Cable 2m", Barcode = "4012345678902", Category = "Accessories", Quantity = 120, Price = 12.50m, Location = "A-02-01", MinimumStock = 20 },
            new() { Id = 3, Name = "Mechanical Keyboard", Barcode = "4012345678903", Category = "Electronics", Quantity = 3, Price = 89.90m, Location = "B-01-02", MinimumStock = 5 },
            new() { Id = 4, Name = "Monitor Stand", Barcode = "4012345678904", Category = "Furniture", Quantity = 0, Price = 45.00m, Location = "C-03-01", MinimumStock = 5 },
            new() { Id = 5, Name = "Webcam HD 1080p", Barcode = "4012345678905", Category = "Electronics", Quantity = 8, Price = 65.00m, Location = "A-01-05", MinimumStock = 5 },
            new() { Id = 6, Name = "Desk Lamp LED", Barcode = "4012345678906", Category = "Furniture", Quantity = 22, Price = 34.99m, Location = "C-02-04", MinimumStock = 8 },
            new() { Id = 7, Name = "Ethernet Cable 5m", Barcode = "4012345678907", Category = "Accessories", Quantity = 2, Price = 8.99m, Location = "A-02-03", MinimumStock = 15 },
            new() { Id = 8, Name = "Laptop Stand Aluminum", Barcode = "4012345678908", Category = "Furniture", Quantity = 15, Price = 55.00m, Location = "C-01-02", MinimumStock = 5 },
        };
        _nextId = _products.Max(p => p.Id) + 1;
    }

    /// <summary>Tüm ürünleri getir</summary>
    public List<Product> GetAll() => _products.OrderBy(p => p.Name).ToList();

    /// <summary>ID ile ürün getir</summary>
    public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);

    /// <summary>Barkod ile ürün ara</summary>
    public Product? GetByBarcode(string barcode) => _products.FirstOrDefault(p => p.Barcode == barcode);

    /// <summary>Kategoriye göre filtrele</summary>
    public List<Product> GetByCategory(string category) =>
        _products.Where(p => p.Category == category).OrderBy(p => p.Name).ToList();

    /// <summary>Arama (isim veya barkod)</summary>
    public List<Product> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return GetAll();
        
        return _products
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || p.Barcode.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || p.Location.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Name)
            .ToList();
    }

    /// <summary>Yeni ürün ekle</summary>
    public Product Add(Product product)
    {
        product.Id = _nextId++;
        product.LastUpdated = DateTime.Now;
        _products.Add(product);
        return product;
    }

    /// <summary>Ürün güncelle</summary>
    public bool Update(Product product)
    {
        var existing = GetById(product.Id);
        if (existing == null) return false;

        existing.Name = product.Name;
        existing.Barcode = product.Barcode;
        existing.Category = product.Category;
        existing.Quantity = product.Quantity;
        existing.Price = product.Price;
        existing.Location = product.Location;
        existing.MinimumStock = product.MinimumStock;
        existing.LastUpdated = DateTime.Now;
        return true;
    }

    /// <summary>Ürün sil</summary>
    public bool Delete(int id)
    {
        var product = GetById(id);
        if (product == null) return false;
        return _products.Remove(product);
    }

    /// <summary>Tüm kategorileri getir</summary>
    public List<string> GetCategories() =>
        _products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();

    /// <summary>Stok istatistikleri</summary>
    public StockSummary GetSummary() => new()
    {
        TotalProducts = _products.Count,
        TotalItems = _products.Sum(p => p.Quantity),
        TotalValue = _products.Sum(p => p.Quantity * p.Price),
        LowStockCount = _products.Count(p => p.Status == StockStatus.LowStock),
        OutOfStockCount = _products.Count(p => p.Status == StockStatus.OutOfStock),
    };

    /// <summary>
    /// Barkod simülasyonu — rastgele bir EAN-13 üretir.
    /// Gerçek hayatta bu bir barkod tarayıcıdan gelir (FiftyScan gibi).
    /// </summary>
    public string GenerateBarcode()
    {
        var random = new Random();
        // EAN-13: Ülke kodu (40-44 = Almanya) + Üretici + Ürün + Check digit
        var code = "40" + string.Concat(Enumerable.Range(0, 10).Select(_ => random.Next(0, 10)));
        
        // Check digit hesapla (EAN-13 standardı)
        int sum = 0;
        for (int i = 0; i < 12; i++)
            sum += (code[i] - '0') * (i % 2 == 0 ? 1 : 3);
        int checkDigit = (10 - sum % 10) % 10;
        
        return code + checkDigit;
    }
}

/// <summary>Dashboard için stok özet bilgileri</summary>
public class StockSummary
{
    public int TotalProducts { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalValue { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
}
