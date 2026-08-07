// ============================================================
// 📝 PROTOBUF DERSİ - ANA PROGRAM
// ============================================================
// Bu program Protobuf'un temellerini öğretir:
// 1. Protobuf mesaj oluşturma
// 2. Serialize (binary) / Deserialize
// 3. JSON karşılaştırma (boyut & hız)
// ============================================================

using System.Diagnostics;
using System.Text.Json;
using Google.Protobuf;
using ProtobufDemo.Protos;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║    📦 Protobuf Demo - Warehouse App     ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine();

// ============================================================
// BÖLÜM 1: Protobuf Mesaj Oluşturma
// ============================================================
Console.WriteLine("━━━ BÖLÜM 1: Protobuf Mesaj Oluşturma ━━━");
Console.WriteLine();

// 📝 Proto dosyasından otomatik üretilen C# class'ı kullanıyoruz
// warehouse.proto'daki "message Product" → C# "Product" class'ı oldu
var product = new Product
{
    Id = 1,
    Name = "Wireless Mouse",
    Barcode = "4012345678901",
    Category = "Electronics",
    Quantity = 45,
    Price = 29.99,
    Location = "A-01-03",
    MinimumStock = 10,
    Status = StockStatus.InStock,
    LastUpdatedTicks = DateTime.Now.Ticks
    // 📝 snake_case (minimum_stock) → otomatik PascalCase (MinimumStock) oldu!
};

Console.WriteLine($"  Ürün: {product.Name}");
Console.WriteLine($"  Barkod: {product.Barcode}");
Console.WriteLine($"  Fiyat: €{product.Price:F2}");
Console.WriteLine($"  Stok: {product.Quantity} (min: {product.MinimumStock})");
Console.WriteLine($"  Durum: {product.Status}");
Console.WriteLine();

// ============================================================
// BÖLÜM 2: Serialize & Deserialize
// ============================================================
Console.WriteLine("━━━ BÖLÜM 2: Serialize & Deserialize ━━━");
Console.WriteLine();

// 📝 PROTOBUF SERIALIZE (Object → Binary)
byte[] protobufBytes = product.ToByteArray();
// ToByteArray() → Protobuf'un binary encoding'i
// Bu binary veri ağ üzerinden gönderilir veya dosyaya yazılır

Console.WriteLine($"  Protobuf binary boyutu: {protobufBytes.Length} bytes");
Console.WriteLine($"  Binary (hex): {BitConverter.ToString(protobufBytes[..Math.Min(50, protobufBytes.Length)])}...");
Console.WriteLine();

// 📝 PROTOBUF DESERIALIZE (Binary → Object)
var deserializedProduct = Product.Parser.ParseFrom(protobufBytes);
// Parser.ParseFrom() → Binary veriyi tekrar object'e çevir

Console.WriteLine($"  Deserialized: {deserializedProduct.Name} - €{deserializedProduct.Price:F2}");
Console.WriteLine($"  Eşit mi? {product.Equals(deserializedProduct)}");
Console.WriteLine();

// ============================================================
// BÖLÜM 3: JSON vs Protobuf Karşılaştırma
// ============================================================
Console.WriteLine("━━━ BÖLÜM 3: JSON vs Protobuf Boyut Karşılaştırması ━━━");
Console.WriteLine();

// Birden fazla ürünle test edelim
var productList = new ProductList { TotalCount = 100 };
for (int i = 0; i < 100; i++)
{
    productList.Products.Add(new Product
    {
        Id = i + 1,
        Name = $"Product {i + 1:D4}",
        Barcode = $"40{i:D11}",
        Category = i % 3 == 0 ? "Electronics" : i % 3 == 1 ? "Accessories" : "Furniture",
        Quantity = Random.Shared.Next(0, 500),
        Price = Math.Round(Random.Shared.NextDouble() * 100, 2),
        Location = $"{(char)('A' + i % 4)}-{i % 5:D2}-{i % 10:D2}",
        MinimumStock = 10,
        Status = (StockStatus)(i % 4),
        LastUpdatedTicks = DateTime.Now.Ticks
    });
}

// Protobuf boyutu
byte[] protobufListBytes = productList.ToByteArray();

// JSON boyutu (karşılaştırma için)
// 📝 Aynı veriyi JSON olarak serialize edelim
var jsonProducts = productList.Products.Select(p => new
{
    p.Id, p.Name, p.Barcode, p.Category, p.Quantity,
    p.Price, p.Location, p.MinimumStock,
    Status = p.Status.ToString(),
    LastUpdated = new DateTime(p.LastUpdatedTicks).ToString("o")
}).ToList();

string jsonString = JsonSerializer.Serialize(jsonProducts, new JsonSerializerOptions
{
    WriteIndented = false // Compact JSON
});
byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

Console.WriteLine($"  100 ürün için:");
Console.WriteLine($"  ┌─────────────┬──────────────┬──────────────┐");
Console.WriteLine($"  │ Format      │ Boyut        │ Oran         │");
Console.WriteLine($"  ├─────────────┼──────────────┼──────────────┤");
Console.WriteLine($"  │ JSON        │ {jsonBytes.Length,8:N0} B   │ %100         │");
Console.WriteLine($"  │ Protobuf    │ {protobufListBytes.Length,8:N0} B   │ %{(double)protobufListBytes.Length / jsonBytes.Length * 100:F1,-10} │");
Console.WriteLine($"  └─────────────┴──────────────┴──────────────┘");
Console.WriteLine($"  Protobuf {(double)jsonBytes.Length / protobufListBytes.Length:F1}x daha küçük!");
Console.WriteLine();

// ============================================================
// BÖLÜM 4: Hız Karşılaştırması
// ============================================================
Console.WriteLine("━━━ BÖLÜM 4: Hız Karşılaştırması ━━━");
Console.WriteLine();

const int iterations = 10_000;

// JSON Serialize hızı
var sw = Stopwatch.StartNew();
for (int i = 0; i < iterations; i++)
{
    JsonSerializer.Serialize(jsonProducts);
}
sw.Stop();
var jsonSerializeMs = sw.ElapsedMilliseconds;

// Protobuf Serialize hızı
sw.Restart();
for (int i = 0; i < iterations; i++)
{
    productList.ToByteArray();
}
sw.Stop();
var protoSerializeMs = sw.ElapsedMilliseconds;

// JSON Deserialize hızı
sw.Restart();
for (int i = 0; i < iterations; i++)
{
    JsonSerializer.Deserialize<List<object>>(jsonString);
}
sw.Stop();
var jsonDeserializeMs = sw.ElapsedMilliseconds;

// Protobuf Deserialize hızı
sw.Restart();
for (int i = 0; i < iterations; i++)
{
    ProductList.Parser.ParseFrom(protobufListBytes);
}
sw.Stop();
var protoDeserializeMs = sw.ElapsedMilliseconds;

Console.WriteLine($"  {iterations:N0} iterasyon ({productList.Products.Count} ürün):");
Console.WriteLine($"  ┌──────────────┬──────────────┬──────────────┐");
Console.WriteLine($"  │ İşlem        │ JSON         │ Protobuf     │");
Console.WriteLine($"  ├──────────────┼──────────────┼──────────────┤");
Console.WriteLine($"  │ Serialize    │ {jsonSerializeMs,8} ms  │ {protoSerializeMs,8} ms  │");
Console.WriteLine($"  │ Deserialize  │ {jsonDeserializeMs,8} ms  │ {protoDeserializeMs,8} ms  │");
Console.WriteLine($"  └──────────────┴──────────────┴──────────────┘");
Console.WriteLine();

// ============================================================
// BÖLÜM 5: Protobuf Özel Özellikler
// ============================================================
Console.WriteLine("━━━ BÖLÜM 5: Protobuf Özel Özellikler ━━━");
Console.WriteLine();

// 📝 repeated = List<T> — otomatik yönetilir
Console.WriteLine("  [repeated] → List<T> karşılığı:");
var update = new StockUpdate
{
    ProductId = 1,
    QuantityChange = -5,
    Reason = "Shipped to customer",
    TimestampTicks = DateTime.Now.Ticks,
    Type = StockUpdate.Types.UpdateType.Shipped
};
Console.WriteLine($"  StockUpdate: Product #{update.ProductId}, Change: {update.QuantityChange}, Type: {update.Type}");
Console.WriteLine();

// 📝 Default değerler — Protobuf'ta "0" ve "" gönderilmez (bandwidth tasarrufu)
var emptyProduct = new Product(); // Tüm alanlar default
byte[] emptyBytes = emptyProduct.ToByteArray();
Console.WriteLine($"  Boş Product boyutu: {emptyBytes.Length} bytes (default değerler gönderilmez!)");
Console.WriteLine();

// 📝 Protobuf JSON formatı — debug için
Console.WriteLine("  Protobuf → JSON (debug amaçlı):");
string protoJson = JsonFormatter.Default.Format(product);
Console.WriteLine($"  {protoJson[..Math.Min(200, protoJson.Length)]}...");
Console.WriteLine();

// ============================================================
// BÖLÜM 6: Ne Zaman Protobuf, Ne Zaman JSON?
// ============================================================
Console.WriteLine("━━━ BÖLÜM 6: Ne Zaman Hangisini Kullan? ━━━");
Console.WriteLine();
Console.WriteLine("  ┌────────────────────────┬────────────────────────┐");
Console.WriteLine("  │ PROTOBUF Kullan         │ JSON Kullan            │");
Console.WriteLine("  ├────────────────────────┼────────────────────────┤");
Console.WriteLine("  │ Mikroservisler arası    │ REST API (public)      │");
Console.WriteLine("  │ Yüksek performans       │ Web frontend iletişimi │");
Console.WriteLine("  │ Büyük veri transferi    │ Config dosyaları       │");
Console.WriteLine("  │ Mobile <-> Backend      │ Debug/test kolaylığı   │");
Console.WriteLine("  │ IoT cihazları           │ 3rd party entegrasyon  │");
Console.WriteLine("  │ Real-time streaming     │ Küçük veri miktarları  │");
Console.WriteLine("  │ gRPC servisleri         │ Human-readable gerekli │");
Console.WriteLine("  └────────────────────────┴────────────────────────┘");
Console.WriteLine();

Console.WriteLine("✅ Protobuf Demo tamamlandı!");
