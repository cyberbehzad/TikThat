<div align="center">

# ✅ TikThat

**A Cross-Platform, Privacy-First Task Manager & Focus Companion Built with .NET MAUI**

[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-10.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/apps/maui)
[![Platforms](https://img.shields.io/badge/Platforms-Windows%20%7C%20Android-0078D4?style=for-the-badge&logo=windows)](https://github.com/cyberbehzad/TikThat/releases/latest)
[![SQLite](https://img.shields.io/badge/Storage-Local%20SQLite-003B57?style=for-the-badge&logo=sqlite)](https://www.sqlite.org/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

🌐 **Language:** **English** | [فارسی (Persian)](README.fa.md)

[📥 Download Latest Release](https://github.com/cyberbehzad/TikThat/releases/latest) • [📖 راهنمای فارسی کاربران](docs/USER_GUIDE_FA.md) • [🏗️ Technical Architecture](docs/ARCHITECTURE.md)

</div>

---

## ✨ Overview

**TikThat** is a modern, offline-first task management and productivity application built on **.NET MAUI (.NET 10)** for **Windows** and **Android**. Designed with a sleek dark/light adaptive UI, it combines daily task tracking, native Jalali (Persian) seasonal calendars, geofenced location reminders, biometric task locking, and a gamified **Ninja Focus Mode** — while keeping 100% of user data strictly local on the device.

---

## 🚀 Key Features

* **📅 Native Jalali (Persian) Seasonal Calendar:** Full Persian calendar integration featuring dynamic monthly backgrounds and seasonal Lottie animations.
* **🥷 Ninja Focus Mode:** Distraction-free Pomodoro-style focus timer with ink-transition animations, custom session durations, and point rewards upon completion.
* **📍 Geofenced Location Reminders:** Interactive map picker and background geofencing (powered by Shiny) that alerts you when entering a 200m radius of a task's location, complete with GPS boundary jitter throttling.
* **⏰ Smart Time & Daily Reminders:** Platform-native scheduling using WinRT Toast notifications via Windows Task Scheduler on desktop and local notification channels on Android.
* **🔒 Biometric & Privacy Task Lock:** Hide sensitive task titles behind device authentication with instant lock/unlock toggles.
* **🏆 Gamification & Secret Achievements:** Unlockable badges, points, and hidden missions discovered through everyday productivity habits.
* **💾 Offline JSON Backup & Restore:** Export and smartly merge tasks, achievements, custom reminders, and profile stats locally with zero cloud dependency.

---

## 📥 Installation (For End Users)

No developer tools are required to use **TikThat**. Go to the [**Releases**](https://github.com/cyberbehzad/TikThat/releases/latest) page and download the installer matching your device:

* **🪟 Windows (64-bit Recommended):** `TikThat_Setup_x64.exe`
* **🪟 Windows (32-bit Legacy):** `TikThat_Setup_x86.exe`
* **🤖 Android (Modern 64-bit Recommended):** `TikThat-arm64.apk`
* **🤖 Android (Universal — All Devices):** `TikThat-universal.apk`
* **🤖 Android (32-bit Legacy):** `TikThat-arm32.apk`

---

## 🛠️ Building from Source (For Developers)

### Prerequisites
* [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
* .NET MAUI workload (`dotnet workload install maui`)
* **Visual Studio 2022 / 2026** with *.NET Multi-platform App UI development* workload

### Clone & Run

```bash
git clone [https://github.com/cyberbehzad/TikThat.git](https://github.com/cyberbehzad/TikThat.git)
cd TikThat
dotnet restore
dotnet build -t:Run -f net10.0-windows10.0.19041.0
