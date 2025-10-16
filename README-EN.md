# 🚀 Uprix Assistant

<div align="center">

![Version](https://img.shields.io/badge/version-3.0.0.0-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)
![.NET](https://img.shields.io/badge/.NET-Framework%204.8-512BD4.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

**English** | **[فارسی](./README.md)**

### Smart Desktop Assistant for Managing Applications, Bookmarks, and Tasks

<img src="docs/images/banner.png" alt="Uprix Banner" width="800"/>

[Download Latest Release](https://github.com/MehranQadirian/Uprix-Application/releases/latest) • [Documentation](./README-EN.md) • [Report Issue](https://github.com/MehranQadirian/Uprix-Application/issues)

</div>

---

## 📋 Table of Contents

- [About The Project](#-about-the-project)
- [Features](#-features)
- [Screenshots](#️-screenshots)
- [Installation](#-installation)
- [Usage Guide](#-usage-guide)
- [Technologies Used](#️-technologies-used)
- [Contributing](#-contributing)
- [Contact & Support](#-contact--support)

---

## 🎯 About The Project

**Uprix Assistant** is a powerful and modern desktop application designed for smart management of applications, browser bookmarks, and daily tasks. With a beautiful and intuitive user interface, it provides a smooth and efficient experience for users.

### 🎨 Key Features

- **Smart Application Management** with advanced search engine
- **Bookmark Synchronization** from all popular browsers
- **Professional Task Management** with projects and subtasks support
- **Advanced Theme System** with 10 different neon themes
- **Auto-Update** from GitHub Releases

---

## ✨ Features

### 🚀 Application Management (App Launcher)

<details open>
<summary><b>Click to view details</b></summary>

- ✅ **Auto-detection** of installed applications from Start Menu and Registry
- 🔍 **Smart Search** with algorithms:
  - Levenshtein Distance
  - Damerau-Levenshtein Distance
  - Jaro-Winkler Similarity
  - N-Gram Similarity (Trigram)
- ⭐ **Favorite marking** for applications
- 📊 **Usage tracking** and sorting by Rate
- 🎯 **Advanced filtering** by name, category, etc.
- 🔄 **Lazy loading** with Skeleton Loader animation

</details>

### 🔖 Bookmark Management (Bookmark Manager)

<details>
<summary><b>Click to view details</b></summary>

- 🌐 **Full support** for browsers:
  - Google Chrome
  - Microsoft Edge
  - Mozilla Firefox
  - Custom browsers
- 🔄 **Auto-extraction** of bookmarks
- 📝 **Edit and manage** bookmarks
- ⭐ **Favorite marking**
- 🎯 **Rate system** with 4 levels:
  - Normal (green)
  - Warning (orange)
  - Critical (red)
  - Emergency (dark red)
- 🔀 **Drag & Drop** between browsers
- 📊 **Usage statistics** for each bookmark
- 🗂️ **Categorization** by browser

</details>

### ✅ Task Management (Task Manager)

<details>
<summary><b>Click to view details</b></summary>

- 📋 **Create and manage** tasks and projects
- 🔗 Unlimited **Subtasks**
- 📊 **Progress chart** in Real-time
- 🎨 **Project coloring** with 6 different colors
- 🏷️ **Task tagging**
- 📅 **Due date** with Overdue warning
- 🎯 **Prioritization** with 4 levels:
  - Low
  - Medium
  - High
  - Critical
- 📱 **QR Code generation** for each task
- 📊 **Daily report** with complete statistics
- 🔍 **Advanced filter and search**
- 💾 **Local storage** with LiteDB
- 📤 **Export to CSV**

</details>

### 🎨 Theme System

<details>
<summary><b>Click to view details</b></summary>

10 beautiful neon themes:
- 🔵 **Neon Cyan** (default)
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

### 🔄 Update System

<details>
<summary><b>Click to view details</b></summary>

- ✅ **Auto-check** for new versions
- 📦 **Download and install** automatically
- 📊 **Download progress** display
- 🔔 **Notification** for new versions

</details>

---

## 🖼️ Screenshots

<div align="center">

### Main Screen
<img alt="Main Screen" width="700" src="https://github.com/user-attachments/assets/22c64124-e0b2-4718-b235-554211a3d2eb" />

### Bookmark Management
<img alt="Bookmark Manager" width="700" src="https://github.com/user-attachments/assets/49976dc4-3071-4cdf-af19-14daef199e6f" />

### Task Management
<img alt="Task Manager" width="700" src="https://github.com/user-attachments/assets/4f4b341a-061c-404d-901a-3c454cc8595b" />

### Settings & Themes
<img alt="Settings" width="700" src="https://github.com/user-attachments/assets/dea91bdf-b95a-4826-8930-5d35e11b0d44" />

</div>

---

## 🔧 Installation

### Prerequisites

- Windows 10/11 (64-bit)
- .NET Framework 4.8 or higher
- Minimum 100MB free space

### 📦 Dependencies

The project automatically downloads the following packages:
```xml
<PackageReference Include="LiteDB" Version="5.0.11" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
<PackageReference Include="QRCoder" Version="1.4.3" />
<PackageReference Include="System.Data.SQLite.Core" Version="1.0.115" />
```

---

## 📖 Usage Guide

### 🚀 App Launcher

#### Searching for Applications
```plaintext
1. Start typing in the search box
2. Use arrow keys ↑↓ to navigate
3. Press Enter or double-click
```

#### Marking as Favorite
```plaintext
1. Click on the star icon ⭐
2. The application will be displayed at the top of the list
```

### 🔖 Bookmark Manager

#### Adding a Bookmark
```plaintext
1. Click the "+ Add Bookmark" button
2. Enter the information:
   - Browser: Select browser
   - Title: Bookmark title
   - URL: Website address
   - Rate: Importance level
   - Favorite: Mark as favorite
3. Click the Save button
```

#### Transfer Between Browsers
```plaintext
1. Drag the bookmark
2. Drop it on the destination browser tab
3. The bookmark will be added to the new browser
```

### ✅ Task Manager

#### Creating a Task
```plaintext
1. Click the "+ Add Task" button
2. Enter the information:
   - Title: Task title
   - Description: Details
   - Project: Select project
   - Priority: Priority level
   - Status: Current status
   - Due Date: Deadline
   - Tags: Labels (comma separated)
3. Click the Save button
```

#### Adding a Subtask
```plaintext
1. Click the "+ Add Subtask" button on the task card
2. Enter the subtask information
3. The subtask will be added to the list
```

#### Generating QR Code
```plaintext
1. Click the 📱 icon on the task card
2. QR Code will be displayed
3. Scan with your mobile device
```

---

## 🛠️ Technologies Used

### Frameworks and Libraries

| Technology | Version | Usage |
|-----------|---------|-------|
| **WPF** | .NET Framework 4.8 | User Interface |
| **C#** | 10.0 | Programming Language |
| **LiteDB** | 5.0.11 | NoSQL Database |
| **Newtonsoft.Json** | 13.0.1 | JSON Processing |
| **QRCoder** | 1.4.3 | QR Code Generation |
| **SQLite** | 1.0.115 | Firefox DB Processing |
| **IWshRuntimeLibrary** | - | Shortcut Processing |

### Design Patterns

- **MVVM** (Model-View-ViewModel)
- **Repository Pattern** for data access
- **Service Layer** for business logic
- **Dependency Injection** for dependency management

## 📞 Contact & Support

<div align="center">

### Contact Information

| Platform | Link |
|----------|------|
| 📧 Email | [mehranghadirian01@gmail.com](mailto:mehranghadirian01@gmail.com) |
| 💬 Telegram | [@UprixApplication](https://t.me/UprixApplication) |
| 🐙 GitHub | [@MehranQadirian](https://github.com/MehranQadirian) |
| 🌐 Website | [Lumora Flow](https://lumora-flow.pages.dev) |

### Support the Project

If this project was useful to you, you can:

⭐ **Star** the project

🐛 Report an **Issue**

💡 Suggest a new **Feature**

🤝 **Contribute** to development

</div>

---

<div align="center">

### 💝 Support the Project

If you enjoyed this project, please:

[![GitHub Stars](https://img.shields.io/github/stars/MehranQadirian/Uprix-Application?style=social)](https://github.com/MehranQadirian/Uprix-Application)
[![GitHub Forks](https://img.shields.io/github/forks/MehranQadirian/Uprix-Application?style=social)](https://github.com/MehranQadirian/Uprix-Application/fork)

---

Made with ❤️ by [Mehran Qadirian](https://github.com/MehranQadirian)

**© 2025 Uprix Assistant. All rights reserved.**

</div>
