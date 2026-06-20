# Anime // MAL Status Companion

A lightweight WPF desktop companion application that automatically broadcasts your currently watching anime and reading manga progress from MyAnimeList (MAL) directly into your Discord Rich Presence status, styled in a clean, high-contrast retro **Manga aesthetic**.

---

## 🎨 Design Language (Manga Style)
* **Hard Outlines & Sharp Edges:** Zero corner rounding on cards, buttons, text fields, and image borders (`CornerRadius="0"`), resembling hand-drawn ink frames.
* **Solid Block Shadows:** Features flat, offset black block shadows with zero blur (`BlurRadius="0"`, `ShadowDepth="4"`, `Opacity="1.0"`) instead of modern gradients.
* **High Contrast Palette:** Uses solid colors (rich `#E0115F` ruby red accent, slate charcoal sidebar, and clean white content panels) with zero color gradients.

---

## ✨ Features
1. **Discord Rich Presence Sync:** 
   * Broadcasts your active progress to Discord in real-time.
   * Auto-detects media types (Anime broadcasts "Watching", Manga broadcasts "Watching" with the matching TV screen status icon).
   * Displays details like "Watching {Title}" and "Episode {X}/{Y}" alongside a "View on MAL" clickable button.
2. **Currently Watching Active Session:**
   * A dedicated tab in the navigation menu holds the active broadcast state.
   * Navigating to other views (e.g. search, settings, library) does not disrupt the active presence or reset the elapsed time counter.
3. **Local Library Integration:**
   * Save any media locally to quickly view it again without searching.
   * Persisted locally under `%LOCALAPPDATA%/MALStatus/local_library.json`.
4. **Origin Badges:**
   * Overlay tags (`MAL` / `LOCAL`) display on card covers in the library grid to quickly identify the source databases. If saved in both, both badges are rendered side-by-side.
5. **Offline Fallback Safety:**
   * If offline or the MAL API fails, a non-blocking warning is shown and the library seamlessly loads all local entries.
6. **Direct Stepper Editing:**
   * Change current progress by typing numbers directly in the text field or using the incremental arrow buttons.

---

## 🛠️ Tech Stack & Dependencies
* **Core:** .NET 8.0 & WPF (Windows Presentation Foundation)
* **Architecture:** Model-View-ViewModel (MVVM) powered by `CommunityToolkit.Mvvm`
* **Secure Storage:** Windows Data Protection API (DPAPI) for local credential encryption
* **Discord RPC:** `DiscordRichPresence` SDK

---

## 🚀 Building & Running

### Prerequisites
* **.NET 8.0 SDK** (Desktop workload enabled)
* **Discord Desktop Client** (Must be running for Rich Presence)

### 1. Build and Run (Debug)
Clone the repository and run the following in the project root:
```bash
dotnet restore
dotnet run --project MALDiscordPresence
```

### 2. Publish (Production Release)
To compile a single-folder self-contained win-x64 binary bundle:
```bash
# Clean intermediate caches first to ensure fresh resource compile
Remove-Item -Path "MALDiscordPresence/obj", "MALDiscordPresence/bin" -Recurse -Force -ErrorAction SilentlyContinue

# Publish
dotnet publish MALDiscordPresence -c Release -r win-x64 --self-contained true -o ./publish
```

---

## 📦 Creating the Installer

The project includes an Inno Setup script `setup.iss` to package the published binaries into a native installer `AnimeSetup.exe`.

1. Install Inno Setup 6 (via winget):
   ```bash
   winget install --id JRSoftware.InnoSetup -e --silent
   ```
2. Compile the installer:
   ```bash
   & "$env:LocalAppData\Programs\Inno Setup 6\ISCC.exe" setup.iss
   ```
3. Find the compiled installer executable under the `Output/` folder:
   * `Output/AnimeSetup.exe`
