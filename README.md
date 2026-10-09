<div align="center">

# 🧟‍♂️ KF2Monitor

**A highly customizable home screen widget for Android that displays the real-time status of Killing Floor 2 MOD-EU servers, featuring advanced visual settings and extra tools.**

[![Platform](https://img.shields.io/badge/Platform-Android-3DDC84?style=for-the-badge&logo=android&logoColor=white)]()
[![Built with Avalonia](https://img.shields.io/badge/Built_with-Avalonia_UI-purple?style=for-the-badge)](https://avaloniaui.net/)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](https://opensource.org/licenses/MIT)

[Features](#-features) • [Installation](#-installation) • [Screenshots](#-screenshots) • [Contributing](#-contributing) • [Acknowledgments](#-acknowledgments)

</div>

---

## 📖 About The Project

**KF2Monitor** is a must-have tool for regular players of the Killing Floor 2 MOD-EU community. It brings the server directly to your home screen, allowing you to instantly check player counts, current maps, and ongoing waves without launching the game. 

Beyond just displaying stats, it acts as a personal tracking assistant—notifying you when your friends come online or when your favorite server gets crowded.

## ✨ Features

*   📊 **Real-time Server Status:** Live monitoring of player counts, current wave progress, and active maps across all MOD-EU servers.
*   🎨 **Ultimate Customization:** Tailor the widget to fit your home screen perfectly:
    *   Custom background images (with opacity and fade controls).
    *   Adjustable corner radii and UI element swapping (Left/Right, Wave/Players).
    *   Full RGB color picker for theme and transparency adjustments.
*   🔔 **Smart Notifications:**
    *   **Player Tracking:** Enter your friends' exact nicknames and get notified the moment they join or leave a server.
    *   **Crowd Alerts:** Set a player threshold and receive an alert when a server fills up.
*   ☁️ **Cloud Synchronization:** Automatically fetches the latest server IPs and Ports from the cloud if the server infrastructure changes.
*   🔄 **Built-in Auto-Updater:** Checks for updates daily and allows you to download and install new versions directly from within the app.
*   🌍 **Multi-language Support:** Translated into 9 languages including English, German, French, Polish, Japanese, and more.

## 🚀 Installation

### Option 1: Direct Download (Recommended)
1. Go to the [Releases](https://github.com/ext4n/KF2Monitor/releases) page.
2. Download the latest `KF2Monitor.apk` file.
3. Open the file on your Android device and select "Install" (you may need to allow installations from unknown sources).

### Option 2: Build from Source
If you want to compile the app yourself, you will need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and the Avalonia UI workload.

```bash
# Clone the repository
git clone https://github.com/ext4n/KF2Monitor.git

# Navigate into the project directory
cd KF2Monitor

# Build the Android project
dotnet build KF2Monitor.Android -c Release
```

## 🛠️ Tech Stack

*   **Framework:** [Avalonia UI](https://avaloniaui.net/) (Cross-platform XAML framework)
*   **Language:** C# 12 / .NET 10
*   **Architecture:** MVVM using `CommunityToolkit.Mvvm`
*   **Networking:** Standard `UdpClient` for querying Source Engine/Unreal Engine server protocols.

## 🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 🏆 Acknowledgments

This project was made possible through the immense support of many amazing people. 
* A huge, heartfelt thank you to everyone involved in the development and growth! 
* Special, boundless gratitude to the founders, admins, and managers of the MOD-EU project — **Edvis** and **Wyvern**. Your endless dedication and belief make this community truly alive and awesome!
* Check out the official community at [MOD-EU DISCORD](https://discordapp.com/invite/Ps3jqRr).

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

---
<div align="center">
  <i>Idea & Development by EXT4N</i>
</div>
