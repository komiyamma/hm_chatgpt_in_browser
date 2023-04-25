using HmChatGptInBrowser.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpenAI.Net;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;
using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace HmChatGptInBrowser;

public partial class Program
{
    static WebApplication app = null;


    public static void Main(string[] args)
    {
        if (args.Length >= 4)
        {
            if (args[2] == "call_by_hidemaru") {
               _ = WatcherHidemaruProcess(Int32.Parse(args[1]), args[3]);
            }
        }

        var builder = WebApplication.CreateBuilder(args);

        if (args.Length >= 2)
        {
            builder.WebHost.UseUrls($"http://localhost:{args[1]}");
        }

        string? key = Environment.GetEnvironmentVariable("OPENAI_KEY");
        if (key == null)
        {
            key = "";
        }
        if (args.Length >= 1)
        {
            key = args[0];
        }

        //Add OpenAI
        builder.Services.AddOpenAIServices(o =>
        {
            // ★ ファイルに埋め込むのではなく、渡してくるようにする            o.ApiKey = builder.Configuration["OpenAI:ApiKey"];
            o.ApiKey = key;
        });

        builder.Services.AddSignalR(o =>
        {
            o.EnableDetailedErrors = true;
            o.MaximumReceiveMessageSize = long.MaxValue;
        });

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        // ★ BlazorServerアプリへの接続や切断をイベントハンドラ的に監視する
        builder.Services.AddSingleton<CircuitHandler, CircuitHandlerService>();
        

        app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}