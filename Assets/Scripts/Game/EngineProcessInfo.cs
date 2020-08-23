using System.Diagnostics;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public class EngineProcessInfo : MonoBehaviour
    {
        public static ProcessStartInfo RunEngine(string enginePath)
        {
            var psi = new ProcessStartInfo();

            // psi.FileName = "C:\\Users\\leleleX\\Desktop\\LinuxHome\\minishogi_server\\YaneuraOu-KKPPT.exe";
            // psi.WorkingDirectory = "C:\\Users\\leleleX\\Desktop\\LinuxHome\\minishogi_server";

            UnityEngine.Debug.Log(enginePath);
            
            // full path
            psi.FileName = enginePath;

            // directory
            psi.WorkingDirectory = System.IO.Path.GetDirectoryName(enginePath);

            psi.UseShellExecute = false;        // シェルを使用せず子プロセスを起動
            psi.RedirectStandardInput = true;   // 子プロセスの標準入力をリダイレクトする
            psi.RedirectStandardOutput = true;  // 子プロセスの標準出力をリダイレクトする
            psi.RedirectStandardError = true;   // 子プロセスの標準エラー出力をリダイレクトする
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;

            return psi;

#if false
            process = Process.Start(psi);
            process.OutputDataReceived += PrintOutputData;
            process.ErrorDataReceived += PrintErrorData;

            // 標準出力・標準エラーの非同期読み込みを開始する
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            engineState = EngineState.None;
            process.StandardInput.WriteLine("usi");
#endif
        }

#if false
        void OnApplicationQuit()
        {
            if (!engineProcess.HasExited)
            {
                engineProcess.CloseMainWindow();
            }

            engineProcess.Close();
            engineProcess = null;
        }
#endif

    }
}
