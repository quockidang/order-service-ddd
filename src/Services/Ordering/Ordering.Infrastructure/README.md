ASPNETCORE_ENVIRONMENT=Production dotnet ef database update
dotnet watch run --environment "Development"
dotnet restore
dotnet build
Migration commands for Ordering API:
cd into Ordering folder
dotnet ef migrations add "SampleMigration" -p Ordering.Infrastructure --startup-project Ordering.API -o Persistence/Migrations
dotnet ef migrations remove -p Ordering.Infrastructure --startup-project Ordering.API
dotnet ef database update -p Ordering.Infrastructure --startup-project Ordering.API


docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d --remove-orphans --build