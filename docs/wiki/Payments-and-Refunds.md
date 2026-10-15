# Payments and refunds

Every order has exactly one payment, for its full total. The payment is stored on the order itself: its payment status, method (cash or card), when it was paid, who took it at the register, and for cash how much was received.

## How orders get paid

| Order from | How it is paid | Recorded as |
| --- | --- | --- |
| Customer app, card | The guest pays when ordering; the order goes to the bar paid | Card, no bartender ("Paid online") |
| Customer app, cash | The order goes to the bar unpaid; the guest pays at the register, in person or through a bartender who takes the cash there | Cash or card, with the bartender who took it |
| Bartender app | Unpaid until the guest pays at the register | Cash or card, with the bartender |
| Register sale | Paid in the same step | Cash or card, with the bartender |

The Bar app and the Bartender app never take money.

### Card payments in the customer app

The payment provider is **simulated**: it approves every card except `4000 0000 0000 0002`, which is declined (`Payment.CardDeclined`, and no order is created). The card number must have 13 to 19 digits, the expiry is MM/YY and not in the past, the CVC has 3 or 4 digits. Card details go to the provider and nowhere else: they are never stored or logged.

### At the register

- **Cash:** the amount received must cover the total (at most €1,000); the change is shown and printed on the receipt.
- **Card:** the bar's own card terminal charges the card; the register only records that it was paid by card.

## Refunds

When a paid order is declined, cancelled or withdrawn:

| Paid | Refund |
| --- | --- |
| Online by card (customer app) | At once (simulated): **Refunded** |
| At the register (cash or card) | **Refund due**: it shows in the register's Refunds due list ("Hand back €12.00 in cash" or "Refund €12.00 on the terminal") until a bartender marks it **Refunded** |

## Where payments show up

| Place | What it shows |
| --- | --- |
| Register, **To pay** | Unpaid orders that still need paying (not declined or cancelled), oldest first |
| Register, **Refunds due** | Orders with money to hand back, from any day |
| Register, **Today** | The business day's cash, card at the register and card online, refunds due, per bartender, and every payment |
| Bartender app, Orders | Each order's payment chip |
| Customer app | "To pay at the register", "Paid by card", "Refunded to your card", ... on the order |
| Admin, **Payments** | Totals per method for a period and every payment, with filters and a receipt panel |
| Admin, Orders | A payment filter and a payment breakdown in the order details |
| Admin, Dashboard | Revenue, payments by method, register status in Right now |

## Revenue

Revenue means **money paid**: orders with status Paid, dated by when they were paid. Refunded payments and refunds due drop out of revenue. Order counts, on the other hand, count every order by when it was placed.

## Receipts

The register shows a receipt after each payment and can print it: OrderPoint, the order number and place, the date and time paid, the lines, the total, how it was paid (cash received and change, or card), "Refunded" if it was, who served it, and "Thank you!". It prints on any printer, including a receipt printer. The Admin's Payments page can print the same receipt.

See the payment states in [Order lifecycle](Order-Lifecycle).