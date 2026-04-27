using FFMpegCore;
using FFMpegCore.Enums;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource _cts;

        public Form1()
        {
            InitializeComponent();
        }

        public void FinalizarNginx()
        {
            // O Nginx no Windows geralmente aparece como "nginx" no Gerenciador de Tarefas
            foreach (var processo in Process.GetProcessesByName("nginx"))
            {
                try
                {
                    processo.Kill();
                    processo.WaitForExit(3000); // Aguarda até 3 segundos para fechar
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao finalizar Nginx: {ex.Message}");
                }
            }
        }

        public void FinalizarFfmpeg()
        {
            // O Nginx no Windows geralmente aparece como "nginx" no Gerenciador de Tarefas
            foreach (var processo in Process.GetProcessesByName("ffmpeg"))
            {
                try
                {
                    processo.Kill();
                    processo.WaitForExit(3000); // Aguarda até 3 segundos para fechar
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao finalizar Nginx: {ex.Message}");
                }
            }
        }

        public void StartNginx()
        {
            FinalizarNginx();

            string baseDir = $"{AppDomain.CurrentDomain.BaseDirectory}Nginx\\";
            Process _nginxProcess = new Process();
            _nginxProcess.StartInfo.FileName = $"{baseDir}nginx.exe";
            _nginxProcess.StartInfo.WorkingDirectory = baseDir;
            _nginxProcess.StartInfo.CreateNoWindow = true;
            _nginxProcess.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartNginx();
            var ret = ObterDispositivosComId("Stereo");
        }



        public DispositivoFFmpeg ObterDispositivosComId(string search)
        {
            var dispositivos = new List<DispositivoFFmpeg>();

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "D:\\Labs\\Ffmpeg\\ffmpeg.exe",
                Arguments = "-list_devices true -f dshow -i dummy",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (Process process = Process.Start(startInfo))
            {
                string linha;
                string ultimoNomeEncontrado = null;

                while ((linha = process.StandardError.ReadLine()) != null)
                {
                    // Procura por linhas que tenham aspas (onde estão os nomes e IDs)
                    if (linha.Contains("\""))
                    {
                        var match = Regex.Match(linha, "\"([^\"]*)\"");
                        if (match.Success)
                        {
                            string valor = match.Groups[1].Value;

                            // Se a linha contém "Alternative name", ela é o ID do nome anterior
                            if (linha.Contains("Alternative name"))
                            {
                                if (ultimoNomeEncontrado != null)
                                {
                                    dispositivos.Add(new DispositivoFFmpeg
                                    {
                                        Nome = ultimoNomeEncontrado,
                                        Id = valor
                                    });
                                    ultimoNomeEncontrado = null; // Reseta para o próximo par
                                }
                            }
                            else
                            {
                                // É um nome amigável. Guardamos para esperar o ID na próxima linha
                                ultimoNomeEncontrado = valor;
                            }
                        }
                    }
                }
                process.WaitForExit();
            }
            return dispositivos.Where(x => x.Nome.ToUpper().Contains(search.ToUpper())).FirstOrDefault();
        }

        public async Task IniciarCapturaDesktopSemAudio()
        {
            string baseDir = $"{AppDomain.CurrentDomain.BaseDirectory}ffmpeg";

            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = baseDir });

            bool isRunnning = Process.GetProcessesByName("ffmpeg").Any();

            if (!isRunnning)
            {
                await FFMpegArguments
                    .FromFileInput("desktop", false, options => options
                        .WithCustomArgument("-f gdigrab")
                        .WithCustomArgument("-framerate 20")
                        .WithCustomArgument("-thread_queue_size 4096"))
                    .OutputToFile("rtmp://localhost/live/stream", false, options => options
                        .WithVideoCodec(VideoCodec.LibX264)
                        .WithSpeedPreset(Speed.UltraFast)
                        .WithCustomArgument("-pix_fmt yuv420p")
                        .WithCustomArgument("-map 0:v:0")
                        .WithCustomArgument("-f flv")
                        .WithCustomArgument("-max_delay 0"))
                    .ProcessAsynchronously();
            }
        }

        public async Task IniciarStreamPCComAudio()
        {
            _cts = new CancellationTokenSource();

            // Nome do dispositivo de áudio exatamente como listado no dshow
            var nomeAudio = ObterDispositivosComId("Stereo Mix");

            string baseDir = $"{AppDomain.CurrentDomain.BaseDirectory}ffmpeg";

            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = baseDir });

            bool isRunnning = Process.GetProcessesByName("ffmpeg").Any();

            if (!isRunnning)
            {
                await FFMpegArguments
                .FromFileInput("desktop", false, options => options
                    .WithCustomArgument("-f gdigrab")
                    .WithCustomArgument("-framerate 20")
                    .WithCustomArgument("-offset_x 0")
                    .WithCustomArgument("-offset_y 0")
                    .WithCustomArgument("-video_size 1920x1080")
                    .WithCustomArgument("-thread_queue_size 4096"))
                .AddDeviceInput($"audio={nomeAudio.Id}", options => options
                    .WithCustomArgument("-f dshow")
                    .WithCustomArgument("-thread_queue_size 4096"))
                .OutputToFile("rtmp://localhost/live/stream", false, options => options
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithCustomArgument("-pix_fmt yuv420p")
                    // Aplicando o filtro de áudio para corrigir o delay
                    .WithCustomArgument("-af \"adelay=500|500,aresample=async=1\"")
                    .WithCustomArgument("-map 0:v:0")
                    .WithCustomArgument("-map 1:a:0")
                    .WithCustomArgument("-f flv")
                    .WithCustomArgument("-max_delay 0"))
                    .ProcessAsynchronously(false);
            }
        }

        public async Task IniciarStreamCamera()
        {
            var nomeAudio = ObterDispositivosComId("Microphone (USB2.0 Device)");
            var nomeCamera = ObterDispositivosComId("Integrated Camera");

            string baseDir = $"{AppDomain.CurrentDomain.BaseDirectory}ffmpeg";

            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = baseDir });

            bool isRunnning = Process.GetProcessesByName("ffmpeg").Any();

            if (!isRunnning)
            {

                await FFMpegArguments
                .FromFileInput($"video={nomeCamera.Id}", false, options => options
                    .WithCustomArgument("-f dshow")
                    .WithCustomArgument("-rtbufsize 256M")
                    .WithCustomArgument("-thread_queue_size 512"))
                .AddDeviceInput($"audio={nomeAudio.Id}", options => options
                    .WithCustomArgument("-f dshow")
                    .WithCustomArgument("-rtbufsize 128M")
                    .WithCustomArgument("-thread_queue_size 512"))
                .OutputToFile("rtmp://localhost/live/stream", false, options => options
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithSpeedPreset(Speed.UltraFast)
                    .WithCustomArgument("-pix_fmt yuv420p")
                    .WithCustomArgument("-map 0:v:0") // Vídeo da Câmera
                    .WithCustomArgument("-map 1:a:0") // Áudio do Stereo Mix
                    .WithCustomArgument("-f flv")
                    .WithCustomArgument("-max_delay 0"))
                .ProcessAsynchronously();
            }
        }

        public async Task StreamDeMidia()
        {

            string baseDir = $"{AppDomain.CurrentDomain.BaseDirectory}ffmpeg";

            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = baseDir });

            bool isRunnning = Process.GetProcessesByName("ffmpeg").Any();

            if (!isRunnning)
            {
                string caminhoArquivo = @"d:\teste.mp4";

                await FFMpegArguments
                    .FromFileInput(caminhoArquivo, false, options => options
                        .WithCustomArgument("-re")) // Lê o arquivo em tempo real (essencial para live)
                    .OutputToFile("rtmp://localhost/live/stream", false, options => options
                        .WithVideoCodec(VideoCodec.LibX264)
                        .WithAudioCodec(AudioCodec.Aac)
                        .WithCustomArgument("-f flv"))
                    .ProcessAsynchronously();
            }

        }

        private async void btnCaptureDesktopWithoutAudio_Click(object sender, EventArgs e)
        {
            await IniciarCapturaDesktopSemAudio();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            FinalizarFfmpeg();
            FinalizarNginx();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await IniciarStreamPCComAudio();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await IniciarStreamCamera();
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            await StreamDeMidia();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FinalizarFfmpeg();
        }
    }

}
