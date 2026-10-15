# Inventory

The stock behind the bar: what is on the shelves, what it cost, what the drinks use, and what to buy. Everything is managed in the [Admin web](Admin-Web); the other apps only feel its effects (items selling out by themselves).

## Stock items

A stock item is anything the bar buys and uses: a gin, a keg of beer, limes, sugar syrup.

| Field | Meaning |
| --- | --- |
| **Name** | Unique, as on the bottle or invoice |
| **Group** | Spirits, Beer, Wine, Mixers, Syrups, Garnish or Other; the stock sheet is grouped by it |
| **Unit** | The base unit every quantity is in: millilitres, grams or pieces. It can't change after creation, because the history is written in it |
| **Pack** | How it is bought: a name ("bottle", "keg", "case") and the base units in one pack (700 ml, 30,000 ml, 24 pieces) |
| **Cost per pack** | In euros, as invoices show it; updated by deliveries |
| **Par level** | At or below this quantity it counts as low and is suggested for reordering |
| **Quantity** | What is on the shelf, in base units. It may go below zero, because served drinks are never refused |
| **Supplier** | Where it is usually bought, optional |
| **Status** | Active or Inactive |

From these the app shows the **unit cost** (cost per pack ÷ pack size), the **value** on the shelf (never negative) and the **level**: **Out** at zero or less, **Low** at or below par, otherwise **Ok**. Quantities are shown both ways, e.g. "3.4 kegs · 102 L".

## Movements

Every change to a quantity is a **movement**, and movements are never edited. Each one records the signed change, the quantity after it and the unit cost at that moment, so later reports value it at the cost of its time.

| Type | Created by |
| --- | --- |
| **Opening** | Creating a stock item with an opening quantity |
| **Waste** | Recording waste (spilled, broken, expired, given away) |
| **Correction** | Setting the quantity to what is really there |
| **Sale** | Completing an order whose items have recipes ("Order #128") |
| **Delivery** | Recording a delivery |
| **Count** | Closing a stock count with a difference |

A stock item's details show its whole history. A stock item with movements other than its opening can't be deleted (deactivate it instead), nor one used in a recipe or a count.

## Recipes

A menu item's **recipe** says what one serving takes from the stock, for example a Gin & Tonic: 40 ml gin, 100 ml tonic, 1 lime wedge. With a recipe:

- **Cost and margin:** the item's cost is the sum of its ingredients at their unit cost; item details show "Cost · Price · Margin (n%)".
- **Selling takes stock off:** when an order is **completed**, every line's recipe is taken off the stock as Sale movements. Completion is never refused for lack of stock.
- **Sold out by itself:** an item is sold out while any ingredient has less than one serving left. Guests can't see or order it, staff see a "Sold out" chip, and it comes back as soon as stock does (after a delivery, correction or count). This happens live, without anyone touching the item's status.

An item without a recipe has no cost and its stock isn't tracked. Deleting an item deletes its recipe.

## Suppliers and deliveries

- **Suppliers** have a unique name, a contact, phone, email, note and status. Each shows its stock items, how many need reordering and its last delivery. A supplier with deliveries can't be deleted.
- **Deliveries** record what came in: the supplier, the date (not later than tomorrow), an optional invoice number and note, and lines in packs with the price per pack. A delivery is never edited or deleted. Recording one adds the stock as Delivery movements valued at the delivery's own price and updates each stock item's **average cost**: the stock already on the shelf and the new packs averaged by quantity (stock below zero counts as none).

## Reordering

The **Reorder** list groups every active stock item at or below par by supplier ("No supplier" last). For each it suggests enough **whole packs to get back to twice the par level** (at least one) and estimates the cost. "Record delivery" opens the delivery form already filled with the suggestions.

## Stock counts

A count checks the shelves against the system.

1. **Start** a count for chosen groups (at most one count is open at a time). It lists every active stock item of those groups.
2. **Enter** what is on the shelf on the count sheet, as full packs plus the opened amount (e.g. 2 bottles + 300 ml). Each figure saves when the field is left. The expected quantity is taken **when the figure is entered**, so the bar can stay open during the count: sales after that still come off.
3. **Close** the count: every counted line with a difference becomes a Count movement, valued at the cost when it was counted. Lines not counted stay as they are. An open count can also be discarded.

Closed counts show how many lines were counted, the difference in euros, and the losses and gains.

## On the dashboard

- **Gross profit:** revenue, **cost of goods** (the period's Sale movements at their stored cost), gross profit and margin, and how much of the revenue comes from items with a recipe (until every item has one, the margin looks better than it is).
- **Stock losses:** the value of waste, and of count and correction differences.
- **Low stock:** what is out or low, with the Reorder list one click away.

## Limits

| Value | Limit |
| --- | --- |
| Quantities, pack size, par | Up to 1,000,000 base units, 3 decimals |
| Cost per pack | Up to €100,000, 2 decimals |
| Recipe | Up to 30 ingredients, each stock item once, quantities above zero |
| Delivery | Up to 50 lines, each stock item once |
| Notes | 200 characters |