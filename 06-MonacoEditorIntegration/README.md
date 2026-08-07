# 06 - Monaco Editor Integration

VS Code'un temelini oluşturan Monaco Editor'ün web sayfasına entegrasyonu.

## 🎯 Bu Projede Öğrenilen Kavramlar

| Kavram | Açıklama |
|--------|----------|
| Monaco Editor | VS Code'un web editör engine'i |
| AMD Loader | RequireJS ile modül yükleme |
| Custom Language | Özel dil tanımlama (AL) |
| Monarch Tokenizer | Syntax highlighting kuralları |
| CompletionItemProvider | IntelliSense / auto-complete |
| Themes | vs-dark, vs, hc-black |

## ▶️ Çalıştırma

Basit bir HTTP server ile:
```bash
cd 06-MonacoEditorIntegration
# Python ile
python -m http.server 8080
# veya Node.js ile
npx serve .
```

Tarayıcıda: http://localhost:8080

## 📝 Özellikler

- ✅ AL dili için custom syntax highlighting
- ✅ AL snippet'leri ile IntelliSense
- ✅ Tema değiştirme (Dark / Light / High Contrast)
- ✅ Dil değiştirme (AL, C#, JSON, JavaScript, HTML)
- ✅ Örnek kod seçici
- ✅ Minimap toggle
- ✅ Cursor pozisyon gösterimi
- ✅ VS Code kısayolları
