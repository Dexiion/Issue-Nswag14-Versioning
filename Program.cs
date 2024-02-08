using testSwagger;

var builder = Host.CreateDefaultBuilder()
    .ConfigureWebHostDefaults(webBuilder => {
        webBuilder
            .UseStartup<Startup>();
    });
var app = builder.Build();
app.Run();
