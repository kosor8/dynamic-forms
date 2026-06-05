

## 1. Proje Adı
Dinamik Form ve Anket Oluşturma Motoru

## 2. Grup Üyeleri

## 032390068 Eren Boylu
## 032390048 Arda Berat Kosor
## 032390049 Mustafa Ozan Aydın

## 3. Projenin Amacı
Bu projenin temel amacı, teknik bilgisi olmayan kullanıcıların dahi görsel bir arayüz
üzerinden sürükle-bırak yöntemiyle karmaşık form ve anket yapıları oluşturabileceği
bir masaüstü uygulaması geliştirmektir. Google Forms mantığını masaüstü ortamına
taşıyan bu uygulama, esnek veri saklama yöntemleri kullanarak veritabanı şemasını
değiştirmeden sınırsız türde veri alanı eklenmesine olanak tanır.

Proje, özellikle dinamik kolon yönetimi ve esnek veritabanı tasarımı konularında
derinlemesine bir öğrenim çıktısı hedeflemektedir.



## 4. Teknik Detaylar
## 4.1. Kullanılan Windows Forms Kontrolleri
● Panel & FlowLayoutPanel: Tasarım alanının ve sürüklenen öğelerin
düzenlenmesi için.

● Label, TextBox, CheckBox, RadioButton: Formun temel girdi bileşenleri
olarak.
● Button: Form kaydetme, yayınlama ve silme işlemleri için.
● ListBox / DataGridView: Toplanan verilerin ve mevcut formların listelenmesi
için.
● TabControl: Tasarım modu ve önizleme modu arasında geçiş yapmak için.

4.2. Kullanılan Sınıflar (Mimari Yapı)
● FormElement: Tüm form bileşenlerinin türediği temel base class.
● FormManager: Formların oluşturulması, silinmesi ve listelenmesi mantığını
yöneten sınıf.
● DatabaseEngine: EAV modeline uygun olarak veritabanı bağlantılarını ve
sorgularını yöneten sınıf.
● ValidationProvider: Form elemanlarının zorunluluk ve tip kontrolünü yapan
yardımcı sınıf.

## 4.3. Kullanılan Collection Yapıları
● List<FormElement>: Aktif tasarım alanındaki elemanları bellekte tutmak için.
● Dictionary<string, object>: Form verilerini (key-value pair olarak) geçici
olarak saklamak ve işlemek için.

## 4.4. Kullanılan Eventler
● MouseDown / MouseMove / MouseUp: Sürükle-bırak mekanizmasını
gerçekleştirmek için.
● Click: Bileşen seçimi ve ayarların açılması için.
● TextChanged: Dinamik başlık güncellemeleri için.
● Custom Events: Form elemanı eklendiğinde veya silindiğinde UI'ı tetikleyen
özel olaylar.

## 5. Veri Saklama Yöntemi

Projede Entity-Attribute-Value (EAV) Modeli kullanılmaktadır. Bu model sayesinde:
Her yeni soru/alan için veritabanında yeni bir kolon açılması gerekmez. Veriler
"Entity" (Form ID), "Attribute" (Soru Başlığı) ve "Value" (Cevap) şeklinde üç
temel kolonda saklanır. Bu yapı, sistemin ölçeklenebilirliğini artırır ve kullanıcıya
sınırsız esneklik sunar.


## 7. Final Teslimine Kadar Yapılacaklar

● Sürükle-bırak mekanizmasındaki koordinat hatalarının giderilmesi.
● EAV modelinin SQL Server veya SQLite entegrasyonunun tamamlanması.

● Oluşturulan formların bir dosya formatında (.xml veya .json) dışa
aktarılabilmesi.
● Kullanıcı geri bildirimleri için validasyon (doğrulama) kontrollerinin eklenmesi.
● Arayüzün kullanıcı dostu modern ikonlarla güncellenmesi.

