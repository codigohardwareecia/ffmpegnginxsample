#####Stream com FFmpeg e NGINX
O **FFmpeg** é o "canivete suíço" do mundo multimídia. É uma estrutura de código aberto (open-source) que contém uma coleção gigantesca de bibliotecas e ferramentas para processar praticamente qualquer coisa relacionada a áudio, vídeo e imagem.

O **Nginx** é um servidor de alto desempenho que funciona como um "diretor de tráfego" na internet. Ele é extremamente leve e rápido, sendo usado principalmente para três funções:

1. **Servidor Web:** Entrega arquivos (HTML, imagens, vídeos) para os usuários.
    
2. **Proxy Reverso:** Recebe as conexões da internet e as repassa para outros aplicativos (como o seu em C# ou um banco de dados).
    
3. **Servidor de Mídia:** No seu caso, ele recebe o vídeo do FFmpeg via **RTMP** e o converte para formatos que rodam no navegador (**HLS**).
### Pré-requisitos - NGINX

Baixe a versão Gryphon do Ninject foi com essa que fiz meus testes
http://nginx-win.ecsds.eu/download/
http://nginx-win.ecsds.eu/download/nginx%201.7.11.3%20Gryphon.zip

Descompacte o arquivo em um diretorio e renomeie para Nginx para facilitar o acesso

Dentro da pasta do Nginx no diretório acesse o arquivo conf\nginx.conf com o editor de texto, caso não exista crie o arquivo nginx.conf e cole o código abaixo, salve o arquivo:

```
worker_processes  1;

events {
    worker_connections  1024;
}

rtmp {
    server {
        listen 1935; # Porta para o seu C# enviar o vídeo
        chunk_size 1024;

        application live {
            live on;
            record off; # Não salva em disco para não encher o HD

            # Configuração para assistir no Navegador (HTML5)
            hls on;
            hls_path temp/hls;
            hls_fragment 6;
            hls_playlist_length 120;
        }
    }
}


# Se for usar HTML5, precisa desse servidor HTTP para ler o .m3u8
http {
    server {
        listen 8080;
        location /hls {
            types {
                application/vnd.apple.mpegurl m3u8;
            }
            root temp;
            add_header Cache-Control no-cache;
            add_header Access-Control-Allow-Origin *; # Permite rodar no seu site
        }
    }
}
```

O Nginx precisa do Visual C++ Runtime para funcionar, ao executar pode ser que apareça a mensagem de erro da biblioteca **MSVCR100.dl**, então vamos já instalar o "# Microsoft Visual C++ 2010 Service Pack 1 Redistributable Package" pelo link https://www.microsoft.com/en-us/download/details.aspx?id=26999, faça o download da versão x86 pois o Nginx que baixamos funciona só em 32bits inclusive o Fffmpeg.
### Pré-requisitos - FFMPEG

- Acesse a url https://ffmpeg.org/download.html
- Clique na janelinha do Windows, logo abaixo será exibido "Windows EXE Files"
- Clique em  "Windows builds from gyan.dev" ou pelo link https://www.gyan.dev/ffmpeg/builds/
- Na página que vai se abrir procure "git master builds", localiza a seçõ "latest git master branch build" e logo abaixo clique no link "ffmpeg git essencials.7z" ou pelo link https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-essentials.7z.sha256, aguarde o download.
- Descompacte o arquivo numa pasta de facil acesso

### Pré-requisitos VB-Audio Virtual Cable

Acesse o endereço https://vb-audio.com/Cable/ e clique no botão de download ao lado
Descompacte a pasta e execute o arquivo VBCABLE_Setup_x64.exe e depois "Install Drive"
Para ver se foi instalado execute o comando a seguir na pasta onde vc baixou o Ffmpeg

```MSDOS
ffmpeg -list_devices true -f dshow -i dummy
```

Se aparecer "CABLE Output (VB-Audio Virtual Cable)" foi  instalado com sucesso.
### Inciando o Nginx

O Nginx vai ser fundamental para visualizarmos a transmissão, acesse a pasta que vc baixou o Nginx e execute o arquivo nginx.exe, isso iré iniciar o servidor do Nginx

### Primeira parte - Testando a transmissão de imagem do Ffmpeg

Acesse a pasta onde vc baixou o ffmpeg e execute o seguinte comando no prompt de comandos:

```
// para capturar todos os monitores
ffmpeg -f gdigrab -framerate 20 -thread_queue_size 4096 -i desktop -c:v libx264 -preset ultrafast -pix_fmt yuv420p -map 0:v:0 -f flv -max_delay 0 "rtmp://localhost/live/stream"

// para capturar apenas um monitor
ffmpeg -f gdigrab -framerate 20 -offset_x 0 -offset_y 0 -video_size 1920x1080 -thread_queue_size 4096 -i desktop -c:v libx264 -preset ultrafast -pix_fmt yuv420p -map 0:v:0 -f flv -max_delay 0 "rtmp://localhost/live/stream"
```

### Como visualizar

Há várias formas de visualizar :

#### VLC
Abra o VLC e abra Media > Open Network Stream, cole a URL http://localhost:8080/hls/stream.m3u8 e clique em Play


#### FFPLAY
A biblioteca do FFmpeg tem um player, acesse a pasta que vc descompactou e execute o comando

```
ffplay -i http://localhost:8080/hls/stream.m3u8
```
### PÁGINA HTML

Vamos construir nossa página html com nosso player, copie o código a seguir:

```Html
<!DOCTYPE html>
<html>
<body>
  <video id="video" controls style="width: 100%;"></video>

  <script src="https://cdn.jsdelivr.net/npm/hls.js@latest"></script>
  <script>
   var video = document.getElementById('video');

   if (video.requestFullscreen) {
        video.requestFullscreen();
    } else if (video.webkitRequestFullscreen) { /* Safari / iOS */
        video.webkitRequestFullscreen();
    } else if (video.msRequestFullscreen) { /* IE11 / Edge antigo */
        video.msRequestFullscreen();
    }
    
  var videoSrc = '/hls/stream.m3u8'; 

  if (Hls.isSupported()) {
    // Para Android/Chrome
    var hls = new Hls();
    hls.loadSource(videoSrc);
    hls.attachMedia(video);
    hls.on(Hls.Events.MANIFEST_PARSED, function() {
      video.play();
    });
  } 
  else if (video.canPlayType('application/vnd.apple.mpegurl')) {
    // Para iOS/Safari Nativo
    video.src = videoSrc;
    video.addEventListener('loadedmetadata', function() {
      video.play();
    });
  }
  </script>
</body>
</html>
```

Agora vá dentro da pasta do Nginx e ache a pasta html, abra o arquivo index.html e apague o conteúdo, cole o conteúdo copiado acima e salve.

Abra a url http://localhost:8080

Também funciona no celular

#### Reproduzindo Tela do desktop com Som

Temos algumas dificuldades para reproduzir audio, por isso instalamos o Cable Audio, em algumas placas mäes o driver e instalado pelo Windows, e o mesmo não instala o Stereo Mix.

Se de alguma maneira nao tiver instalado use o Cable Audio, nunca esquecendo de selecionar ele como Gravacao nas opções de som do Windows

Após a instalação do driver do fabricante da sua placa de som, vá no alto-falante com o botão direito e selecione Sons, localizae Stereo Mix, ele vai estar desabilitado, então habilite-o.

Explique a questão da sincronica.

Execute o seguinte script:
```
// Comando FFMEPG para utilizar o Audio do CABLE OUTPUT
ffmpeg -f gdigrab -framerate 20 -offset_x 0 -offset_y 0 -video_size 1920x1080 -thread_queue_size 4096 -i desktop -f dshow -thread_queue_size 4096 -i audio="CABLE Output (VB-Audio Virtual Cable)" -c:v libx264 -c:a aac -pix_fmt yuv420p -af "adelay=1050|1050,aresample=async=1" -map 0:v:0 -map 1:a:0 -f flv -max_delay 0 "rtmp://localhost/live/stream"

// Comando FFMPEG para usar o Stereo Mix (Realtek(R) Audio)
ffmpeg -f gdigrab -framerate 20 -offset_x 0 -offset_y 0 -video_size 1920x1080 -thread_queue_size 4096 -i desktop -f dshow -thread_queue_size 4096 -i audio="Stereo Mix (Realtek(R) Audio)" -c:v libx264 -c:a aac -pix_fmt yuv420p -af "adelay=0500|0500,aresample=async=1" -map 0:v:0 -map 1:a:0 -f flv -max_delay 0 "rtmp://localhost/live/stream"
```

#### Reproduzindo da Camera do PC

Abaixo o comando para reproduzir a partir da camera do PC
```
// Com uma entrada de audio
ffmpeg -f dshow -rtbufsize 256M -thread_queue_size 512 -i video="Integrated Camera" -f dshow -rtbufsize 128M -thread_queue_size 512 -i audio="Microphone (USB2.0 Device)" -c:v libx264 -c:a aac -preset ultrafast -pix_fmt yuv420p        -map 0:v:0 -map 1:a:0 -f flv -max_delay 0 "rtmp://localhost/live/stream"

// Com duas entradas de audio
ffmpeg -f dshow -rtbufsize 256M -thread_queue_size 512 -i video="Integrated Camera" -f dshow -rtbufsize 128M -thread_queue_size 512 -i audio="Stereo Mix (Realtek(R) Audio)" -f dshow -rtbufsize 128M -thread_queue_size 512 -i audio="Microphone (USB2.0 Device)" -filter_complex "[1:a][2:a]amix=inputs=2:duration=first[mixed]; [mixed]adelay=500|500[aout]" -c:v libx264 -c:a aac -preset ultrafast -pix_fmt yuv420p -map 0:v:0 -map "[aout]" -f flv -max_delay 0 "rtmp://localhost/live/stream"
```

#### Enviando arquivo

```
// Enviando somente um arquivo
ffmpeg -re -i "d:\teste.mp4" -c:v libx264 -c:a aac -f flv "rtmp://localhost/live/stream"

// Enviando de uma lista de arquivos
ffmpeg -re -f concat -safe 0 -i "D:\lista.txt" -c:v libx264 -c:a aac -preset veryfast -f flv "rtmp://localhost/live/stream"
```

#### Utilizando o FFMPEGCore no Visual Studio C#

#### FFmpegCore
Será necessário instalar o FFmpegCore, vá em Tools > Nuget Package Manager> Manage Nuget for solutions e digite FFMpegCore, selecione o primeiro item da lista versão 5.4, e  clique em Install e depois em Apply

#### Imports
Faça os seguintes imports
```
using FFMpegCore;
using FFMpegCore.Enums;
```

##### Nginx

Vamos precisar copiar a pasta do Nginx que usamos até agorapara o nosso código.

Vamos copiar a pasta do ffmpeg que baixamos.

Vamos editar nosso arquivo de projeto, ele tem que ficar parecido como este

```
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

	<ItemGroup>
		<None Include="nginx\**\*.*">
			<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
		</None>
		<None Include="ffmpeg\**\*.*">
			<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
		</None>
	</ItemGroup>
	<ItemGroup>
	  <PackageReference Include="FFMpegCore" Version="5.4.0" />
	</ItemGroup>

	<ItemGroup>
	  <Folder Include="nginx\temp\client_body_temp\" />
	  <Folder Include="nginx\temp\fastcgi_temp\" />
	  <Folder Include="nginx\temp\hls\" />
	  <Folder Include="nginx\temp\proxy_temp\" />
	  <Folder Include="nginx\temp\recordings\" />
	  <Folder Include="nginx\temp\scgi_temp\" />
	  <Folder Include="nginx\temp\uwsgi_temp\" />
	</ItemGroup>
	<Target Name="MakeNginxTempFolder" AfterTargets="Build">
		<MakeDir Directories="$(OutputPath)nginx\temp\hls" />
		<MakeDir Directories="$(OutputPath)nginx\temp\recordings" />
		<MakeDir Directories="$(OutputPath)nginx\logs" />
	</Target>
</Project>
```

Adicionar o método finalizador de processo do Nginx e do FFmpeg

```CSharp

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
```

Adicionar no método de FormClosing as chamadas a estes dois métodos

```CSharp

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            FinalizarFfmpeg();
            FinalizarNginx();
        }
```

Agora vamos adicionar o método para iniciar o Ngixn

```CSharp
public void StartNginx()
{
    string caminhoNginx = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nginx", "nginx.exe");

    // 1. Limpa instâncias travadas
    FinalizarNginx();

    // 2. Configura e inicia
    ProcessStartInfo startInfo = new ProcessStartInfo
    {
        FileName = caminhoNginx,
        WorkingDirectory = Path.GetDirectoryName(caminhoNginx),
        CreateNoWindow = true,
        UseShellExecute = false
    };

    Process.Start(startInfo);
}
```

Vamos criar o método para fechar e inicializar o Nginx

```CSharp
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
```

#### Listar dispositivos de Video ou Áudio
Crie uma classe com o nome DispositivoFFmpeg e cole o código abaixo:

```CSharp
    public class DispositivoFFmpeg
    {
        public string Nome { get; set; }
        public string Id { get; set; }
    }
```

Cole o método abaixo para trazer o tipo de dispositivo necessário para nossa implementação:

```Csharp

        public DispositivoFFmpeg ObterDispositivosComId(string search)
        {
            var dispositivos = new List<DispositivoFFmpeg>();
		    string baseDir = $"{AppDomain.CurrentDomain.BaseDirectory}ffmpeg";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = $"{baseDir}\\ffmpeg.exe",
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
```

#### Implementação do FFMPEGCore

Adicione um botão ao formulário, coloque  o titulo "Capturar PC sem som" e cole o seguinte código:

```CSharp
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
```

Adicione outro botão e coloque o titulo de Captura da tela com áudio do PC

```CSharp
 public async Task IniciarStreamPCComAudio()
 {
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
         .ProcessAsynchronously();
     }
 }
```

Adicione outro botão vamos enviar os dados da camera com áudio

```CSharp
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
```

Agora adicione o botão para transmitirmos um arquivo mp4

```CSharp
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
```

Se precisar gravar o video altere o Nginx.conf para

```
worker_processes  1;

events {
    worker_connections  1024;
}

rtmp {
    server {
        listen 1935; # Porta para o seu C# enviar o vídeo
        chunk_size 1024;

        application live {
            live on;
            record all;                         # Grava tudo (áudio e vídeo)
            record_path temp/recordings;        # Pasta onde os vídeos serão salvos
            record_unique on;                   # Adiciona um sufixo de tempo (evita sobrescrever)

            # Configuração para assistir no Navegador (HTML5)
            hls on;
            hls_path temp/hls;
            hls_fragment 6;
            hls_playlist_length 120;
        }
    }
}


# Se for usar HTML5, precisa desse servidor HTTP para ler o .m3u8
http {
    server {
        listen 8080;
        location /hls {
            types {
                application/vnd.apple.mpegurl m3u8;
            }
            root temp;
            add_header Cache-Control no-cache;
            add_header Access-Control-Allow-Origin *; # Permite rodar no seu site
        }
    }
}
```
