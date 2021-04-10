using System.Diagnostics;
using Assets.Scripts.Shogi;

namespace Assets.Scripts.LocalEngine {
    public class LocalEngineProcess {
        private Process Process { get; set; } = null;

        public Move BestMove { get; set; } = Move.NONE;

        public bool Thinking { get; set; } = false;

        public bool ReadyOk { get; private set; } = false;

        public void RunEngine(string path) {
            var psi = new ProcessStartInfo();
            UnityEngine.Debug.Log(path);

            // full path
            psi.FileName = path;

            // directory
            psi.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
            psi.UseShellExecute = false;        // シェルを使用せず子プロセスを起動
            psi.RedirectStandardInput = true;   // 子プロセスの標準入力をリダイレクトする
            psi.RedirectStandardOutput = true;  // 子プロセスの標準出力をリダイレクトする
            psi.RedirectStandardError = true;   // 子プロセスの標準エラー出力をリダイレクトする
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;

            Process = Process.Start(psi);
            Process.OutputDataReceived += OutputDataReceived;
            Process.ErrorDataReceived += ErrorDataReceived;

            // 標準出力・標準エラーの非同期読み込みを開始する
            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();

            ReadyOk = false;
            Thinking = false;
            Process.StandardInput.WriteLine("usi");
        }

        public void QuitEngine() {
            if (Process == null)
                return;

            // 標準出力・標準エラーの非同期読み込みを終了する
            Process.CancelOutputRead();
            Process.CancelErrorRead();

            Process.StandardInput.WriteLine("stop");
            Process.StandardInput.WriteLine("quit");
            Process = null;
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e) {
            if (Process == null || string.IsNullOrEmpty(e.Data))
                return;

            string[] cmd = e.Data.Split(' ');
            switch (cmd[0]) {
            case "usiok":
                Process.StandardInput.WriteLine("isready");
                break;

            case "readyok":
                Process.StandardInput.WriteLine("usinewgame");
                ReadyOk = true;
                break;

            case "bestmove":
                UnityEngine.Debug.Log(e.Data);
                BestMove = Util.ToMove(cmd[1]);
                break;

            default:
                break;
            }
        }

        private void ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            throw new System.NotImplementedException();
        }

        public void Think(string positionCommand) {
            if (Process == null || Thinking)
                return;

            Thinking = true;
            UnityEngine.Debug.Log("> " + positionCommand);

            Process.StandardInput.WriteLine(positionCommand);
            Process.StandardInput.WriteLine("go byoyomi 2000");
        }
    }
}
