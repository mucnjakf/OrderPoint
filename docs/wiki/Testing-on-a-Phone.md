# Testing on a phone

Aspire serves the apps on `localhost` only, so a phone can't reach them directly. A **Microsoft dev tunnel** gives every app (Admin, Register, Bar, Bartender, Customer) a public HTTPS address that forwards to your machine. The AppHost declares it as the `tunnel` resource (Aspire's Dev Tunnels integration).

Only the apps' pages go through the tunnel. The apps call the API from their own servers, so the API, the database, the storage and the cache stay private.

## One-time setup

Install the devtunnel CLI and sign in:

```bash
winget install Microsoft.devtunnel           # Windows; open a new terminal afterwards so devtunnel is on the PATH
devtunnel user login -g                      # GitHub account; drop -g for a Microsoft account
```

On macOS use `brew install --cask devtunnel`; on Linux, `curl -sL https://aka.ms/DevTunnelCliInstall | bash`.

To make the Admin's table QR codes open the customer app through the tunnel, store its address in the Admin's user secrets (read at startup):

```bash
dotnet user-secrets set "CustomerApp:PublicUrl" "https://orderpoint-7038.euw.devtunnels.ms" --project app/web/OrderPoint.Admin
```

## Every time

1. Start the system: `aspire run --project app/host/OrderPoint.AppHost`.
2. The `tunnel` resource doesn't start with the rest. Start it in the Aspire dashboard (▶ on its row). The first time it creates the tunnel `orderpoint`; after that the addresses stay the same:

   | App | Address |
   | --- | --- |
   | Admin web | `https://orderpoint-7025.euw.devtunnels.ms` |
   | Register app | `https://orderpoint-7032.euw.devtunnels.ms` |
   | Bar app | `https://orderpoint-7034.euw.devtunnels.ms` |
   | Bartender app | `https://orderpoint-7036.euw.devtunnels.ms` |
   | Customer app | `https://orderpoint-7038.euw.devtunnels.ms` |

3. On the phone, open an address and tap **Continue** on the dev tunnels warning page (shown once).
4. For the customer app, scan a table's QR code in the Admin (Tables, select a table) or open `/t/<table code>`, e.g. `https://orderpoint-7038.euw.devtunnels.ms/t/T7`.
5. **Stop the `tunnel` resource** in the dashboard when you're done (stopping Aspire stops it too): while it runs, anyone with an address can open the apps.

## Good to know

- **Images work on the phone too.** Stored image addresses point at the storage emulator on your machine, which a phone can't reach, so every app serves images itself under `/images` (the AppHost passes them the storage address).
- **QR codes follow `CustomerApp:PublicUrl`.** While it is set, the QR codes and the Admin's "Open" button go through the tunnel and only work while it runs. To go back to local addresses:

  ```bash
  dotnet user-secrets remove "CustomerApp:PublicUrl" --project app/web/OrderPoint.Admin
  ```

  then restart the Admin.
- **Sign-in errors from the tunnel resource:** run `devtunnel user login -g` again.
- **A different tunnel:** the AppHost names the tunnel `orderpoint` in the West Europe region. If you use another id or region, the addresses change and so does `CustomerApp:PublicUrl`.
- **Blazor needs a steady connection.** If the phone loses Wi-Fi, the app shows a reconnect dialog and reloads what it shows once it is back.