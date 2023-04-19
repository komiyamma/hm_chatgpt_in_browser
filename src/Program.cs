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
            await Task.Delay(3000); // 2�b�ҋ@

            // �n���ꂽexe�̃t���p�X
            var basename = Path.GetFileNameWithoutExtension(exefullpath);

            while (true) // �������[�v
            {
                await Task.Delay(1000); // 1�b����

                // �uhidemaru�v�̕����������o�B������ hidemaru.exe �̖��O��ύX����悤�Ȑl�����Ă����삷��悤�ɂ���B
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
                        // �{���Ɉ�v���Ă���񂾂낤�ˁH �Ƃ����Ō�̔O����
                        if (proc?.MainModule?.FileName?.ToLower() == exefullpath.ToLower())
                        {
                            true_list.Add(proc);
                        }
                    }
                }

                // �풓���ǂ����`�F�b�N
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
                        Debug.WriteLine("�q�E�B���h�E����");// �q�E�B���h�E����Ȃ̂ŗL���ȏG�ۃG�f�B�^������B
                    }
                    else
                    {
                        Debug.WriteLine("�q�E�B���h�E�Ȃ�");
                        // �B���hidemaru�v���Z�X�͎q�E�B���h�E�Ȃ��Ȃ̂ŁA����͏풓�G�ۂ��Ȃɂ��ł���
                        break; // �I���
                    }
                }
                else
                {
                    Debug.WriteLine("�Q�q�ȏ�");
                    // Console.WriteLine("�Q�ȏ�v���Z�X����");
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
                // �� �t�@�C���ɖ��ߍ��ނ̂ł͂Ȃ��A�n���Ă���悤�ɂ���            o.ApiKey = builder.Configuration["OpenAI:ApiKey"];
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