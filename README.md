# <img src="orderpoint.svg" width="25"/> OrderPoint

### **👁️ Overview**
Order Management Web Application - 2026.

<p align="justify">
    OrderPoint is an order management system for a bar, made of five web apps that work together in real time. Guests scan the QR code on their table to browse the menu, order and pay from their phone, and follow their order live. Bartenders work the order queue and the tables map in the bar app, take orders at the tables and the counter in the bartender app, and handle counter sales, payments, refunds and receipts in the register app. The owner manages the menu, the team, the floor plan and the inventory (stock, recipes, suppliers, deliveries and counts) in the admin web app, and follows revenue, profit and service on a live dashboard.
</p>

#

### **⚙️ Tech Stack**
- Hosting
  - Aspire
  - Docker
- Frontend
  - .NET 10 - ASP.NET Core Blazor Web App (Interactive Server)
  - MudBlazor
  - SignalR Client
  - QRCoder
- Backend
  - .NET 10 - ASP.NET Core Minimal API
  - MediatR
  - FluentValidation
  - Entity Framework Core
  - ASP.NET Core Identity + JWT
  - SignalR
  - HybridCache
  - Scalar
  - OpenTelemetry
- Data
  - PostgreSQL
  - Redis
  - Azure Blob Storage (Azurite)

#

### **📖 Documentation**
<p align="justify">
    Everything about OrderPoint, from the features of every app to the architecture, the data model, the API and how to run it, is in the <a href="https://github.com/mucnjakf/OrderPoint/wiki">wiki</a>.
</p>