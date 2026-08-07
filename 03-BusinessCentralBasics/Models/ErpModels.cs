namespace MiniErp.Models;

/// <summary>
/// 📝 BUSINESS CENTRAL DERSİ - TEMEL ERP MODELLERİ:
/// 
/// BC'de her şey "Document" (belge) merkezlidir:
/// Sales Order → Sales Shipment → Sales Invoice
/// Purchase Order → Purchase Receipt → Purchase Invoice
/// 
/// Her belgenin bir Header (başlık) ve Lines (satırlar) yapısı vardır.
/// Bu pattern tüm ERP sistemlerinde ortaktır (SAP, Oracle, BC...)
/// </summary>

// ============================================================
// MÜŞTERİ
// ============================================================
public class Customer
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;     // BC: "No."
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = "DE";
    public decimal CreditLimit { get; set; } = 10000m;
    public decimal Balance { get; set; }                    // Açık bakiye
}

// ============================================================
// ÜRÜN (ITEM)
// ============================================================
public class Item
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;     // BC: "No."
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Inventory { get; set; }                     // Mevcut stok
    public string UnitOfMeasure { get; set; } = "PCS";     // Birim (adet, kg, m)
}

// ============================================================
// SATIŞ SİPARİŞİ (SALES ORDER) — Belge Başlığı
// ============================================================
/// <summary>
/// 📝 BC'de Satış Süreci:
/// 1. Sales Quote (Teklif) → Opsiyonel
/// 2. Sales Order (Sipariş) → Müşteri onayladı
/// 3. Sales Shipment (Sevkiyat) → Ürünler gönderildi (Post)
/// 4. Sales Invoice (Fatura) → Ödeme talep edildi (Post)
/// 5. Payment (Ödeme) → Müşteri ödedi
/// 
/// "Post" = Belgeyi kesinleştir (geri dönüşü zor/yok)
/// </summary>
public class SalesOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;  // SO-001
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.Today;
    public DateTime? ShipmentDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Open;
    public List<SalesOrderLine> Lines { get; set; } = new();

    // Hesaplanmış alanlar (BC'deki FlowField gibi)
    public decimal TotalAmount => Lines.Sum(l => l.LineAmount);
    public int TotalItems => Lines.Count;
}

// ============================================================
// SATIŞ SİPARİŞ SATIRI (SALES ORDER LINE) — Belge Satırı
// ============================================================
public class SalesOrderLine
{
    public int LineNo { get; set; }
    public int ItemId { get; set; }
    public string ItemNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineAmount => Quantity * UnitPrice;
    public int QuantityShipped { get; set; }                // Sevk edilen miktar
    public int QuantityInvoiced { get; set; }               // Faturalanan miktar
    public int QuantityRemaining => Quantity - QuantityShipped;
}

// ============================================================
// FATURA (POSTED SALES INVOICE)
// ============================================================
/// <summary>
/// 📝 BC'de "Posted" belgeler değiştirilemez!
/// Sipariş "Post" edilince ayrı bir Posted Invoice oluşur.
/// Orijinal sipariş silinir veya arşivlenir.
/// </summary>
public class PostedInvoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;  // INV-001
    public string OrderNumber { get; set; } = string.Empty;    // İlişkili sipariş
    public string CustomerName { get; set; } = string.Empty;
    public DateTime PostingDate { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsPaid { get; set; }
}

// ============================================================
// ENUM'LAR
// ============================================================
public enum OrderStatus
{
    Open,           // Düzenleniyor
    Released,       // Onaylandı (düzenleme kilitli)
    PartiallyShipped, // Kısmen sevk edildi
    Shipped,        // Tamamen sevk edildi
    Invoiced,       // Faturalandi
    Completed       // Tamamlandı
}
