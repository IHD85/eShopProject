using Microsoft.AspNetCore.RateLimiting;
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        builder.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("fixed", limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromSeconds(10); // hver 10 sekunder, et nyt vindue
                limiterOptions.PermitLimit = 20; // maks 20 anmodninger per vindue (per klient)
                limiterOptions.QueueLimit = 0; // ingen kø (429, hvis for mange request)
            });
        });

        builder.Services.AddHealthChecks();

        var app = builder.Build();
        
        app.MapHealthChecks("/health");

        app.UseRateLimiter();

        app.MapReverseProxy();

        app.Run();
    }
}