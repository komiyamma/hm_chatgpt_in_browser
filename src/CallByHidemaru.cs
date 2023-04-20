using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HmChatGptInBrowser;

public partial class Program
{
    /*
    const int GWL_STYLE = -16;
    const long WS_VISIBLE = 0x10000000L;

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
    */
    
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

                /* これは意味がない。なぜなら共有ブラウザ枠でサーバーを呼び出すと、本体とは別途に秀丸プロセスが追加で割り当てられるから。
                // Hidemaru32Class の子ウインドウにさらに Hidemaru32Class がぶら下がっている
                IntPtr hWndNested = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Hidemaru32Class", IntPtr.Zero);
                if (hWndNested != IntPtr.Zero)
                {
                    IntPtr hBrowserWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "HM32CommonBrowserPane", IntPtr.Zero);
                    if (hBrowserWnd != IntPtr.Zero)
                    {
                        long hontai = (long)GetWindowLongPtr(hWndNested, GWL_STYLE);
                        long browser = (long)GetWindowLongPtr(hBrowserWnd, GWL_STYLE);

                        // 本体は表示されてるのに、ブラウザは表示されてない
                        if ((hontai & WS_VISIBLE) > 0 && (browser & WS_VISIBLE) == 0)
                        {
                            Trace.WriteLine("本体は表示されてるが共有ブラウザは非表示"); //
                            break;
                        }
                    }
                }
                */

                IntPtr hChild = FindWindowEx(hWnd, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                if (hChild != IntPtr.Zero)
                {
                    // Trace.WriteLine("子ウィンドウあり");// 子ウィンドウありなので有効な秀丸エディタがある。
                }
                else
                {
                    // Trace.WriteLine("子ウィンドウなし");
                    // 唯一のhidemaruプロセスは子ウィンドウなしなので、これは常駐秀丸かなにかである
                    break; // 終わる
                }
            }
            else
            {
                // Trace.WriteLine("２子以上");
                // Console.WriteLine("２つ以上プロセスあり");
            }
        }

        Environment.Exit(0);
    }

}

