using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using SeriesMvc.Data;
using SeriesMvc.Services; // RedisCacheService'in yer aldığı ad uzayını ekleyin

var builder = WebApplication.CreateBuilder(args);

// Redis Cache yapılandırması
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "SampleInstance"; // İsteğe bağlı isim
});

// ICacheService ve RedisCacheService’i servislere ekleyin
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Diğer servisleri eklemeye devam edin
builder.Services.AddDbContext<SeriesMvcContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SeriesMvcContext")
    ?? throw new InvalidOperationException("Connection string 'SeriesMvcContext' not found.")));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Diğer kodlar (örneğin, Identity, DbContext, Controller) burada tanımlanır
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
