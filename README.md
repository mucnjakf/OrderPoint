# <img src="orderpoint.svg" width="25"/> OrderPoint

### **👁️ Overview**
Order Management Web Application - 2026.

<p align="justify">
    OrderPoint is a multi-application order management system designed to streamline customer ordering and beverage service operations. The system includes a customer mobile app that allows users to browse menus and place orders, and an admin web app for managing products, categories, and orders in real time.
</p>

#

### **⚙️ Tech Stack**
- Hosting
  - Aspire
- Frontend
  - .NET 10 - ASP.NET Core Blazor App
  - MudBlazor
- Backend
  - .NET 10 - ASP.NET Core Minimal API
  - FluentValidation
  - MediatR  
  - Entity Framework Core
  - Scalar
  - OpenTelemetry
- Database
  - PostgreSQL

#

### **📱 Testing on a phone**
<p align="justify">
    Aspire serves the apps on <code>localhost</code> only, so a phone can't reach them directly. A Microsoft dev tunnel gives the bartender app and the customer app public HTTPS addresses that forward to your machine. Only the apps' pages go through it: they call the API from the server, so nothing else needs to be exposed.
</p>

**One-time setup**
```bash
winget install Microsoft.devtunnel           # open a new terminal afterwards, so devtunnel is on the PATH
devtunnel user login -g                      # GitHub account; drop -g for a Microsoft account
devtunnel create orderpoint --allow-anonymous
devtunnel port create orderpoint -p 7034 --protocol https   # bartender app
devtunnel port create orderpoint -p 7038 --protocol https   # customer app
```

To make the Admin's table QR codes open the customer app through the tunnel, store its address in the Admin's user secrets (the Admin reads it at startup):
```bash
dotnet user-secrets set "CustomerApp:PublicUrl" "https://orderpoint-7038.euw.devtunnels.ms" --project app/web/OrderPoint.Admin
```

**Every time**
1. Start the app: `aspire run --project app/host/OrderPoint.AppHost`.
2. In a second terminal, start the tunnel and leave it running: `devtunnel host orderpoint`. It prints the addresses, which stay the same every time:
   - Bartender app: `https://orderpoint-7034.euw.devtunnels.ms`
   - Customer app: `https://orderpoint-7038.euw.devtunnels.ms`
3. On the phone, open an address and tap Continue on the dev tunnels warning page (shown once). For the customer app, scan a table's QR code in the Admin (Tables, select a table), or open `/t/<table code>`, e.g. `https://orderpoint-7038.euw.devtunnels.ms/t/T7`.
4. Stop the tunnel with Ctrl+C when you're done: while it runs, anyone with the address can open the apps.

**Good to know**
- Images work on the phone too: their stored addresses point at the storage emulator on your machine, which a phone can't reach, so the bartender and customer apps serve them themselves under `/images` (the AppHost passes them the storage address as `Images__StorageUrl`). The Admin still loads them straight from storage.
- While `CustomerApp:PublicUrl` is set, the QR codes and the Admin's "Open" button go through the tunnel, so they only work while it runs. To go back to the local address: `dotnet user-secrets remove "CustomerApp:PublicUrl" --project app/web/OrderPoint.Admin`, then restart.
- If the app ports change, check them in the Aspire dashboard and update the tunnel's ports (`devtunnel port delete` / `devtunnel port create`).
- A tunnel unused for 30 days is deleted; run the `create` and `port create` commands again.
