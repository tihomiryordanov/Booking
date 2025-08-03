using Booking.Application.Common.Interfaces;
using Booking.Application.Services.Implementation;
using Booking.Application.Services.Interface;
using Booking.Domain.Entities;
using Booking.Infrastructure.Data;
using Booking.Infrastructure.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Syncfusion.Licensing;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Booking.Application.Contact;
using Booking.Infrastructure.Emails;

var builder = WebApplication.CreateBuilder(args);

// Enhanced Security Headers Configuration
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});

// Database Context with enhanced security
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
            sqlOptions.CommandTimeout(30);
        });
    
    // Enable sensitive data logging only in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging(false);
    }
});

// Enhanced Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password policy
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredUniqueChars = 6;

    // Lockout policy
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User policy
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add Facebook Authentication
builder.Services.AddAuthentication()
    .AddFacebook(facebookOptions =>
    {
        facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
        facebookOptions.SaveTokens = true;
        
        // Request additional permissions/scopes
        facebookOptions.Scope.Add("email");
        facebookOptions.Scope.Add("public_profile");
        
        // Map additional claims
        facebookOptions.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        facebookOptions.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        facebookOptions.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        facebookOptions.ClaimActions.MapJsonKey("urn:facebook:first_name", "first_name");
        facebookOptions.ClaimActions.MapJsonKey("urn:facebook:last_name", "last_name");
        
        // Handle the callback
        facebookOptions.Events.OnCreatingTicket = context =>
        {
            var identity = (ClaimsIdentity)context.Principal.Identity;
            var profileImg = $"https://graph.facebook.com/{context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value}/picture?type=large";
            identity.AddClaim(new Claim("urn:facebook:picture", profileImg));
            return Task.CompletedTask;
        };
    });

// Enhanced Cookie Configuration with more flexible settings for payment flows
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Error/AccessDenied";
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    
    // Security settings - adjusted for payment callback compatibility
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.SameAsRequest 
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax; // Changed from Strict to Lax for external redirects
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
    
    // Session timeout
    options.Events.OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync;
});

// Configure external cookie options for OAuth providers
builder.Services.ConfigureExternalCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.SameAsRequest 
        : CookieSecurePolicy.Always;
});

// Anti-forgery token configuration with exemptions for payment callbacks
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__Host-X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax; // Changed from Strict to Lax
});

// Session configuration for additional security
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "__Host-Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax; // Changed from Strict to Lax
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// Security Headers
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// Register services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IVillaService, VillaService>();
builder.Services.AddScoped<IVillaNumberService, VillaNumberService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IStripeSessionService, StripeSessionService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
// Register HttpClient for ChatbotService
builder.Services.AddHttpClient<IChatbotService, ChatbotService>();

// Add MVC with selective antiforgery validation
builder.Services.AddControllersWithViews(options =>
{
    // Don't add global antiforgery filter - we'll apply it selectively
    // options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

//register stripe settings
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
SyncfusionLicenseProvider.RegisterLicense(builder.Configuration.GetSection("Syncfusion:Licensekey").Get<string>());

// Configure Google OAuth and Email settings
builder.Services.Configure<GoogleOAuthSettings>(
    builder.Configuration.GetSection("GoogleOAuth"));
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Register Google Token Service and Email Service
builder.Services.AddSingleton<IGoogleTokenService, GoogleTokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline with enhanced error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Index");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    // Still handle status codes in development for testing
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}

// Security middleware pipeline
//app.UseForwardedHeaders();

// Custom security headers middleware with exemptions for payment callbacks
//app.Use(async (context, next) =>
//{
//    // Skip strict security headers for payment callback endpoints
//    var path = context.Request.Path.Value?.ToLower();
//    var isPaymentCallback = path?.Contains("/payment/") == true || 
//                           path?.Contains("/booking/confirmation") == true ||
//                           path?.Contains("/stripe/") == true;

//    if (!isPaymentCallback)
//    {
//        // Security headers for regular requests
//        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
//        context.Response.Headers.Add("X-Frame-Options", "DENY");
//        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
//        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
//        context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
        
//        // Content Security Policy
//        var csp = "default-src 'self'; " +
//                  "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://js.stripe.com; " +
//                  "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
//                  "img-src 'self' data: https:; " +
//                  "font-src 'self' https://cdn.jsdelivr.net; " +
//                  "connect-src 'self' https://api.stripe.com; " +
//                  "frame-src https://js.stripe.com https://hooks.stripe.com; " +
//                  "frame-ancestors 'none';";
        
//        context.Response.Headers.Add("Content-Security-Policy", csp);
//    }
    
//    await next();
//});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Map routes
app.MapControllerRoute(
    name: "villaDetails",
    pattern: "VillaDetails/{id:int}",
    defaults: new { controller = "Home", action = "VillaDetails" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Catch-all route for 404 errors (should be last)
app.MapFallback(context =>
{
    context.Response.Redirect("/Error/404");
    return Task.CompletedTask;
});

SeedDatabase();
app.Run();

void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}

