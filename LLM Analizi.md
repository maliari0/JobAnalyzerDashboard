# LLM Analizi

JobAnalyzerDashboard projesi için yapılan değerlendirmede, **ücretsiz kullanım** imkânı sunan başlıca üç seçenek; **Mistral NeMo**, **OpenRouter** ve **Gemini 2.0 Flash** olarak öne çıkmaktadır.

Üretim aşamasında ise **Mistral NeMo** ve **Gemini 1.5 Flash-8B** modelleri en düşük birim token maliyetini (sırasıyla $0.30 ve $0.1875/1M token) sunarken, Türkçe e-posta oluşturma gibi yüksek dil kalitesi gerektiren görevlerde **GPT-4.1 mini** ($2.00/1M token) ve **Anthropic Claude 3.5 Haiku** ($4.80/1M token) tercih edilebilir.

Gizlilik ve offline senaryolar için **Ollama** self-hosted yöntemi uygun bir alternatiftir.

# 1. Ücretsiz Kullanım Seçenekleri

### 1.1 Mistral Cloud Chat

- **Kota:** Aylık 1 milyar token, 1 istek/saniye, 500 000 token/dk
- **Modeller:** Mistral NeMo, Mistral Small, Mistral Large.
- **Entegrasyon:** n8n’de yerel “Mistral Cloud Chat” düğümü.
- **Kullanım Alanları:** JSON çıkarımı, kategori etiketleme, kalite puanlama.

### 1.2 OpenRouter

- **Kota:** Günlük 50 istek; 10 kredi satın alındığında günlük 1 000 istek
- **Modeller:** Claude, Mistral, LLaMA, Qwen vb.
- **Entegrasyon:** n8n’de yerel “OpenRouter” düğümü.
- **Kullanım Alanları:** Model çeşitliliği, esneklik.

### 1.3 Ollama (Self-Hosted)

- **Lisans:** Açık kaynak, ücretsiz.
- **Maliyet:** API ücreti yok; yalnızca altyapı kaynağı gerektirir.
- **Modeller:** LLaMA 2/3, Mistral, Qwen, Gemma vb.
- **Kullanım Alanları:** Gizlilik öncelikli, offline işlemler.

## 2. Fiyat/Performans Karşılaştırması

| Model | Girdi Maliyeti (1 M Token) | Çıktı Maliyeti (1 M Token) | Toplam Maliyet (1 M Token) | Notlar | Kaynaklar |
| --- | --- | --- | --- | --- | --- |
| **Mistral NeMo** | $0.15 | $0.15 | **$0.30** | Düşük birim maliyet, güçlü çokdilli performans | ([Mistral AI](https://mistral.ai/news/september-24-release)) |
| **GPT-3.5 Turbo** | $0.50  | $1.50   | **$2.00** | Yüksek tutarlılık, n8n entegrasyonu kolay (Fakat eski) | ([OpenAI](https://platform.openai.com/docs/pricing)) |
| **GPT-4.1-mini** | $0.40 | $1.60 | $2.00 | Yüksek tutarlılık, n8n entegrasyonu kolay | ([OpenAI](https://platform.openai.com/docs/pricing)) |
| **GPT-4.1-nano** | $0.10 | $0.40 | **$0.50** | Düşük maliyet, dil desteği | ([OpenAI](https://platform.openai.com/docs/pricing)) |
| **DeepSeek Reasoner** | $0.14  (cache hit) | $2.19 | **$2.33** | Cache avantajıyla düşük girdi maliyeti; çıktı maliyeti orta düzeyde | ([DeepSeek API Docs](https://api-docs.deepseek.com/quick_start/pricing-details-usd), [Team-GPT](https://team-gpt.com/blog/deepseek-pricing/)) |
| **Gemini 1.5 Flash-8B** | $0.0375 | $0.15 | **$0.1875** | En düşük maliyetli Gemini versiyonu; 1 M token  | ([Google AI for Developers](https://ai.google.dev/gemini-api/docs/pricing)) |
| **Gemini 2.0 Flash** | $0.10 | $0.40 | **$0.50** | 1 M token, dengeli multimodal performans, Google search özelliği | [(Google AI for Developers)](https://ai.google.dev/gemini-api/docs/pricing) |
| **Gemini 2.0 Flash-Lite** | $0.075 | $0.30 | **$0.375** | Düşük gecikme, verimli; 1 M token, Google Search yok. | [(Google Cloud)](https://cloud.google.com/vertex-ai/generative-ai/pricing) |
| **Gemini 2.5 Flash** | $0.15 | $0.60 | **$0.75** | Düşük gecikme, 1 M token, Google Search özelliği | ([Google AI for Developers](https://ai.google.dev/gemini-api/docs/pricing)) |
| **Gemini 2.5 Pro** | $1.25 | $10.00 | **$11.25** | Yüksek doğruluk, 2 M token bağlam; yüksek çıktı maliyeti | ([Google AI for Developers](https://ai.google.dev/gemini-api/docs/pricing)) |
| **Anthropic Claude 3.5 Haiku** | $0.80 | $4.00  | **$4.80** | Üst düzey kalite ve tutarlılık; 200 K token pencere | ([Anthropic](https://www.anthropic.com/pricing)) |

## 3. Önerilen Model Seçimleri ve Kullanım Senaryoları

### 3.1 JSON Çıkarma & Etiketleme

- **Önerilen Model:** Mistral NeMo (open-mistral-nemo-2407)
- **Alternatif:** Gemini 1.5 Flash-8B (Düşük Maliyet), Gemini 2.0 Flash

### 3.2 Kalite Puanlama & Kategori Atama

- **Önerilen Model:** Mistral NeMo
- **Alternatif:** Gemini 2.0 Flash

### 3.3 Başvuru E-Postası Üretimi (Türkçe)

- **Önerilen Model:** GPT-4.1 mini veya nano
- **Alternatif:** Anthropic Claude 3.5 Haiku

### 3.4 Tamamen Offline / Gizlilik Gerektiren

- **Önerilen Model:** Ollama Self-Hosted

## 4. Ollama Self-Hosted İçin Donanım Gereksinimleri

| Model Boyutu | Minimum GPU Belleği | Önerilen GPU | CPU Alternatifi | Notlar |
| --- | --- | --- | --- | --- |
| 7 B | 8 GB | NVIDIA RTX 3060 | 16 çekirdekli CPU | Düşük gecikme için GPU tercih edilmeli |
| 13 B | 16 GB | NVIDIA RTX 3090 | 32 çekirdekli CPU | Daha yüksek performans için güçlü GPU gerek |
| 70 B | 48 GB | NVIDIA A100 / H100 | N/A | Veri merkezleri için uygun, maliyetli |
- **Depolama:** SSD, 10–100 GB (model boyutuna göre)
- **RAM:** Minimum 16 GB; ideal 32 GB+
- **İşletim Sistemi:** Linux (Ubuntu 20.04+)
- **Diğer:** CUDA, PyTorch kurulumu gereklidir.

## 5. Sonuç ve Yol Haritası

- **Ücretsiz Katmanlar:** Mistral Cloud Chat ve OpenRouter ile prototip geliştirmeyi denemek gerek. Ortaya güzel bir karışım çıkabilir.
- **Maliyet/Performans Dengesi:** Üretimde Mistral NeMo veya Gemini 1.5 Flash-8B kullanılabilir. Mevcut 2.0 Flash sürümü 1.5 Flash-8B’den daha iyi. Ama prompt mühendisliği ile sıkı bir prompt yazılırsa Gemini 1.5 Flash-8B sayesinde çok daha az maliyetle halledebiliriz.
Dipnot: Mistral Nemo token bağlam penceresi: 98.304 (Bazı kaynaklar 128k demektedir) olduğu için sorun olabilir.
- **Yüksek Dil Kalitesi Gerektirenler:** Türkçe e-posta üretiminde GPT-3.5 Turbo veya GPT-4.1-mini veya Claude 3.5 Haiku tercih edilebilir. Çokdilli performansı yüksek olduğu için Mistral NeMo da tercihler arasında yer alabilir.
- **Gizlilik Gerektiren Senaryolar:** Ollama self-hosted ile offline çözüm denenebilir.

NOT: OpenRouter tek bir yerden birçok LLM modeline erişmemize olanak sağlıyor. Aynı anda hem mistral, hem openai hem de direkt olarak n8n üzerinde var olmayan perplexity, nvidia llama gibi modellere de erişim sağlayabiliyoruz. Pay-as-go yaklaşımı var.
Kaynak: https://openrouter.ai/models?fmt=table