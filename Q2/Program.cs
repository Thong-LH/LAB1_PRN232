using Q2;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient(); // Register HttpClient factory

//Initialize UrlUtilities with configuration
//DO NOT change this code
Utilities.Initialize(builder.Configuration);
//End

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Redirect root to /RoomType
app.MapGet("/", () => Results.Redirect("/RoomType"));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=RoomType}/{action=Index}/{id?}");

app.Run();
