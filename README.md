<p align="center"><img src="assets/subtitle-tracker.png" width="144" alt="Subtitle Tracker logo: a caption bubble with a check mark"></p>

# Dreamstreaming Subtitle Tracker

Find movies and episodes with missing subtitles in Jellyfin Server 12.4. After your daily Wyzie Subs task runs, open the tracker to see which titles still need a subtitle track.

## 📦 Install from the Jellyfin catalog

1. Sign in as an administrator and open **Dashboard → Plugins → Repositories**.
2. Add a repository named **Dreamstreaming** and paste the following URL into **Repository URL**:

   ```text
   https://github.com/certified-dumbass/subtitle-checker/releases/latest/download/manifest.json
   ```

3. Save, open **Catalog**, select **Dreamstreaming Subtitle Tracker**, and click **Install**.
4. Restart Jellyfin when convenient, then open the tracker from the dashboard.

Catalog installation requires no API key or development tools.

## 🔎 Check subtitle coverage

The plugin controls are currently in Dutch. The default view lists titles with missing subtitles.

| Control | What it does |
| --- | --- |
| **Ondertiteling** | Select **Nee** (No) for titles with zero indexed subtitle tracks or **Ja** (Yes) for titles with one or more. |
| **Type** | Choose **Films** (Movies), **Afleveringen** (Episodes), or **Alles** (Both). Episode rows include the series name and available season and episode numbers. |
| **Vorige / Volgende** | Move through results in pages of up to 50, ordered by sort title. The counter shows the visible range and the total for your filters. |
| **Vernieuwen** | Refresh the results from Jellyfin’s index for the libraries your account can access. |

Any language counts. For example, a movie with only a German subtitle track appears under **Ja**. Jellyfin can index embedded tracks and external files such as SRT subtitles. Burned-in captions are part of the video image and do not count as a separate track.

Subtitle downloads stay with your existing subtitle provider. The tracker reads Jellyfin’s index without recording watch history. If a newly downloaded track is missing, check the title’s media information, rescan its library if needed, and click **Vernieuwen**.

## 🧰 Manual installation

Download the plugin ZIP from [Releases](https://github.com/certified-dumbass/subtitle-checker/releases/latest), extract the DLL into its own folder inside the `plugins` directory of your Jellyfin data folder, and restart Jellyfin. To uninstall, use the plugin dashboard.

## 🛠️ Build on Windows

Install the .NET SDK matching the target framework in your server’s `jellyfin.runtimeconfig.json`. Open PowerShell in the extracted source directory and run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1
```

For a custom server location, append `-ServerDir "D:\Jellyfin\Server"`. The compiled DLL is written to `output\`.

## 🚀 Publish a release

The GitHub Actions workflow builds against assemblies from the official Jellyfin 12.4 container. Each release includes the plugin ZIP and a Jellyfin manifest with the ZIP’s MD5 checksum. Bump the version in `Tracker.csproj` before pushing a new plugin version to `main`.
