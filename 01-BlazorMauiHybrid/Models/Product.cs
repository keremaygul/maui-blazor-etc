namespace StockTracker.Models;

/// <summary>
/// Ürün modeli — Depo/stok takibi için temel entity.
/// 
/// 📝 BLAZOR DERSİ - MODEL:
/// Blazor'da veri modelleri normal C# class'larıdır.
/// ASP.NET MVC'den bildiğin model yapısının aynısı.
/// </summary>
public class Product
{
    public int Id { get; set; }

    /// <summary>Ürün adı</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Barkod numarası (EAN-13 formatında)</summary>
    public string Barcode { get; set; } = string.Empty;

    /// <summary>Ürün kategorisi</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Stok adedi</summary>
    public int Quantity { get; set; }

    /// <summary>Birim fiyat (EUR)</summary>
    public decimal Price { get; set; }

    /// <summary>Depo lokasyonu (ör: A-01-03)</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Minimum stok seviyesi — altına düşerse uyarı verir</summary>
    public int MinimumStock { get; set; } = 5;

    /// <summary>Son güncelleme tarihi</summary>
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    /// <summary>Stok durumu kontrolü</summary>
    public StockStatus Status => Quantity switch
    {
        0 => StockStatus.OutOfStock,
        _ when Quantity <= MinimumStock => StockStatus.LowStock,
        _ => StockStatus.InStock
    };
}

/// <summary>
/// Stok durumu enum'u
/// 
/// 📝 BLAZOR DERSİ - ENUM:
/// switch expression (C# 8+) ile birlikte kullanıyoruz.
/// Bu pattern matching özelliği Blazor UI'da renk kodlama için ideal.
/// </summary>
public enum StockStatus
{
    InStock,     // Yeterli stok var
    LowStock,    // Stok azalıyor (minimum seviyenin altı)
    OutOfStock   // Stok tükendi
}
