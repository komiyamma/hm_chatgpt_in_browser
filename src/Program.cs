using HmChatGptInBrowser.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpenAI.Net;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;
using System;
using System.Diagnostics;

namespace HmChatGptInBrowser
{
    public partial class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpClassName, IntPtr strWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, IntPtr lpClassName, IntPtr strWindowName);

        private static async Task CheckFindHidemaruWindowTasks(string exefullpath)
        {
            await Task.Delay(3000); // 2秒待機

            // 渡されたexeのフルパス
            var basename = Path.GetFileNameWithoutExtension(exefullpath);

            while (true) // 無限ループ
            {
                await Task.Delay(1000); // 1秒おき

                // 「hidemaru」の部分だけ抽出。万が一 hidemaru.exe の名前を変更するような人が居ても動作するようにする。
                Process[] proc_list = Process.GetProcessesByName(basename);
                if (proc_list.Length == 0)
                {
                    break;
                }

                List<Process> true_list = new List<Process>();
                foreach (var proc in proc_list)
                {
                    if (proc != null)
                    {
                        // 本当に一致しているんだろうね？ という最後の念押し
                        if (proc?.MainModule?.FileName?.ToLower() == exefullpath.ToLower())
                        {
                            true_list.Add(proc);
                        }
                    }
                }

                // 常駐かどうかチェック
                if (true_list.Count == 1)
                {

                    IntPtr hWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Hidemaru32Class", IntPtr.Zero);
                    if (hWnd == IntPtr.Zero)
                    {
                        break;
                    }

                    IntPtr hChild = FindWindowEx(hWnd, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    if (hChild != IntPtr.Zero)
                    {
                        Debug.WriteLine("子ウィンドウあり");// 子ウィンドウありなので有効な秀丸エディタがある。
                    }
                    else
                    {
                        Debug.WriteLine("子ウィンドウなし");
                        // 唯一のhidemaruプロセスは子ウィンドウなしなので、これは常駐秀丸かなにかである
                        break; // 終わる
                    }
                }
                else
                {
                    Debug.WriteLine("２子以上");
                    // Console.WriteLine("２つ以上プロセスあり");
                }
            }

            Environment.Exit(-1);
        }

    }

    public partial class Program
    {
        static WebApplication app = null;


        public static void Main(string[] args)
        {
            if (args.Length >= 4)
            {
                if (args[2] == "call_by_hidemaru") {
                   _ = CheckFindHidemaruWindowTasks(args[3]);
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
}