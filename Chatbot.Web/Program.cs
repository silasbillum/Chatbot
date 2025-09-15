using Chatbot.Core;
using Chatbot.Core.Chatbot;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
var csvPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "trainingData.csv");

// Registrer NLUService som singleton
builder.Services.AddSingleton<Chatbot.Core.NLU.NLUService>(provider =>
    new Chatbot.Core.NLU.NLUService(csvPath)
);

builder.Services.AddSingleton<Chatbot.Core.Chatbot.ChatbotService>(provider =>
    new Chatbot.Core.Chatbot.ChatbotService(csvPath)
);

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
