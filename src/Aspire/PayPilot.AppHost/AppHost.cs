var builder = DistributedApplication.CreateBuilder(args);

var chat = builder.AddConnectionString("chat");

// Database: Postgres in a container, data kept between runs, pgAdmin for browsing
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();
var db = postgres.AddDatabase("paypilotdb");

// API: gets the database connection string, starts after the database is ready
var api = builder.AddProject<Projects.PayPilot_Api>("api")
    .WithReference(db)
    .WithReference(chat)
    .WaitFor(db);

// Angular: runs "npm start" (ng serve), gets the API address as environment variables
builder.AddViteApp("web", "../../Presentation/paypilot-web", "start")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
