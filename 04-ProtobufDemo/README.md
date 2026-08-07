# 04 - Protocol Buffers (Protobuf) Demo

Binary serialization formatı ve gRPC temelleri.

## 🎯 Bu Projede Öğrenilen Kavramlar

| Kavram | Açıklama |
|--------|----------|
| `.proto` dosyası | Veri şeması tanımlama |
| `message` | Veri yapısı (C# class gibi) |
| `enum` | Sabit değerler |
| `repeated` | Liste/koleksiyon (`List<T>`) |
| `service` | gRPC servis tanımı |
| Serialize | Object → Binary |
| Deserialize | Binary → Object |
| JSON karşılaştırma | Boyut ve hız farkları |

## ▶️ Çalıştırma

```bash
cd 04-ProtobufDemo
dotnet run
```

## 📊 Beklenen Çıktı

- Protobuf JSON'dan ~2-3x daha küçük boyut
- Protobuf JSON'dan ~5-10x daha hızlı serialize/deserialize
- Binary format (human-readable değil)

## 📝 Notlar

- `Grpc.Tools` NuGet paketi `.proto` dosyalarını otomatik C# class'larına derler
- Build sırasında `obj/` altında üretilen kodu görebilirsin
- gRPC servisi çalıştırmak için ayrı bir server projesi gerekir (bu demo'da sadece mesaj tanımları var)
