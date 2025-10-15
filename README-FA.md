<div dir="rtl">

# 🚀 Uprix Assistant

<div align="center">

![Version](https://img.shields.io/badge/version-3.0.0.0-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)
![.NET](https://img.shields.io/badge/.NET-Framework%204.8-512BD4.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

**[English](./README-EN.md)** | **فارسی**

### دستیار هوشمند دسکتاپ برای مدیریت برنامه‌ها، بوک‌مارک‌ها و وظایف

<img src="docs/images/banner.png" alt="Uprix Banner" width="800"/>

[دانلود نسخه آخر](https://github.com/MehranQadirian/Uprix-Application/releases/latest) • [مستندات](docs/README-FA.md) • [گزارش مشکل](https://github.com/MehranQadirian/Uprix-Application/issues)

</div>

---

## 📋 فهرست مطالب

- [درباره پروژه](#-درباره-پروژه)
- [ویژگی‌ها](#-ویژگی‌ها)
- [نمایش تصویری](#-نمایش-تصویری)
- [نصب و راه‌اندازی](#-نصب-و-راه‌اندازی)
- [راهنمای استفاده](#-راهنمای-استفاده)
- [تکنولوژی‌های استفاده شده](#️-تکنولوژی‌های-استفاده-شده)
- [ساختار پروژه](#-ساختار-پروژه)
- [مشارکت](#-مشارکت)
- [نقشه راه](#️-نقشه-راه)
- [لایسنس](#-لایسنس)
- [تماس و پشتیبانی](#-تماس-و-پشتیبانی)

---

## 🎯 درباره پروژه

**Uprix Assistant** یک برنامه دسکتاپ قدرتمند و مدرن است که برای مدیریت هوشمند برنامه‌ها، بوک‌مارک‌های مرورگر و وظایف روزانه طراحی شده است. این برنامه با رابط کاربری زیبا و شهودی، تجربه‌ای روان و کارآمد را برای کاربران فراهم می‌کند.

### 🎨 ویژگی‌های کلیدی

- **مدیریت هوشمند برنامه‌ها** با موتور جستجوی پیشرفته
- **همگام‌سازی بوک‌مارک‌ها** از تمام مرورگرهای محبوب
- **مدیریت وظایف حرفه‌ای** با پشتیبانی از پروژه‌ها و زیروظایف
- **سیستم تم‌بندی پیشرفته** با 10 تم نئونی مختلف
- **آپدیت خودکار** از GitHub Releases

---

## ✨ ویژگی‌ها

### 🚀 مدیریت برنامه‌ها (App Launcher)

<details open>
<summary><b>کلیک کنید برای مشاهده جزئیات</b></summary>

- ✅ **شناسایی خودکار** برنامه‌های نصب شده از Start Menu و Registry
- 🔍 **جستجوی هوشمند** با الگوریتم‌های:
  - Levenshtein Distance
  - Damerau-Levenshtein Distance
  - Jaro-Winkler Similarity
  - N-Gram Similarity (Trigram)
- ⭐ **علامت‌گذاری مورد علاقه** برنامه‌ها
- 📊 **ردیابی میزان استفاده** و مرتب‌سازی بر اساس Rate
- 🎯 **فیلتر پیشرفته** بر اساس نام، دسته‌بندی و...
- 🔄 **بارگذاری Lazy** با انیمیشن Skeleton Loader

</details>

### 🔖 مدیریت بوک‌مارک‌ها (Bookmark Manager)

<details>
<summary><b>کلیک کنید برای مشاهده جزئیات</b></summary>

- 🌐 **پشتیبانی کامل** از مرورگرها:
  - Google Chrome
  - Microsoft Edge
  - Mozilla Firefox
  - مرورگرهای سفارشی
- 🔄 **استخراج خودکار** بوک‌مارک‌ها
- 📝 **ویرایش و مدیریت** بوک‌مارک‌ها
- ⭐ **نشانه‌گذاری مورد علاقه**
- 🎯 **سیستم Rate** با 4 سطح:
  - Normal (سبز)
  - Warning (نارنجی)
  - Critical (قرمز)
  - Emergency (قرمز تیره)
- 🔀 **Drag & Drop** بین مرورگرها
- 📊 **آمار استفاده** از هر بوک‌مارک
- 🗂️ **دسته‌بندی** بر اساس مرورگر

</details>

### ✅ مدیریت وظایف (Task Manager)

<details>
<summary><b>کلیک کنید برای مشاهده جزئیات</b></summary>

- 📋 **ایجاد و مدیریت** وظایف و پروژه‌ها
- 🔗 **زیروظایف** (Subtasks) نامحدود
- 📊 **نمودار پیشرفت** به صورت Real-time
- 🎨 **رنگ‌بندی پروژه‌ها** با 6 رنگ مختلف
- 🏷️ **تگ‌گذاری** وظایف
- 📅 **تاریخ سررسید** با هشدار Overdue
- 🎯 **اولویت‌بندی** با 4 سطح:
  - Low
  - Medium
  - High
  - Critical
- 📱 **تولید QR Code** برای هر وظیفه
- 📊 **گزارش روزانه** با آمار کامل
- 🔍 **فیلتر و جستجو** پیشرفته
- 💾 **ذخیره‌سازی محلی** با LiteDB
- 📤 **Export به CSV**

</details>

### 🎨 سیستم تم‌بندی

<details>
<summary><b>کلیک کنید برای مشاهده جزئیات</b></summary>

10 تم نئونی زیبا:
- 🔵 **Neon Cyan** (پیش‌فرض)
- 🟢 **Neon Green**
- 🟡 **Neon Yellow**
- 🟠 **Neon Orange**
- 🔴 **Neon Red**
- 🟣 **Neon Purple**
- 🩷 **Neon Pink**
- 🔷 **Neon Cobalt Blue**
- 🌊 **Neon Teal**
- 🟨 **Neon Gold**

</details>

### 🔄 سیستم آپدیت

<details>
<summary><b>کلیک کنید برای مشاهده جزئیات</b></summary>

- ✅ **بررسی خودکار** نسخه‌های جدید
- 📦 **دانلود و نصب** به صورت خودکار
- 📊 **نمایش پیشرفت** دانلود
- 🔔 **نوتیفیکیشن** برای نسخه‌های جدید

</details>

---

## 🖼️ نمایش تصویری

<div align="center">

### صفحه اصلی
<img alt="Main Screen" width="700" src="https://github.com/user-attachments/assets/22c64124-e0b2-4718-b235-554211a3d2eb" />

### مدیریت بوک‌مارک‌ها
<img alt="Bookmark Manager" width="700" src="https://github.com/user-attachments/assets/49976dc4-3071-4cdf-af19-14daef199e6f" />

### مدیریت وظایف
<img alt="Task Manager" width="700" src="https://github.com/user-attachments/assets/4f4b341a-061c-404d-901a-3c454cc8595b" />

### تنظیمات و تم‌ها
<img alt="Settings" width="700" src="https://github.com/user-attachments/assets/dea91bdf-b95a-4826-8930-5d35e11b0d44" />


</div>

---

## 🔧 نصب و راه‌اندازی

### پیش‌نیازها

- Windows 10/11 (64-bit)
- .NET Framework 4.8 یا بالاتر
- حداقل 100MB فضای خالی
- 
### 📦 وابستگی‌ها

پروژه به صورت خودکار پکیج‌های زیر را دانلود می‌کند:
```xml
<PackageReference Include="LiteDB" Version="5.0.11" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
<PackageReference Include="QRCoder" Version="1.4.3" />
<PackageReference Include="System.Data.SQLite.Core" Version="1.0.115" />
```

---

## 📖 راهنمای استفاده

### 🚀 App Launcher

#### جستجوی برنامه‌ها
```plaintext
1. در کادر جستجو شروع به تایپ کنید
2. از کلیدهای جهت‌دار ↑↓ برای حرکت استفاده کنید
3. Enter را بزنید یا دو بار کلیک کنید
```

#### علامت‌گذاری مورد علاقه
```plaintext
1. روی آیکون ستاره ⭐ کلیک کنید
2. برنامه در بالای لیست نمایش داده می‌شود
```

### 🔖 Bookmark Manager

#### افزودن بوک‌مارک
```plaintext
1. دکمه "+ Add Bookmark" را بزنید
2. اطلاعات را وارد کنید:
   - Browser: انتخاب مرورگر
   - Title: عنوان بوک‌مارک
   - URL: آدرس وب‌سایت
   - Rate: سطح اهمیت
   - Favorite: علامت‌گذاری
3. دکمه Save را بزنید
```

#### انتقال بین مرورگرها
```plaintext
1. بوک‌مارک را Drag کنید
2. روی تب مرورگر مقصد Drop کنید
3. بوک‌مارک به مرورگر جدید اضافه می‌شود
```

### ✅ Task Manager

#### ایجاد وظیفه
```plaintext
1. دکمه "+ Add Task" را بزنید
2. اطلاعات را وارد کنید:
   - Title: عنوان وظیفه
   - Description: توضیحات
   - Project: انتخاب پروژه
   - Priority: اولویت
   - Status: وضعیت
   - Due Date: تاریخ سررسید
   - Tags: برچسب‌ها (جدا شده با کاما)
3. دکمه Save را بزنید
```

#### افزودن زیروظیفه
```plaintext
1. روی دکمه "+ Add Subtask" در کارت وظیفه کلیک کنید
2. اطلاعات زیروظیفه را وارد کنید
3. زیروظیفه به لیست اضافه می‌شود
```

#### تولید QR Code
```plaintext
1. روی آیکون 📱 در کارت وظیفه کلیک کنید
2. QR Code نمایش داده می‌شود
3. با موبایل اسکن کنید
```

---

## 🛠️ تکنولوژی‌های استفاده شده

### فریمورک‌ها و کتابخانه‌ها

| تکنولوژی | نسخه | کاربرد |
|---------|------|---------|
| **WPF** | .NET Framework 4.8 | رابط کاربری |
| **C#** | 10.0 | زبان برنامه‌نویسی |
| **LiteDB** | 5.0.11 | دیتابیس NoSQL |
| **Newtonsoft.Json** | 13.0.1 | پردازش JSON |
| **QRCoder** | 1.4.3 | تولید QR Code |
| **SQLite** | 1.0.115 | پردازش Firefox DB |
| **IWshRuntimeLibrary** | - | پردازش Shortcut |

### الگوهای طراحی

- **MVVM** (Model-View-ViewModel)
- **Repository Pattern** برای دسترسی به دیتا
- **Service Layer** برای لجیک تجاری
- **Dependency Injection** برای مدیریت وابستگی‌ها


## 🤝 مشارکت

مشارکت شما در توسعه این پروژه بسیار ارزشمند است! لطفاً این مراحل را دنبال کنید:

### روش مشارکت

1️⃣ **Fork** کردن پروژه

2️⃣ ایجاد **Branch** جدید:
```bash
git checkout -b feature/AmazingFeature
```

3️⃣ **Commit** کردن تغییرات:
```bash
git commit -m 'Add some AmazingFeature'
```

4️⃣ **Push** به Branch:
```bash
git push origin feature/AmazingFeature
```

5️⃣ ایجاد **Pull Request**

## 📞 تماس و پشتیبانی

<div align="center">

### راه‌های ارتباطی

| پلتفرم | لینک |
|--------|------|
| 📧 Email | [mehranghadirian01@gmail.com](mailto:mehranghadirian01@gmail.com) |
| 💬 Telegram | [@UprixApplication](https://t.me/UprixApplication) |
| 🐙 GitHub | [@MehranQadirian](https://github.com/MehranQadirian) |
| 🌐 Website | [Lumora Flow](https://lumora-flow.pages.dev) |

### پشتیبانی پروژه

اگر این پروژه برای شما مفید بود، می‌توانید:

⭐ **Star** دادن به پروژه

🐛 گزارش **Issue**

💡 پیشنهاد **Feature** جدید

🤝 **مشارکت** در توسعه

</div>

---

<div align="center">

### 💝 حمایت از پروژه

اگر از این پروژه لذت بردید، لطفاً:

[![GitHub Stars](https://img.shields.io/github/stars/MehranQadirian/Uprix-Application?style=social)](https://github.com/MehranQadirian/Uprix-Application)
[![GitHub Forks](https://img.shields.io/github/forks/MehranQadirian/Uprix-Application?style=social)](https://github.com/MehranQadirian/Uprix-Application/fork)

---

ساخته شده با ❤️ توسط [Mehran Qadirian](https://github.com/MehranQadirian)

**© 2025 Uprix Assistant. All rights reserved.**

</div>

</div>
