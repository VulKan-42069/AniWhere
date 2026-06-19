# MALDiscordPresence - Anime/Manga Discord Rich Presence

> **For Hermes:** Use subagent-driven-development skill to implement this plan task-by-task.

**Goal:** Build a native Windows desktop app (C# WPF) that connects to a user's MyAnimeList account and sets their Discord Rich Presence to show what anime/manga they're currently watching/reading, including episode/chapter number.

**Architecture:** MVVM WPF desktop application with three core services: MAL OAuth2 API client, Discord RPC client via local IPC, and a manga-panel-inspired light-theme UI. The app runs as a system tray companion that maintains Discord presence. User authenticates via MAL OAuth2 (PKCE flow via localhost callback), browses their anime/manga library in a panel-based UI, and selects what to display.

**Tech Stack:**
- C# / .NET 9 / WPF
- `DiscordRichPresence` NuGet (Lachee's discord-rpc-csharp, v1.6.1.70)
- `CommunityToolkit.Mvvm` NuGet (MVVM source generators)
- `System.Text.Json` (MAL API responses)
- `System.Net.Http` (MAL API calls)
- MyAnimeList API v2 (REST, OAuth2 with PKCE)
- Target: Windows 10 1809+ (x64)

**App Name:** `MALDiscordPresence`
**App Display Name:** `MAL Status`

---

## Design System

### Ruby Color Palette (Light Theme)

| Role | Color | Hex | Usage |
|------|-------|-----|-------|
| Primary Ruby | Ruby | `#E0115F` | CTAs, active states, selected items |
| Dark Ruby | Crimson | `#9B111E` | Borders, strong accents, title text |
| Medium Ruby | Rose | `#D64A5A` | Hover states, secondary buttons |
| Light Ruby | Blush | `#F3C6C8` | Card fills, soft backgrounds, badges |
| Surface | Off-White | `#FAF8F7` | Main window background |
| Card Surface | Paper White | `#FFFFFF` | Panel/card inner backgrounds |
| Text Primary | Sumi Black | `#1B1621` | Body text, labels |
| Text Secondary | Muted Ink | `#6B5B6D` | Timestamps, descriptions |
| Divider | Light Border | `#D4C8D6` | Subtle separators |
| Success | Matcha | `#2D8A4E` | Online, connected indicators |
| Warning | Amber | `#D4870E` | Rate limit, caution states |

### Manga-Style UI Principles

All UI elements follow a strict manga panel aesthetic:

1. **Zero border-radius.** Every button, card, panel, input, and container has `CornerRadius="0"`. No soft edges anywhere.
2. **Hard ink borders.** Primary panels use `BorderThickness="3"` with solid dark ruby or sumi-black strokes. Secondary panels use `1-2px`.
3. **Panel gutters.** Sections are separated by consistent `6-8px` gaps (gutters) that act like manga page gutters. Background color shows through as the gutter.
4. **Caption strips.** Section headers are rendered as uppercase mono-font labels on a dark ruby background strip attached to the top edge of panels.
5. **Offset shadow blocks.** Panels get a `2-4px` solid offset shadow (not blur) in sumi-black or dark ruby for a printed/stacked feel.
6. **No gradients.** Flat fills only. Depth comes from borders, shadows, and gutters.
7. **Halftone texture (optional).** Subtle CSS-style dot pattern overlays at ~5% opacity on accent areas for screentone flavor.
8. **Asymmetric layouts.** Feature sections use intentional asymmetry (e.g., sidebar narrower than main content) to feel editorial, not corporate.

---

## Project Structure

```
MALDiscordPresence/
├── MALDiscordPresence.sln
├── MALDiscordPresence/
│   ├── MALDiscordPresence.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── Models/
│   │   ├── AnimeEntry.cs          # MAL anime list item model
│   │   ├── MangaEntry.cs          # MAL manga list item model
│   │   ├── MalTokenResponse.cs    # OAuth token response
│   │   └── PresenceState.cs       # Current RPC presence state
│   ├── Services/
│   │   ├── IMalService.cs         # MAL API interface
│   │   ├── MalService.cs          # MAL API implementation
│   │   ├── IDiscordService.cs     # Discord RPC interface
│   │   ├── DiscordService.cs      # Discord RPC implementation
│   │   ├── IAuthService.cs        # Auth interface
│   │   └── AuthService.cs         # OAuth2 PKCE + token storage
│   ├── ViewModels/
│   │   ├── MainViewModel.cs       # Main window VM (navigation + state)
│   │   ├── LibraryViewModel.cs    # Anime/manga list browsing
│   │   ├── DetailViewModel.cs     # Selected title detail + episode picker
│   │   └── SettingsViewModel.cs   # App settings + Discord app config
│   ├── Views/
│   │   ├── MainWindow.xaml        # Shell with sidebar navigation
│   │   ├── LibraryView.xaml       # Anime/manga grid
│   │   ├── DetailView.xaml        # Title detail + status picker
│   │   ├── LoginView.xaml         # MAL OAuth login
│   │   └── SettingsView.xaml      # Settings page
│   ├── Controls/
│   │   ├── PanelBorder.xaml       # Reusable manga-panel container
│   │   ├── CaptionStrip.xaml      # Uppercase header bar
│   │   └── SearchBar.xaml         # Manga-styled search input
│   ├── Converters/
│   │   ├── StatusToColorConverter.cs
│   │   ├── BoolToVisibilityConverter.cs
│   │   └── ImageUrlToBitmapConverter.cs
│   ├── Themes/
│   │   ├── MangaTheme.xaml        # All brushes, colors, base styles
│   │   ├── MangaButtons.xaml      # Sharp-corner button styles
│   │   ├── MangaInputs.xaml       # Textbox, ComboBox styles
│   │   └── MangaControls.xaml     # Panel, caption, card styles
│   ├── Assets/
│   │   ├── icon.ico              # App icon (manga ruby style)
│   │   ├── app_icon.png          # Small Discord asset (128x128)
│   │   └── splash.png            # Login screen cover image
│   └── Helpers/
│       ├── TrayIcon.cs           # System tray management
│       └── SecureStorage.cs      # Token encryption via DPAPI
└── README.md
```

---

## Implementation Plan

### Phase 1: Project Scaffold + Design System (Tasks 1-8)

---

### Task 1: Create Solution and WPF Project

**Objective:** Initialize the .NET 9 WPF solution with proper project structure.

**Files:**
- Create: `MALDiscordPresence/MALDiscordPresence.sln`
- Create: `MALDiscordPresence/MALDiscordPresence/MALDiscordPresence.csproj`

**Steps:**

```bash
cd /home/ruby/MALDiscordPresence
dotnet new sln -n MALDiscordPresence
dotnet new wpf -n MALDiscordPresence -f net9.0
dotnet sln add MALDiscordPresence/MALDiscordPresence.csproj
```

**MALDiscordPresence.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <UseWPF>true</UseWPF>
    <ApplicationIcon>Assets\icon.ico</ApplicationIcon>
    <AssemblyName>MALStatus</AssemblyName>
    <RootNamespace>MALDiscordPresence</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="DiscordRichPresence" Version="1.6.1.70" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
  </ItemGroup>
</Project>
```

**Verify:** `dotnet build` succeeds with zero errors.

---

### Task 2: Create Folder Structure

**Objective:** Set up all directories for the MVVM architecture.

**Steps:**

```bash
cd MALDiscordPresence/MALDiscordPresence
mkdir -p Models Services ViewModels Views Controls Converters Themes Assets Helpers
```

**Verify:** All directories exist.

---

### Task 3: Define MangaTheme.xaml - Color Brushes

**Objective:** Create the complete Ruby color palette as WPF resource dictionary.

**File:** `Themes/MangaTheme.xaml`

**XAML:**
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- === RUBY COLOR PALETTE === -->
    <Color x:Key="PrimaryRubyColor">#E0115F</Color>
    <Color x:Key="DarkRubyColor">#9B111E</Color>
    <Color x:Key="MediumRubyColor">#D64A5A</Color>
    <Color x:Key="LightRubyColor">#F3C6C8</Color>
    <Color x:Key="SurfaceColor">#FAF8F7</Color>
    <Color x:Key="CardSurfaceColor">#FFFFFF</Color>
    <Color x:Key="TextPrimaryColor">#1B1621</Color>
    <Color x:Key="TextSecondaryColor">#6B5B6D</Color>
    <Color x:Key="DividerColor">#D4C8D6</Color>
    <Color x:Key="SuccessColor">#2D8A4E</Color>
    <Color x:Key="WarningColor">#D4870E</Color>

    <!-- === SOLID COLOR BRUSHES === -->
    <SolidColorBrush x:Key="PrimaryRubyBrush" Color="{StaticResource PrimaryRubyColor}" />
    <SolidColorBrush x:Key="DarkRubyBrush" Color="{StaticResource DarkRubyColor}" />
    <SolidColorBrush x:Key="MediumRubyBrush" Color="{StaticResource MediumRubyColor}" />
    <SolidColorBrush x:Key="LightRubyBrush" Color="{StaticResource LightRubyColor}" />
    <SolidColorBrush x:Key="SurfaceBrush" Color="{StaticResource SurfaceColor}" />
    <SolidColorBrush x:Key="CardSurfaceBrush" Color="{StaticResource CardSurfaceColor}" />
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}" />
    <SolidColorBrush x:Key="TextSecondaryBrush" Color="{StaticResource TextSecondaryColor}" />
    <SolidColorBrush x:Key="DividerBrush" Color="{StaticResource DividerColor}" />
    <SolidColorBrush x:Key="SuccessBrush" Color="{StaticResource SuccessColor}" />
    <SolidColorBrush x:Key="WarningBrush" Color="{StaticResource WarningColor}" />

    <!-- === MANGA PANEL PROPERTIES === -->
    <sys:Double x:Key="PanelBorderWidth">3</sys:Double>
    <sys:Double x:Key="ThinBorderWidth">1</sys:Double>
    <sys:Double x:Key="GutterSize">6</sys:Double>
    <sys:Double x:Key="PanelShadowOffset">3</sys:Double>
    <Thickness x:Key="PanelPadding">16</Thickness>
    <Thickness x:Key="GutterPadding">6</Thickness>
</ResourceDictionary>
```

*(Add `xmlns:sys="clr-namespace:System;assembly=mscorlib"` to the ResourceDictionary tag.)*

**Verify:** `dotnet build` succeeds.

---

### Task 4: Define MangaButtons.xaml - Sharp Button Styles

**Objective:** Create all button styles with zero border-radius and hard ink borders.

**File:** `Themes/MangaButtons.xaml`

**Key Styles:**

```xml
<!-- Primary CTA Button: Ruby fill, white text, 3px dark border -->
<Style x:Key="RubyButton" TargetType="Button">
    <Setter Property="Background" Value="{StaticResource PrimaryRubyBrush}" />
    <Setter Property="Foreground" Value="White" />
    <Setter Property="FontWeight" Value="Bold" />
    <Setter Property="FontSize" Value="13" />
    <Setter Property="FontFamily" Value="Consolas, Courier New" />
    <Setter Property="Padding" Value="20,10" />
    <Setter Property="BorderThickness" Value="3" />
    <Setter Property="BorderBrush" Value="{StaticResource DarkRubyBrush}" />
    <Setter Property="Cursor" Value="Hand" />
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border x:Name="border"
                        Background="{TemplateBinding Background}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        CornerRadius="0"
                        Padding="{TemplateBinding Padding}"
                        SnapsToDevicePixels="True">
                    <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center" />
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="border" Property="Background"
                                Value="{StaticResource MediumRubyBrush}" />
                    </Trigger>
                    <Trigger Property="IsPressed" Value="True">
                        <Setter TargetName="border" Property="Background"
                                Value="{StaticResource DarkRubyBrush}" />
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>

<!-- Ghost/Outline Button: transparent fill, ruby border -->
<Style x:Key="GhostButton" TargetType="Button" BasedOn="{StaticResource RubyButton}">
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="Foreground" Value="{StaticResource DarkRubyBrush}" />
    <!-- Hover fills with LightRubyBrush -->
</Style>
```

**Verify:** `dotnet build` succeeds.

---

### Task 5: Define MangaInputs.xaml - Input Control Styles

**Objective:** Style TextBox, ComboBox, and SearchBar with manga aesthetics.

**File:** `Themes/MangaInputs.xaml`

**Key Style:**
```xml
<Style x:Key="MangaTextBox" TargetType="TextBox">
    <Setter Property="Background" Value="{StaticResource CardSurfaceBrush}" />
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}" />
    <Setter Property="BorderThickness" Value="2" />
    <Setter Property="BorderBrush" Value="{StaticResource DividerBrush}" />
    <Setter Property="FontFamily" Value="Segoe UI" />
    <Setter Property="FontSize" Value="13" />
    <Setter Property="Padding" Value="10,8" />
    <!-- NO CornerRadius anywhere -->
    <!-- Focus triggers set BorderBrush to PrimaryRubyBrush -->
</Style>
```

**Verify:** `dotnet build` succeeds.

---

### Task 6: Define MangaControls.xaml - Panel & Card Styles

**Objective:** Create the manga-panel container, caption strip, and card styles.

**File:** `Themes/MangaControls.xaml`

**Key Controls:**

```xml
<!-- MANGA PANEL: The core container. Hard border, offset shadow, no radius. -->
<Style x:Key="MangaPanel" TargetType="Border">
    <Setter Property="Background" Value="{StaticResource CardSurfaceBrush}" />
    <Setter Property="BorderThickness" Value="{StaticResource PanelBorderWidth}" />
    <Setter Property="BorderBrush" Value="{StaticResource DarkRubyBrush}" />
    <Setter Property="Padding" Value="{StaticResource PanelPadding}" />
    <!-- Shadow simulated via second Border wrapping this one -->
</Style>

<!-- CAPTION STRIP: Uppercase mono label at panel top -->
<!-- This is a UserControl defined in Controls/CaptionStrip.xaml -->

<!-- LIST ITEM: Anime/manga entry in the library grid -->
<Style x:Key="LibraryListItem" TargetType="Border">
    <Setter Property="Background" Value="{StaticResource CardSurfaceBrush}" />
    <Setter Property="BorderThickness" Value="2" />
    <Setter Property="BorderBrush" Value="{StaticResource DividerBrush}" />
    <Setter Property="Padding" Value="12" />
    <!-- Hover: BorderBrush -> PrimaryRubyBrush, Background -> LightRubyBrush -->
</Style>
```

**Verify:** `dotnet build` succeeds.

---

### Task 7: Merge All Themes in App.xaml

**Objective:** Wire up the resource dictionaries so all styles are available app-wide.

**File:** `App.xaml`

```xml
<Application x:Class="MALDiscordPresence.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="Views/MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Themes/MangaTheme.xaml" />
                <ResourceDictionary Source="Themes/MangaButtons.xaml" />
                <ResourceDictionary Source="Themes/MangaInputs.xaml" />
                <ResourceDictionary Source="Themes/MangaControls.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

**Verify:** `dotnet build` succeeds.

---

### Task 8: Create MainWindow Shell

**Objective:** Build the main window with sidebar navigation + content area.

**File:** `Views/MainWindow.xaml`

**Layout:**
```
┌──────────────────────────────────────────────┐
│ MAL STATUS          [status dot]  [_][□][X]  │ ← Custom title bar (DarkRuby bg)
├────────┬─────────────────────────────────────┤
│        │                                     │
│  LIBRARY│     ← ContentControl here          │
│  ──────│        (swapped by navigation)      │
│  STATUS │                                     │
│  ──────│                                     │
│  SETTINGS│                                   │
│        │                                     │
│  ○ Online│   ← Discord connection indicator  │
│         │                                    │
└────────┴─────────────────────────────────────┘
```

- Sidebar: `200px` wide, `SurfaceBrush` background, `DarkRubyBrush` 3px right border
- Navigation items: uppercase mono-font, hover highlights with `LightRubyBrush`
- Active nav item: `PrimaryRubyBrush` background, white text
- Content area: `SurfaceBrush` background with `GutterPadding`

**Verify:** `dotnet build` succeeds, app launches with visible sidebar and empty content area.

---

### Phase 2: Models + Services (Tasks 9-18)

---

### Task 9: MAL API Models

**Objective:** Create C# models that map to MAL API v2 JSON responses.

**Files:**
- Create: `Models/AnimeEntry.cs`
- Create: `Models/MangaEntry.cs`
- Create: `Models/MalTokenResponse.cs`

**AnimeEntry.cs:**
```csharp
using System.Text.Json.Serialization;

namespace MALDiscordPresence.Models;

public class AnimeEntry
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("num_episodes")]
    public int NumEpisodes { get; set; }

    [JsonPropertyName("my_list_status")]
    public AnimeListStatus? MyListStatus { get; set; }

    [JsonPropertyName("main_picture")]
    public Picture? MainPicture { get; set; }
}

public class AnimeListStatus
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // watching, completed, on_hold, dropped, plan_to_watch

    [JsonPropertyName("num_episodes_watched")]
    public int NumEpisodesWatched { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }
}

public class MangaEntry
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("num_chapters")]
    public int NumChapters { get; set; }

    [JsonPropertyName("my_list_status")]
    public MangaListStatus? MyListStatus { get; set; }

    [JsonPropertyName("main_picture")]
    public Picture? MainPicture { get; set; }
}

public class MangaListStatus
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // reading, completed, on_hold, dropped, plan_to_read

    [JsonPropertyName("num_chapters_read")]
    public int NumChaptersRead { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }
}

public class Picture
{
    [JsonPropertyName("medium")]
    public string Medium { get; set; } = string.Empty;

    [JsonPropertyName("large")]
    public string Large { get; set; } = string.Empty;
}

// Wrapper for paginated MAL responses
public class PagedResponse<T>
{
    [JsonPropertyName("data")]
    public List<DataWrapper<T>> Data { get; set; } = new();

    [JsonPropertyName("paging")]
    public Paging? Paging { get; set; }
}

public class DataWrapper<T>
{
    [JsonPropertyName("node")]
    public T Node { get; set; } = default!;
}

public class Paging
{
    [JsonPropertyName("next")]
    public string? Next { get; set; }
}
```

**MalTokenResponse.cs:**
```csharp
using System.Text.Json.Serialization;

namespace MALDiscordPresence.Models;

public class MalTokenResponse
{
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}
```

**Verify:** `dotnet build` succeeds.

---

### Task 10: PresenceState Model

**Objective:** Model the current Discord presence state.

**File:** `Models/PresenceState.cs`

```csharp
namespace MALDiscordPresence.Models;

public enum MediaType { Anime, Manga }

public class PresenceState
{
    public bool IsActive { get; set; }
    public MediaType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public int CurrentEpisode { get; set; }
    public int TotalEpisodes { get; set; }
    public DateTime StartedAt { get; set; }
    public string MalUrl { get; set; } = string.Empty;

    // Cover image URL from MAL — used directly as Discord LargeImageKey
    // Discord RPC supports external URLs, so no need to upload assets!
    public string CoverImageUrl { get; set; } = string.Empty;

    public string DetailText => Type == MediaType.Anime
        ? $"Watching {Title}"
        : $"Reading {Title}";

    public string StateText => Type == MediaType.Anime
        ? $"Episode {CurrentEpisode}/{TotalEpisodes}"
        : $"Chapter {CurrentEpisode}/{TotalEpisodes}";
}
```

**Verify:** `dotnet build` succeeds.

---

### Task 11: SecureStorage - Token Persistence

**Objective:** Encrypt and store MAL tokens using Windows DPAPI.

**File:** `Helpers/SecureStorage.cs`

```csharp
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MALDiscordPresence.Helpers;

public static class SecureStorage
{
    private static readonly string StoragePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "MALStatus", "tokens.dat");

    public static void SaveTokens(string accessToken, string refreshToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(StoragePath)!);
        var json = System.Text.Json.JsonSerializer.Serialize(
            new { access_token = accessToken, refresh_token = refreshToken });
        var encrypted = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(json), null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(StoragePath, encrypted);
    }

    public static (string? AccessToken, string? RefreshToken)? LoadTokens()
    {
        if (!File.Exists(StoragePath)) return null;
        var encrypted = File.ReadAllBytes(StoragePath);
        var decrypted = ProtectedData.Unprotect(
            encrypted, null, DataProtectionScope.CurrentUser);
        var json = System.Text.Json.JsonSerializer.Deserialize<TokenFile>(
            Encoding.UTF8.GetString(decrypted));
        return json is null ? null : (json.AccessToken, json.RefreshToken);
    }

    public static void ClearTokens()
    {
        if (File.Exists(StoragePath)) File.Delete(StoragePath);
    }

    private class TokenFile
    {
        public string Access_token { get; set; } = "";
        public string Refresh_token { get; set; } = "";
    }
}
```

**Verify:** `dotnet build` succeeds.

---

### Task 12: MAL Service Interface + Implementation

**Objective:** Implement the MAL API v2 client with OAuth2 token management.

**Files:**
- Create: `Services/IMalService.cs`
- Create: `Services/MalService.cs`

**IMalService.cs:**
```csharp
using MALDiscordPresence.Models;

namespace MALDiscordPresence.Services;

public interface IMalService
{
    Task<List<AnimeEntry>> GetAnimeListAsync(string status = "watching");
    Task<List<MangaEntry>> GetMangaListAsync(string status = "reading");
    Task<AnimeEntry?> GetAnimeDetailsAsync(int animeId);
    Task<MangaEntry?> GetMangaDetailsAsync(int mangaId);
    Task<List<AnimeEntry>> SearchAnimeAsync(string query);
    Task<List<MangaEntry>> SearchMangaAsync(string query);
}
```

**MalService.cs key implementation notes:**
- Base URL: `https://api.myanimelist.net/v2`
- Header: `Authorization: Bearer {token}`, `X-MAL-Client-ID: {clientId}`
- Pagination: follow `paging.next` links, collect all pages
- Rate limiting: MAL allows ~100 req/min, implement basic throttling
- Token refresh: check expiry, auto-refresh using refresh_token
- Endpoints:
  - Anime list: `GET /users/@me/animelist?status=watching&limit=100`
  - Manga list: `GET /users/@me/mangalist?status=reading&limit=100`
  - Anime search: `GET /anime?q={query}&limit=10`
  - Manga search: `GET /manga?q={query}&limit=10`

**Verify:** `dotnet build` succeeds.

---

### Task 13: Auth Service - OAuth2 PKCE Flow

**Objective:** Implement MAL OAuth2 with PKCE + localhost redirect for native apps.

**Files:**
- Create: `Services/IAuthService.cs`
- Create: `Services/AuthService.cs`

**Flow:**
1. Generate PKCE code_verifier (43-128 char random string) and code_challenge (SHA256 of verifier)
2. Open browser: `https://myanimelist.net/v1/oauth2/authorize?response_type=code&client_id={id}&code_challenge={challenge}&code_challenge_method=plain&redirect_uri=http://localhost:54537/callback`
3. Start temporary HTTP listener on `localhost:54537`
4. User authorizes in browser, MAL redirects to callback with `?code={auth_code}`
5. Exchange code + code_verifier for access_token + refresh_token via `POST https://myanimelist.net/v1/oauth2/token`
6. Store tokens via SecureStorage

**IAuthService.cs:**
```csharp
namespace MALDiscordPresence.Services;

public interface IAuthService
{
    Task<bool> LoginAsync();
    Task<bool> RefreshTokenAsync();
    bool IsAuthenticated { get; }
    string? AccessToken { get; }
    void Logout();
}
```

**Verify:** `dotnet build` succeeds.

---

### Task 14: Discord RPC Service Interface + Implementation

**Objective:** Wrap the DiscordRichPresence library with a clean interface.

**Files:**
- Create: `Services/IDiscordService.cs`
- Create: `Services/DiscordService.cs`

**IDiscordService.cs:**
```csharp
using MALDiscordPresence.Models;

namespace MALDiscordPresence.Services;

public interface IDiscordService : IDisposable
{
    bool IsConnected { get; }
    void Connect();
    void SetPresence(PresenceState state);
    void ClearPresence();
    event EventHandler<bool>? ConnectionChanged;
}
```

**DiscordService.cs key notes:**
- Discord Application ID: configurable in settings (default: user creates their own at discord.com/developers)
- Must upload assets (images) to Discord Developer Portal for use as LargeImageKey/SmallImageKey
- Default small asset: `app_icon` (custom app icon uploaded as a Discord asset)
- Cover art: use MAL's `main_picture.large` URL directly as `LargeImageKey` (Discord RPC supports external URLs natively, no upload needed!)
- Presence structure:
  ```csharp
  new RichPresence
  {
      Details = state.DetailText,           // "Watching Steins;Gate"
      State = state.StateText,              // "Episode 12/24"
      Assets = new Assets
      {
          LargeImageKey = state.CoverImageUrl,  // MAL cover art URL
          LargeImageText = state.Title,
          SmallImageKey = "app_icon",            // Custom app icon (uploaded asset)
          SmallImageText = "MAL Status"
      },
      Buttons = new Button[]
      {
          new Button { Label = "View on MAL", Url = state.MalUrl }
      },
      Timestamps = Timestamps.FromStart(state.StartedAt)
  };
  ```
- Handle disconnection gracefully (Discord not running)
- Fire `ConnectionChanged` event for UI status indicator
- Dispose: clear presence to avoid ghost status

**Verify:** `dotnet build` succeeds.

---

### Task 15: ViewModel - MainViewModel

**Objective:** Central navigation and app state coordinator.

**File:** `ViewModels/MainViewModel.cs`

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MALDiscordPresence.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private ObservableObject? currentView;
    [ObservableProperty] private int selectedNavIndex;
    [ObservableProperty] private bool isDiscordConnected;
    [ObservableProperty] private bool isAuthenticated;

    // Navigation commands bound to sidebar buttons
    [RelayCommand] private void NavigateToLibrary() { /* swap CurrentView to LibraryVM */ }
    [RelayCommand] private void NavigateToStatus() { /* swap CurrentView to DetailVM */ }
    [RelayCommand] private void NavigateToSettings() { /* swap CurrentView to SettingsVM */ }
}
```

**Verify:** `dotnet build` succeeds.

---

### Task 16: ViewModel - LibraryViewModel

**Objective:** Fetch and display the user's anime/manga list with search and filter.

**File:** `ViewModels/LibraryViewModel.cs`

**Key properties:**
- `ObservableCollection<AnimeEntry> AnimeList`
- `ObservableCollection<MangaEntry> MangaList`
- `bool IsAnimeMode` (toggle between anime/manga tabs)
- `string SearchQuery` (filters list client-side + debounced API search)
- `bool IsLoading` (spinner state)
- `AnimeEntry? SelectedAnime` (triggers navigation to DetailView)
- Commands: `LoadListCommand`, `SearchCommand`, `ToggleMediaTypeCommand`
- On item select: navigate to DetailView with the selected entry

**Verify:** `dotnet build` succeeds.

---

### Task 17: ViewModel - DetailViewModel

**Objective:** Show selected title details and episode/chapter picker for Discord status.

**File:** `ViewModels/DetailViewModel.cs`

**Key properties:**
- `string Title`, `string CoverImageUrl`, `int TotalEpisodes`
- `int SelectedEpisode` (numeric up/down, 1 to TotalEpisodes)
- `bool IsWatching` (toggle to start/stop presence)
- `string StatusDisplay` ("Connected to Discord" / "Disconnected")
- Commands:
  - `StartPresenceCommand` → creates PresenceState, calls DiscordService.SetPresence()
  - `StopPresenceCommand` → calls DiscordService.ClearPresence()
  - `IncrementEpisodeCommand` → updates presence with new episode number
  - `BackToLibraryCommand` → navigates back

**Verify:** `dotnet build` succeeds.

---

### Task 18: ViewModel - SettingsViewModel

**Objective:** App configuration: Discord App ID, MAL client credentials, theme, startup behavior.

**File:** `ViewModels/SettingsViewModel.cs`

**Settings (stored in JSON config at %LOCALAPPDATA%/MALStatus/config.json):**
- `DiscordAppId` (string) — user's Discord Application ID
- `MalClientId` (string) — user's MAL API Client ID
- `StartWithWindows` (bool) — auto-launch on boot
- `StartMinimized` (bool) — start in system tray
- `ShowEpisodeProgress` (bool) — include ep/chapter number in status

**Verify:** `dotnet build` succeeds.

---

### Phase 3: Views + UI Assembly (Tasks 19-25)

---

### Task 19: LoginView - MAL OAuth

**Objective:** Build the login screen with manga-styled MAL auth button.

**File:** `Views/LoginView.xaml`

**Layout:**
```
┌─────────────────────────────────────────┐
│          ╔═══════════════════╗          │
│          ║   MAL STATUS      ║          │ ← Title in DarkRuby caption strip
│          ╚═══════════════════╝          │
│                                         │
│         ┌─────────────────┐            │
│         │   [MAL Logo]    │            │ ← Cover image
│         └─────────────────┘            │
│                                         │
│    Your anime/manga library,            │
│    your Discord status.                 │
│                                         │
│   ╔═══════════════════════════╗         │
│   ║   LOGIN WITH MYANIMELIST  ║        │ ← Big RubyButton
│   ╚═══════════════════════════╝         │
│                                         │
│   [Already logged in? Go to library →]  │ ← GhostButton
│                                         │
└─────────────────────────────────────────┘
```

- Centered card with 3px DarkRuby border
- Offset shadow: 3px right, 3px down, sumi-black
- All text in Consolas/mono for headers, Segoe UI for body

**Verify:** App shows login screen on launch when unauthenticated.

---

### Task 20: LibraryView - Anime/Manga Grid

**Objective:** Display the user's list in a manga-panel grid with search and type toggle.

**File:** `Views/LibraryView.xaml`

**Layout:**
```
┌──────────────────────────────────────────────┐
│ [ANIME ║ MANGA]         [🔍 Search...]       │ ← Toggle tabs + search
├──────────────────────────────────────────────┤
│ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ │
│ │ cover  │ │ cover  │ │ cover  │ │ cover  │ │ ← 4-column grid
│ │ ────── │ │ ────── │ │ ────── │ │ ────── │ │   (wraps responsively)
│ │ Title  │ │ Title  │ │ Title  │ │ Title  │ │
│ │ Ep 12  │ │ Ch 45  │ │ Ep 8   │ │ Ch 3  │ │   Each card: 2px border
│ │ ◉ Watch│ │ ◉ Read │ │ ◉ Watch│ │ ◉ Read │ │   cover image + title
│ └────────┘ └────────┘ └────────┘ └────────┘ │   + current progress
│                                              │   + status button
│ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ │
│ │  ...   │ │  ...   │ │  ...   │ │  ...   │ │
│ └────────┘ └────────┘ └────────┘ └────────┘ │
│                                              │
│ ── Page 1 of 5 ── [< Prev] [Next >] ────── │ ← Pagination
└──────────────────────────────────────────────┘
```

- Grid: `UniformGrid Columns="4"` or `ItemsPanelTemplate` with `WrapPanel`
- Card hover: border turns `PrimaryRubyBrush`, background turns `LightRubyBrush`
- Click card: navigates to DetailView
- Loading state: skeleton cards with halftone pattern overlay
- Empty state: manga speech bubble saying "No anime found! (¬_¬)"

**Verify:** After login, library loads and displays anime/manga entries.

---

### Task 21: DetailView - Title Detail + Status Picker

**Objective:** Show selected anime/manga detail with episode/chapter picker and Discord RPC controls.

**File:** `Views/DetailView.xaml`

**Layout:**
```
┌──────────────────────────────────────────────────┐
│ ◄ Back to Library                                │
├──────────────────────────────────────────────────┤
│                                                    │
│  ╔═══════════════╗  STEINS;GATE                   │
│  ║               ║  ═══════════                    │
│  ║   [Cover      ║  Episodes: 24                  │
│  ║    Image]     ║  Status: Watching              │
│  ║               ║  Score: ★★★★★ (9)             │
│  ╚═══════════════╝                                 │
│                                                    │
│  ┌─ DISCORD STATUS ──────────────────────────────┐│
│  │                                                ││
│  │  Episode: [◄] [  12  ] [►]  / 24             ││ ← Numeric stepper
│  │                                                ││   sharp corners
│  │  Preview:                                      ││
│  │  ┌────────────────────────────────────┐       ││
│  │  │ 📺 Watching Steins;Gate           │       ││ ← Live preview
│  │  │ Episode 12/24                      │       ││   of Discord status
│  │  │ ⏱ 00:45:22                        │       ││
│  │  └────────────────────────────────────┘       ││
│  │                                                ││
│  │  ╔═══════════════════════════╗                 ││
│  │  ║   ▶ START STATUS          ║                ││ ← Big start/stop
│  │  ╚═══════════════════════════╝                 ││   toggle button
│  │                                                ││
│  └────────────────────────────────────────────────┘│
│                                                    │
│  [View on MyAnimeList →]                           │ ← External link
└──────────────────────────────────────────────────┘
```

- Episode stepper: two sharp buttons (◄ ►) + number display, updates presence live
- Start/Stop button: toggles between "▶ START STATUS" (Ruby fill) and "■ STOP STATUS" (DarkRuby fill)
- Live preview panel: shows exactly what Discord friends will see
- Elapsed time: starts counting when presence is activated

**Verify:** Selecting an anime shows detail, episode picker works, start/stop toggles Discord presence.

---

### Task 22: SettingsView

**Objective:** Settings page for Discord App ID, MAL credentials, and app preferences.

**File:** `Views/SettingsView.xaml`

**Layout:**
```
┌──────────────────────────────────────────────┐
│ ╔═══════════════════════════════════════╗     │
│ ║ SETTINGS                              ║     │
│ ╚═══════════════════════════════════════╝     │
│                                                │
│ ┌─ DISCORD ─────────────────────────────────┐ │
│ │ Application ID: [________________________]│ │
│ │ [Test Connection]                          │ │
│ │ Status: ● Connected / ○ Disconnected      │ │
│ └────────────────────────────────────────────┘ │
│                                                │
│ ┌─ MYANIMELIST ─────────────────────────────┐ │
│ │ Client ID: [________________________]     │ │
│ │ Logged in as: [username]  [Logout]        │ │
│ └────────────────────────────────────────────┘ │
│                                                │
│ ┌─ GENERAL ─────────────────────────────────┐ │
│ │ □ Start with Windows                      │ │
│ │ □ Start minimized to tray                 │ │
│ │ □ Show episode progress in status         │ │
│ └────────────────────────────────────────────┘ │
│                                                │
│ ╔═══════════════════════════╗                  │
│ ║   SAVE SETTINGS           ║                 │
│ ╚═══════════════════════════╝                  │
│                                                │
└──────────────────────────────────────────────┘
```

Each section is a manga panel with a caption strip header.

**Verify:** Settings load, save, and persist across app restarts.

---

### Task 23: CaptionStrip Custom Control

**Objective:** Create the reusable uppercase header bar used on all panels.

**File:** `Controls/CaptionStrip.xaml`

```xml
<UserControl x:Class="MALDiscordPresence.Controls.CaptionStrip"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Border Background="{StaticResource DarkRubyBrush}" Padding="10,5">
        <TextBlock Text="{Binding Title}"
                   Foreground="White"
                   FontFamily="Consolas, Courier New"
                   FontSize="11"
                   FontWeight="Bold"
                   TextTransform="Uppercase" />
    </Border>
</UserControl>
```

*(Note: WPF doesn't have TextTransform; use `Text="{Binding Title}"` and set the VM property to `.ToUpper()`, or use a value converter.)*

**Verify:** Caption strips render on all panels.

---

### Task 24: MangaPanel Custom Control

**Objective:** Reusable panel with hard border + offset shadow.

**File:** `Controls/PanelBorder.xaml`

```xml
<UserControl x:Class="MALDiscordPresence.Controls.PanelBorder">
    <!-- Two-border technique: outer border = shadow, inner border = panel -->
    <Border Background="Transparent"
            Margin="{StaticResource PanelShadowOffset}"
            BorderThickness="{StaticResource PanelBorderWidth}"
            BorderBrush="{StaticResource DarkRubyBrush}"
            CornerRadius="0">
        <Border Background="{StaticResource CardSurfaceBrush}"
                BorderThickness="{StaticResource PanelBorderWidth}"
                BorderBrush="{StaticResource DarkRubyBrush}"
                CornerRadius="0"
                Padding="{StaticResource PanelPadding}">
            <ContentPresenter Content="{Binding ElementName=root, Path=ChildContent}" />
        </Border>
    </Border>
</UserControl>
```

Actually, better as a simple `Style` on `Border` rather than a UserControl, since WPF UserControls with ContentPresenter can be tricky. The style approach in MangaControls.xaml (Task 6) handles this.

**Verify:** Panels render with ink borders and offset shadows.

---

### Task 25: System Tray Icon

**Objective:** App minimizes to system tray with right-click menu.

**File:** `Helpers/TrayIcon.cs`

**Features:**
- Uses `System.Windows.Forms.NotifyIcon` (via WinForms interop)
- Icon: custom `icon.ico`
- Right-click menu: "Open", "Status: Watching X", "Exit"
- Double-click tray icon: restore window
- Minimize to tray instead of taskbar (configurable in settings)

**Verify:** App minimizes to tray, tray icon shows context menu, double-click restores.

---

### Phase 4: Integration + Polish (Tasks 26-30)

---

### Task 26: Wire Up MAL Auth Flow

**Objective:** Connect login button to AuthService, handle callback, store tokens, transition to LibraryView.

**Steps:**
1. LoginView button → `AuthService.LoginAsync()`
2. AuthService opens browser, starts localhost listener
3. User authorizes → callback captures code → exchanges for tokens
4. Tokens saved via SecureStorage
5. `MainViewModel.IsAuthenticated = true` → navigates to LibraryView
6. On app startup: check SecureStorage for existing tokens, skip login if valid

**Verify:** Full OAuth flow works end-to-end. Browser opens, user authorizes, app receives token, transitions to library.

---

### Task 27: Wire Up Library Loading

**Objective:** On LibraryView load, fetch user's anime/manga list from MAL.

**Steps:**
1. LibraryViewModel.LoadList → calls MalService.GetAnimeListAsync()
2. Populate ObservableCollection
3. Bind to ItemsControl in LibraryView
4. Handle loading state (spinner), error state (retry button), empty state
5. Implement search: filter locally, debounce, then API search for broader results
6. Pagination: "Load More" button or infinite scroll

**Verify:** Library shows user's actual anime/manga list from MAL.

---

### Task 28: Wire Up Discord RPC

**Objective:** Connect DetailView controls to DiscordService for live status updates.

**Steps:**
1. DetailView "Start" button → create PresenceState → DiscordService.SetPresence()
2. Episode stepper → updates PresenceState.CurrentEpisode → re-calls SetPresence()
3. "Stop" button → DiscordService.ClearPresence()
4. MainViewModel.IsDiscordConnected ← DiscordService.ConnectionChanged
5. Status indicator in sidebar: green dot = connected, gray = disconnected
6. On app exit: DiscordService.Dispose() clears presence (ghost prevention)

**Verify:** Starting status shows correct presence on Discord. Changing episodes updates it live. Stopping clears it.

---

### Task 29: App Lifecycle + Startup

**Objective:** Handle app startup, minimize to tray, start with Windows.

**Steps:**
1. On startup: check SecureStorage for tokens
   - If valid → LibraryView
   - If expired → attempt refresh
   - If no tokens → LoginView
2. If "Start with Windows" enabled: add registry key at `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
3. If "Start minimized": show tray icon only on launch
4. Window close (X): minimize to tray instead of exit (with tray menu "Exit" for full quit)
5. Proper cleanup on exit: dispose DiscordService, save config

**Verify:** App starts correctly, remembers login, minimizes to tray, starts with Windows when configured.

---

### Task 30: Final Polish + Error Handling

**Objective:** Add error handling, loading states, and visual polish.

**Steps:**
1. **Network errors:** MAL API failures show user-friendly error panel (manga speech bubble style)
2. **Discord not running:** Show "Discord not detected" in sidebar, retry connection button
3. **Token expiry:** Auto-refresh silently, re-login prompt if refresh fails
4. **Loading skeletons:** Show halftone-pattern placeholder cards while library loads
5. **Keyboard navigation:** Tab through all controls, Enter to activate buttons
6. **Window resize:** Responsive grid columns (4 → 3 → 2 columns)
7. **Animations:** Panel slide-in on navigation (sharp, fast, no easing curves — manga cut style)
8. **Tooltip styling:** Manga-styled tooltips with hard borders

**Verify:** All error states handled gracefully. App feels polished and responsive.

---

## Discord Developer Portal Setup (Manual)

Before using the app, the user must:

1. Go to https://discord.com/developers/applications
2. Create new application → name it "MAL Status"
3. Copy the **Application ID**
4. Go to **Rich Presence** → **Art Assets** → upload:
   - `app_icon` (custom app icon for the small image, 128x128, PNG)
   - *(Large image = anime/manga cover from MAL, loaded dynamically via URL — no upload needed!)*
5. Paste Application ID in app settings

## MyAnimeList API Setup (Manual)

1. Go to https://myanimelist.net/apiconfig
2. Create new API client
3. Set callback URL to `http://localhost:54537/callback`
4. Copy **Client ID**
5. Paste Client ID in app settings

---

## Risks & Tradeoffs

| Risk | Mitigation |
|------|-----------|
| MAL API rate limiting (100 req/min) | Cache library locally, debounce search, only refresh on focus |
| Discord not running | Detect via IPC pipe connection, show status, retry |
| Token security | DPAPI encryption, never log tokens, clear on logout |
| MAL API breaking changes | Pin to v2, pin response models to current schema |
| WPF theming complexity | Use resource dictionaries, test all controls with manga theme |
| Windows-only | By design (WPF). Could port to Avalonia for cross-platform later |

## Open Questions

1. **Discord App ID**: Should we hardcode a shared app ID or require each user to create their own? (Recommendation: user creates their own for full control over assets)
2. **Auto-detection**: Should the app auto-detect when you start watching an episode (e.g., via Plex integration)? (Out of scope for V1, but architecturally supported)
3. **Multiple Discord accounts**: Discord only allows one RPC connection at a time. No multi-account support needed.
