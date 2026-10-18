using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ticketing.Api.Audit;
using Ticketing.Api.Auth;
using Ticketing.Api.Common;
using Ticketing.Api.Configuration;
using Ticketing.Api.Data;
using Ticketing.Api.Tickets;
using Ticketing.Api.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<TicketOptions>()
    .Bind(builder.Configuration.GetSection(TicketOptions.Section))
    .ValidateOnStart();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>()
                 ?? throw new InvalidOperationException("Missing Jwt configuration section");

builder.Services.AddDbContext<TicketingDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("Ticketing"))
    .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<TicketLifecycleService>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddHostedService<TicketLifecycleWorker>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Convert.FromBase64String(jwtOptions.Secret)),
            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero,

            NameClaimType = CurrentUser.UsernameClaim,
            RoleClaimType = CurrentUser.RoleClaim
        };
    });

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<TimeProvider>((bearer, timeProvider) =>
        bearer.TokenValidationParameters.LifetimeValidator =
            (notBefore, expires, _, _) =>
            {
                var now = timeProvider.GetUtcNow().UtcDateTime;

                if (notBefore.HasValue && now < notBefore.Value)
                {
                    return false;
                }

                return !expires.HasValue || now < expires.Value;
            });

builder.Services.AddAuthorization();
builder.Services
    .AddControllers()
    .AddJsonOptions(json =>
        json.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
builder.Services.AddOpenApi(openApi =>
{
    openApi.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    openApi.AddOperationTransformer<AuthorizeSecurityRequirementTransformer>();
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(swagger =>
    {
        swagger.SwaggerEndpoint("/openapi/v1.json", "Ticketing API v1");
        swagger.DocumentTitle = "Ticketing API";
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "UP" })).AllowAnonymous();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Configuration.GetValue("SeedDemoUsers", false))
{
    await DemoUserSeeder.SeedAsync(app.Services);
}

app.Run();
