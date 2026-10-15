# Database and migrations

OrderPoint keeps its data in PostgreSQL through EF Core. The schema lives in code (one configuration class per entity) and changes through migrations, which the API applies by itself when it starts.

## Where things are

| What | Where |
| --- | --- |
| DbContext (also the unit of work) | `app/api/OrderPoint.Infrastructure/EfCore/ApplicationDbContext.cs` |
| Entity configurations | `app/api/OrderPoint.Infrastructure/EfCore/EntityTypeConfiguration/` |
| Migrations | `app/api/OrderPoint.Infrastructure/EfCore/Migrations/` |
| Design-time factory (for `dotnet ef`) | `ApplicationDbContextFactory`, pointing at `localhost:5432`; fine for `migrations add`, which doesn't connect |
| Applying migrations | `app.ApplyMigrations()` in the API's `Program.cs`, on every startup |

The schema itself is described on [ER diagram](ER-Diagram).

## Changing the schema

1. Change the entity (Domain) and its configuration (Infrastructure). Every property is configured: `HasMaxLength`, `IsRequired`, `HasPrecision(10, 2)` for money, unique indexes, and `OnDelete(DeleteBehavior.Restrict)` for relationships.
2. Create a migration from the repository root:

   ```bash
   dotnet ef migrations add <Name> \
     --project app/api/OrderPoint.Infrastructure \
     --startup-project app/api/OrderPoint.Api \
     --output-dir EfCore/Migrations \
     --configuration Release
   ```

3. Read the generated migration before using it. When existing data needs filling in, add it to the migration as SQL. For example, `Add_Payment_To_Order` marked older orders as paid by card, and `Add_Areas_Tables_And_Fixtures` created an area and a table for every code that orders had used.
4. Restart the API resource (or `aspire run`); it applies the migration on startup. Don't run `dotnet ef database update`.

**Naming:** `<Verb>_<What>`, e.g. `Add_Item`, `Add_ImageUrl_To_Category`, `Add_Category_Description_And_Status`, `Remove_ShortDescription_From_Item`, `Restrict_Category_Delete`.

**Why `--configuration Release`:** while Aspire runs, the Debug build output is locked by the running apps; building in Release avoids "file is being used by another process" errors (MSB3021/MSB3027).

## Looking at the data

- **pgweb**, linked from the `pgweb` resource in the Aspire dashboard, browses tables and runs SQL.
- Any PostgreSQL client can connect to `localhost:59286`, database `order-point-db`, user `postgres`. The password is a parameter Aspire generated and stored in the AppHost's user secrets, and it is shown in the dashboard's resource details.
- Enums are stored as integers; their values are on [ER diagram](ER-Diagram#enum-values).

## Resetting

PostgreSQL and the storage emulator run as **persistent containers with volumes**, so the data survives restarts of Aspire. To start from an empty database:

1. Stop Aspire.
2. Remove the container and its volume:

   ```bash
   docker ps -a --filter name=postgres     # find the container
   docker rm -f <container>
   docker volume ls                        # find the volume ending in -postgres-data
   docker volume rm <volume>
   ```

3. Run Aspire again: the container is recreated, the API applies every migration and seeds the admin.

Images live in the storage emulator's own volume; remove it the same way to clear them. Redis has no volume and is empty after every restart.

## Rules of thumb

- Never edit a migration that has been applied; add a new one.
- Keep validation limits in sync: a new `HasMaxLength` also belongs in the API validator, the domain rule where relevant, and the apps' forms (see [Validation rules](Validation-Rules)).
- Data fixes on a real database: write the SQL, run it once inside `BEGIN; ... ROLLBACK;` to check what it touches, then with `COMMIT;`.