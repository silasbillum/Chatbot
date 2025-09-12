using Chatbot.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Chatbot service - singleton for demo. Provide path relative to content root.
var csvPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "trainingData.csv");
builder.Services.AddSingleton(new ChatbotService(csvPath));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
