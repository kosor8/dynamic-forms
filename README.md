# Dinamik Form ve Anket Oluşturma Motoru (Dynamic Forms Engine)

Bu proje, teknik bilgisi olmayan kullanıcıların görsel bir arayüz (UI) üzerinden sürükle-bırak yöntemiyle kendi formlarını, anketlerini veya veri toplama ekranlarını oluşturabilmesini sağlayan, **Google Forms** mantığına benzer bir **Windows Forms (C#)** uygulamasıdır.

## 🚀 Özellikler

- **Gelişmiş Sürükle-Bırak (Drag & Drop) Tasarımcısı:** 
  Sol taraftaki araç kutusundan istenen soru tipini sürükleyip sağdaki tasarım alanına bırakarak anında form oluşturulabilir. Mavi hedef çizgisi (placeholder) ile soruların tam yerleşeceği konum önceden görüntülenir.
- **Kapsamlı Soru Türleri:**
  - 📝 **Metin Bloğu:** Açıklama ve bilgi alanları (veri almaz).
  - 💬 **Açık Uçlu Soru:** Kısa yanıt veya uzun paragraf girişleri.
  - ☑️ **Seçmeli Soru:** Çoktan seçmeli (Radio), Onay kutusu (Checkbox) veya Açılır menü (Dropdown).
  - 📊 **Ölçeklendirme:** Kullanıcı tanımlı min-max değerleri olan, yatayda otomatik ortalanan doğrusal ölçek (Linear Scale).
  - 🧮 **Seçme Tablosu:** Sürükle-bırak destekli çoktan seçmeli veya onay kutulu karmaşık ızgara (Grid) soruları.
  - 📅 **Zaman:** Tarih veya Saat seçici.
  - 📎 **Dosya Yükleme:** Cihazdan dosya dizini seçme işlemi.
- **Dinamik Özellikler Paneli:**
  Tasarım alanından bir soruya tıklandığında, sağ taraftaki panelde o soruya ait özellikler (Soru Tipi Değiştirme, Satır/Sütun veya Seçenek Ekleme/Silme, Zorunluluk durumu) anlık olarak yönetilebilir.
- **Tam Duyarlı (Responsive) "Kart" Görünümü:**
  Eklenen sorular standart bir genişliğe hapsolmaz, uygulama penceresinin boyutuna göre otomatik genişleyip daralan şık çerçeveli kartlar (bloklar) halinde görünür.
- **Veritabanı ve Dışa Aktarma:**
  - **SQLite:** Modern EAV (Entity-Attribute-Value) modeli ile birlikte, karmaşık soru matrisleri C# `System.Text.Json` serileştirmesi kullanılarak kaydedilir ve yüklenir.
  - **JSON & XML Dışa Aktarma:** Oluşturduğunuz form şablonlarını anında JSON veya XML olarak yedekleyebilirsiniz.
- **Önizleme ve Yanıt Görüntüleme:**
  "Önizleme" sekmesinden formu test edip kaydedebilir, "Yanıtlar" sekmesinden daha önce doldurulmuş formların sonuçlarını inceleyebilirsiniz.

## 🛠️ Kurulum ve Çalıştırma

### Gereksinimler
- **.NET 9.0 SDK** veya üzeri (Projeyi derlemek için).
- **Microsoft.Data.Sqlite** (Proje içerisinde NuGet bağımlılığı olarak bulunmaktadır).

### Çalıştırma Adımları

1. Repoyu bilgisayarınıza klonlayın veya indirin.
2. Proje dizinine gidin (Terminal veya Komut Satırı üzerinden).
3. Gerekli paketleri indirmek ve projeyi çalıştırmak için aşağıdaki komutları kullanın:

```bash
dotnet build
dotnet run
```

4. Visual Studio 2022 veya daha güncel bir IDE kullanıyorsanız, `dynamic-forms.csproj` dosyasına çift tıklayıp doğrudan **F5** tuşu ile çalıştırabilirsiniz.

## 💡 Nasıl Kullanılır?

1. **Form Başlığı:** Sol üst kısımdan formunuzun ana başlığını belirleyin.
2. **Soru Ekleme:** Sol paneldeki araç kutusundan bir soru butonuna fareyle **basılı tutarak** orta boş alana sürükleyin.
3. **Soruyu Düzenleme:** Ortadaki sorunun üzerine tıklayın. Sağ panelde o sorunun ayarları belirecektir.
   - *Seçmeli Soru* eklediyseniz, sağ panelden "Seçenek 2", "Seçenek 3" ekleyebilir ve tiplerini Onay Kutusu olarak değiştirebilirsiniz.
   - *Seçme Tablosu* eklediyseniz, sağ panelden tablonun Satır (Row) ve Sütun (Column) değerlerini artırıp silebilirsiniz.
4. **Kaydetme:** "Formu Veritabanına Kaydet" butonu ile form yapınızı güvenceye alın.
5. **Doldurma:** Üst menüden **Önizleme** sekmesine geçin, formunuzu doldurup gönderin.
6. **Sonuçlar:** **Yanıtlar** sekmesine geçerek açılır menüden formunuzu seçip toplanan verileri anında tabloda görüntüleyin.

## 🏗️ Mimari ve Teknolojiler

- **Dil:** C# (Windows Forms)
- **Veritabanı:** SQLite
- **Tasarım Deseni:** Olay Yönelimli (Event-Driven), Kalıtım ve Polimorfizm ile Dinamik Bileşen Üretimi (Polymorphic Form Elements).
- **Serileştirme:** `System.Text.Json` (JsonDerivedType nitelikleri kullanılarak alt sınıfların eksiksiz depolanması).

## 📄 Lisans
Bu proje açık kaynaklı olup, kişisel veya eğitim amaçlı geliştirme süreçleri için serbestçe kullanılabilir.
