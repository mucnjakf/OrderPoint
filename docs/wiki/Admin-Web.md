# Admin web

The owner's app, on a desktop. It covers reporting, orders and payments, the menu, the team, the floor plan and the whole inventory.

- **Project:** `app/web/OrderPoint.Admin` (Blazor Server)
- **Local address:** `https://localhost:7025`
- **Sign-in:** the admin account, created at startup from the configured email and password (see [Configuration](Configuration))
- **Navigation:** a drawer with four sections: **Overview** (Dashboard, Orders, Payments), **Management** (Bartenders, Items, Categories, Tables), **Inventory** (Stock, Deliveries, Suppliers, Counts) and **System** (Health, Account). The app bar holds the search and the health indicator.

## Across the app

- **Global search** (the search box or Ctrl+K / ⌘K) looks through orders, items, categories, bartenders, tables and pages at once, five results per group, with recent results when empty. Arrow keys and Enter open a result.
- **New order toasts:** "New order #128 · Table 4 · €23.50" on any page, with a View button.
- **Live updates:** every list, count and chart reloads silently when orders, the menu or the tables change.
- **Health indicator:** a coloured icon in the app bar ("All systems operational"), linking to the Health page.

## Overview

### Dashboard

A period switch (Today, Last 7 days, Last 30 days) drives most tiles; every change is compared with the previous period of the same length.

- **KPIs:** revenue, orders, average order, service time.
- **Revenue trend** (hourly for today, daily otherwise), **busiest times** (a weekday × hour heat map over the last 4 weeks), **revenue by category** (top 5 donut).
- **Payments by method:** cash, card at the register, card online, with shares.
- **Gross profit:** revenue, cost of goods, gross profit and margin, how much of the revenue has a known cost, and stock losses (waste and count differences).
- **Right now** (refreshes every 30 s): open orders by status, longest wait, today so far, and what waits at the register.
- **Low stock:** what is out or low, with a Reorder button.
- **Top items**, **bartender leaderboard** and **recent orders**, each opening details.

### Orders

A status pipeline (Pending, Accepted, Active, Completed, Declined, Cancelled) whose cards filter a timeline of order tickets. Tickets show the place, time, status, lines, bartender, total, payment, and for declined or cancelled orders why. Search, sort, a bartender filter and a payment filter (To pay, Nothing to pay, Paid, Refund due, Refunded). The details dialog shows everything about an order, including its source, who took it and a payment breakdown, and can **decline** an open order with a reason for the guest.

### Payments

Totals for cash, card at the register and card online in the chosen period, plus refunds due; below them every payment with search, sort and filters (method, status, who took it). Selecting a payment shows a receipt panel with Print and Order details.

## Management

- **Bartenders:** expandable rows with contact details, notes, orders and dates; a team strip on top. Create (with a temporary password), edit, upload a photo, reset a password, activate or deactivate (which hands their open orders back to the queue), delete (only without orders or payments), in bulk too. The Orders button lists a bartender's orders.
- **Items:** a card grid with picture, category, status, price, portion and orders count, and spotlight cards for the newest, last edited, highest and lowest price. Create, edit (with a price change by amount or percent), pictures, and bulk status, move to another category, adjust prices and delete. Item details show the item's orders and its **recipe** with cost and margin, edited in a recipe dialog.
- **Categories:** a table with the top 5 by items on top. Create, edit, pictures, bulk status and delete (only without items), and **Reorder** to set the menu order by drag and drop.
- **Tables:** the floor plan editor, see [Tables and floor plan](Tables-and-Floor-Plan).

## Inventory

See [Inventory](Inventory) for how it works.

- **Stock:** summary cards (stock value, low, out; the last two filter), and a stock sheet grouped by group with level bars, quantities in packs and base units, par and value. Each row: Details (history), Record waste, Correct quantity, Edit, Delete. **Reorder** lists what to buy per supplier and can record the delivery straight away.
- **Deliveries:** grouped by month with totals; record a delivery with its lines in packs and prices.
- **Suppliers:** contact cards with their stock items, how many need reordering and the last delivery.
- **Counts:** start a count for chosen groups, enter what is on the shelf on the count sheet (it saves as you go), then close it to apply the differences; closed counts show losses and gains.

## System

- **Health:** every component (API, database, storage, cache, the four other apps, and the developer tools) with its status, reloaded every 30 s.
- **Account:** profile picture, appearance, password, sessions, sign out.