using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HmChatGptInBrowser;

public partial class Program
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpClassName, IntPtr strWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, IntPtr lpClassName, IntPtr strWindowName);

    private static async Task WatcherHidemaruProcess(int port, string exefullpath)
    {
        for ( int i=0; i<10; i++)
        {
            await Task.Delay(600); // 0.6秒待機

            // 接続者数が1以上になっていたら、誰か接続したってことで初回接続待機をする必要なしとみなして break;
            if (CircuitHandlerService.RemainingTotalConnections > 0)
            {
                break;
            }
        }

        // 渡されたexeのフルパス
        var basename = Path.GetFileNameWithoutExtension(exefullpath);

        while (true) // 無限ループ
        {
            await Task.Delay(1000); // 1秒おき

            // ★ Blazerアプリの視点で判定する。
            // 接続者数が0になっていたら break;
            // ブラウザなどが強制終了した場合には検知できないので完全ではない。
            // しかし、正常にブラウザ枠を閉じたのであれば、これが最もスマートに検知できるだろう
            if (CircuitHandlerService.RemainingTotalConnections == 0)
            {
                break;
            }

            // ★ このアプリは関係なく、hidemaruの使用状況から判定する。
            // これにより、汎用のChromeやEdgeがhttp接続で該当のportを使用して接続していたとしても、
            // 秀丸エディタが全部終了していたら、このアプリも終了するといったことが可能となる。
            // (常駐秀丸は起動しっぱなしでもよい)

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

            if (true_list.Count == 0)
            {
                break;
            }

            // 常駐かどうかチェック。実はほぼここは実行されない。理由は、共有ブラウザ枠を一度起動すると、
            // 常駐以外でも最低２つ秀丸プロセスが起動する形となるため。
            if (true_list.Count == 1)
            {
                // 基本プロセス１つあるんだから、これは無いはずだが... 一応将来 hidemaru.exe なんだけど Hidemaru32Class を持たないといった実装があるなら
                // (ちなみに、ストアアプリ版は、Hidemaru32Class ではなく Hidemaru32Class_Appx だが、初回公開以降、更新されてないようなので無視
                IntPtr hWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Hidemaru32Class", IntPtr.Zero);
                if (hWnd == IntPtr.Zero)
                {
                    // Trace.WriteLine("Hidemaru32Classなし");
                    break;
                }

                // 常駐秀丸は、「Hidemaru32Class」の下に子ウィンドウは持たないので、この判定が効く
                IntPtr hChild = FindWindowEx(hWnd, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                if (hChild == IntPtr.Zero) {
                    // Trace.WriteLine("これは有効な編集エリアを持つ有効な秀丸エディタのウィンドウハンドルではない。常駐秀丸か何かである");
                    break; // 終わる
                }
            }
            else
            {
                // Trace.WriteLine("2プロセス以上");
            }
        }

        Environment.Exit(0);
    }

}

