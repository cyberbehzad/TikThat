# 🏗️ Technical Architecture

**TikThat** is built on **.NET 10 (.NET MAUI)** and follows a clean **MVVM (Model-View-ViewModel)** architecture powered by `CommunityToolkit.Mvvm` and built-in .NET Dependency Injection (`MauiProgram.cs`).

---

## 1. Core Architectural Layers

* **Models (`Models/`):** Plain C# objects and SQLite-net entities (`TodoItem`, `UserProfile`, `Achievement`, `CalendarDay`, `CustomReminder`).
* **Services (`Services/`):** Encapsulate platform-specific APIs, persistence, and background processing behind clean interfaces (`IUserProfileService`, `ITikThatNotificationService`).
* **ViewModels (`ViewModels/`):** Manage UI state, commands (`[RelayCommand]`), and observable properties (`[ObservableProperty]`) with zero direct dependency on UI controls.
* **Views (`Views/`):** XAML pages utilizing `CompiledBindings` (`x:DataType`), `SkiaSharp` Lottie animations, and adaptive `AppThemeBinding`.
* **Messages (`Messages/`):** Decoupled cross-page communication via `WeakReferenceMessenger` (e.g., `LocationPickedMessage` from `MapPickerPage` to `MainViewModel`).

---

## 2. Offline Persistence Strategy

* **Tasks Database (`DatabaseService`):** Uses `sqlite-net-pcl` with asynchronous connection pooling (`SQLiteAsyncConnection`) and a `SemaphoreSlim` initialization gate to prevent concurrent table creation race conditions.
* **User Profile & Gamification (`UserProfileService`):** Stores profile settings, unlocked achievement IDs, and custom reminders in `userprofile.json` using atomic temp-file writes (`userprofile.json.tmp` -> replace) to eliminate file corruption risks on unexpected shutdowns.
* **Backup & Restore (`BackupService`):** Serializes tasks and profile metadata into portable JSON backups. On restore, duplicate tasks are filtered by matching `(Text, DueDate)` tuples while merging achievement sets and taking the maximum earned points.

---

## 3. Cross-Platform Notifications & Scheduling

* **Android (`NotificationService`):** Uses `Plugin.LocalNotification` with dedicated notification channels for daily reminders, task-specific alerts, and geofence triggers.
* **Windows Unpackaged Desktop (`WindowsScheduledNotificationService`):** Since unpackaged WinUI 3 / MAUI desktop apps cannot rely on UWP background tasks when closed, TikThat registers one-time tasks in `schtasks.exe` that invoke `Windows.UI.Notifications.ToastNotificationManager` via hidden Base64-encoded PowerShell commands.

---

## 4. Background Geofencing

* Powered by **Shiny (`Shiny.Locations`)** via `TaskGeofenceDelegate` and `MyGeofenceDelegate`.
* Registers circular regions (`200m` radius) for tasks with coordinates.
* Implements a `ConcurrentDictionary` with a 10-minute cooldown per `(RegionIdentifier, GeofenceState)` to suppress duplicate alerts caused by GPS boundary jitter.
