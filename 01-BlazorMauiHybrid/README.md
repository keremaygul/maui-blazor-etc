# 01 - Blazor MAUI Hybrid: Stock Tracker App

📦 Depo/stok yönetimi için bir MAUI Blazor Hybrid uygulaması.

## 🎯 Bu Projede Öğrenilen Kavramlar

### Blazor Temel Kavramlar
| Kavram | Açıklama | Dosya |
|--------|----------|-------|
| **Razor Components** | `.razor` dosyaları = HTML + C# birleşimi | Tüm `.razor` dosyaları |
| **@page directive** | Component'i bir URL'e bağlar | `Home.razor`, `ProductList.razor` |
| **@code bloğu** | Component'in C# kodu | Tüm sayfa dosyaları |
| **Data Binding** | `@bind` ile two-way veri bağlama | `ProductForm.razor` |
| **Event Handling** | `@onclick`, `@onkeyup` gibi event'ler | `ProductList.razor`, `Scanner.razor` |
| **Dependency Injection** | `@inject` ile servis kullanma | Tüm sayfalar |
| **Route Parameters** | `{Id:int}` ile URL'den parametre alma | `ProductForm.razor` |
| **Conditional Rendering** | `@if`, `@foreach` ile koşullu render | `Home.razor` |
| **NavigationManager** | Programmatic sayfa yönlendirme | `ProductForm.razor` |
| **CSS Isolation** | Component bazlı CSS | Layout dosyaları |

### MAUI Kavramlar
| Kavram | Açıklama | Dosya |
|--------|----------|-------|
| **MauiApp Builder** | MAUI uygulama konfigürasyonu | `MauiProgram.cs` |
| **BlazorWebView** | MAUI içinde Blazor render etme | `MainPage.xaml` |
| **Service Registration** | DI container'a servis ekleme | `MauiProgram.cs` |

### Depo/Lojistik Kavramları
- Ürün yönetimi (CRUD)
- Barkod tarama simülasyonu (EAN-13)
- Stok seviyesi takibi (InStock / LowStock / OutOfStock)
- Depo lokasyon sistemi (Regal-Reihe-Fach)

## 📁 Proje Yapısı

```
01-BlazorMauiHybrid/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor       ← Ana sayfa layout'u
│   │   └── NavMenu.razor          ← Sol menü navigasyonu
│   ├── Pages/
│   │   ├── Home.razor             ← Dashboard (stok özeti)
│   │   ├── ProductList.razor      ← Ürün listesi (arama/filtre/sıralama)
│   │   ├── ProductForm.razor      ← Ürün ekleme/düzenleme formu
│   │   └── Scanner.razor          ← Barkod tarayıcı simülasyonu
│   ├── Routes.razor               ← Router konfigürasyonu
│   └── _Imports.razor             ← Global using/import'lar
├── Models/
│   └── Product.cs                 ← Ürün veri modeli
├── Services/
│   └── StockService.cs            ← İş mantığı servisi (in-memory)
├── MauiProgram.cs                 ← MAUI + Blazor konfigürasyonu
├── MainPage.xaml                  ← MAUI ana sayfası (BlazorWebView)
└── wwwroot/
    ├── app.css                    ← Global stiller (dark theme)
    └── index.html                 ← Blazor host HTML
```

## ▶️ Çalıştırma

```bash
cd 01-BlazorMauiHybrid
dotnet build
dotnet run
```

## 📝 Notlar

- Veriler in-memory tutulur (uygulama kapanınca sıfırlanır)
- Barkod tarayıcı simülasyondur — gerçek kamera erişimi için JS Interop gerekir
- Dark theme kullanılmıştır
