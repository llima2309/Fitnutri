namespace AppFitNutri.Views;

public class VideoCallPage : ContentPage
{
    private readonly Guid _agendamentoId;
    private readonly string _callToken;
    private readonly string _hubUrl;
    private readonly string _jwtToken;
    private readonly string _userId;
    private readonly string _userType;

    public VideoCallPage(
        Guid agendamentoId,
        string callToken,
        string hubUrl,
        string jwtToken,
        string userId,
        string userType)
    {
        _agendamentoId = agendamentoId;
        _callToken = callToken;
        _hubUrl = hubUrl;
        _jwtToken = jwtToken;
        _userId = userId;
        _userType = userType;

        Title = "Videochamada";

        LoadWebView();
    }

    private void LoadWebView()
    {
        var apiUrl = GetApiBaseUrl();

        var html = GenerateVideoCallHtml(apiUrl);

        var webView = new WebView
        {
            Source = new HtmlWebViewSource { Html = html }
        };

        Content = new Grid
        {
            Children = { webView }
        };
    }

    private string GetApiBaseUrl()
    {
        // API está em produção
        return "https://api.fit-nutri.com";
    }

    private string GenerateVideoCallHtml(string apiUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>
    <title>Videochamada</title>
    <script src='https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.14/dist/browser/signalr.min.js'></script>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: #000;
            overflow: hidden;
        }}
        #videoContainer {{
            position: relative;
            width: 100vw;
            height: 100vh;
            background: #000;
        }}
        video {{
            width: 100%;
            height: 100%;
            object-fit: cover;
        }}
        #remoteVideo {{
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
        }}
        #localVideo {{
            position: absolute;
            top: 20px;
            right: 20px;
            width: 120px;
            height: 160px;
            border-radius: 12px;
            border: 2px solid #fff;
            box-shadow: 0 4px 12px rgba(0,0,0,0.5);
            object-fit: cover;
            z-index: 10;
        }}
        #controls {{
            position: absolute;
            bottom: 30px;
            left: 50%;
            transform: translateX(-50%);
            display: flex;
            gap: 15px;
            z-index: 20;
        }}
        .control-btn {{
            width: 56px;
            height: 56px;
            border-radius: 50%;
            border: none;
            background: rgba(255,255,255,0.2);
            backdrop-filter: blur(10px);
            color: white;
            font-size: 24px;
            cursor: pointer;
            transition: all 0.3s;
            display: flex;
            align-items: center;
            justify-content: center;
        }}
        .control-btn:active {{
            transform: scale(0.95);
        }}
        .control-btn.danger {{
            background: #e74c3c;
        }}
        .control-btn.muted {{
            background: rgba(231, 76, 60, 0.9);
        }}
        #status {{
            position: absolute;
            top: 20px;
            left: 20px;
            background: rgba(0,0,0,0.7);
            color: white;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 12px;
            z-index: 10;
        }}
        .status-dot {{
            display: inline-block;
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background: #2ecc71;
            margin-right: 6px;
            animation: pulse 2s infinite;
        }}
        @keyframes pulse {{
            0%, 100% {{ opacity: 1; }}
            50% {{ opacity: 0.5; }}
        }}
        #loading {{
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            color: white;
            text-align: center;
            z-index: 5;
        }}
        .spinner {{
            border: 4px solid rgba(255,255,255,0.3);
            border-top: 4px solid white;
            border-radius: 50%;
            width: 40px;
            height: 40px;
            animation: spin 1s linear infinite;
            margin: 0 auto 16px;
        }}
        @keyframes spin {{
            0% {{ transform: rotate(0deg); }}
            100% {{ transform: rotate(360deg); }}
        }}
    </style>
</head>
<body>
    <div id='videoContainer'>
        <div id='loading'>
            <div class='spinner'></div>
            <div>Conectando...</div>
        </div>
        <div id='status'>
            <span class='status-dot'></span>
            <span id='statusText'>Conectando</span>
        </div>
        <video id='remoteVideo' autoplay playsinline></video>
        <video id='localVideo' autoplay muted playsinline></video>
        <div id='controls'>
            <button class='control-btn' id='toggleAudio' onclick='toggleAudio()'>🎤</button>
            <button class='control-btn danger' onclick='endCall()'>📞</button>
            <button class='control-btn' id='toggleVideo' onclick='toggleVideo()'>📹</button>
        </div>
    </div>

    <script>
        const CONFIG = {{
            apiUrl: '{apiUrl}',
            agendamentoId: '{_agendamentoId}',
            callToken: '{_callToken}',
            hubUrl: '{_hubUrl}',
            jwtToken: '{_jwtToken}',
            userId: '{_userId}',
            userType: '{_userType}',
            iceServers: [
                {{ urls: 'stun:stun.l.google.com:19302' }},
                {{ urls: 'stun:stun1.l.google.com:19302' }}
            ]
        }};

        let hubConnection = null;
        let localStream = null;
        let peerConnections = new Map();
        let audioEnabled = true;
        let videoEnabled = true;

        async function initialize() {{
            try {{
                updateStatus('Obtendo mídia...');
                await getLocalStream();
                
                updateStatus('Conectando ao servidor...');
                await connectToHub();
                
                hideLoading();
            }} catch (error) {{
                console.error('Erro na inicialização:', error);
                alert('Erro ao iniciar videochamada: ' + error.message);
            }}
        }}

        async function getLocalStream() {{
            try {{
                // Verificar se a API está disponível
                if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {{
                    throw new Error('API de mídia não disponível neste navegador. Use um navegador compatível com WebRTC.');
                }}

                console.log('Solicitando acesso à câmera e microfone...');
                
                localStream = await navigator.mediaDevices.getUserMedia({{
                    video: {{ 
                        facingMode: 'user',
                        width: {{ ideal: 1280 }},
                        height: {{ ideal: 720 }}
                    }},
                    audio: {{
                        echoCancellation: true,
                        noiseSuppression: true
                    }}
                }});
                
                console.log('Mídia obtida com sucesso');
                document.getElementById('localVideo').srcObject = localStream;
            }} catch (error) {{
                console.error('Erro ao acessar mídia:', error);
                
                let errorMessage = 'Erro ao acessar câmera/microfone: ';
                
                if (error.name === 'NotAllowedError' || error.name === 'PermissionDeniedError') {{
                    errorMessage += 'Permissão negada. Por favor, habilite o acesso à câmera e microfone nas configurações do app.';
                }} else if (error.name === 'NotFoundError' || error.name === 'DevicesNotFoundError') {{
                    errorMessage += 'Nenhuma câmera ou microfone encontrado no dispositivo.';
                }} else if (error.name === 'NotReadableError' || error.name === 'TrackStartError') {{
                    errorMessage += 'Câmera ou microfone já está em uso por outro aplicativo.';
                }} else {{
                    errorMessage += error.message || 'Erro desconhecido';
                }}
                
                throw new Error(errorMessage);
            }}
        }}

        async function connectToHub() {{
            try {{
                const hubUrl = CONFIG.apiUrl + CONFIG.hubUrl;
                console.log('Conectando ao hub:', hubUrl);
                console.log('Token JWT:', CONFIG.jwtToken ? 'Presente' : 'Ausente');
                
                hubConnection = new signalR.HubConnectionBuilder()
                    .withUrl(hubUrl, {{
                        accessTokenFactory: () => CONFIG.jwtToken,
                        skipNegotiation: false,
                        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling
                    }})
                    .configureLogging(signalR.LogLevel.Debug)
                    .withAutomaticReconnect()
                    .build();

                setupHubHandlers();

                console.log('Iniciando conexão SignalR...');
                await hubConnection.start();
                console.log('SignalR conectado! Estado:', hubConnection.state);
                
                console.log('Entrando na sala...');
                await hubConnection.invoke('JoinCall', CONFIG.agendamentoId, CONFIG.userId, CONFIG.userType);
                console.log('Entrou na sala com sucesso');
                
                updateStatus('Conectado');
            }} catch (error) {{
                console.error('Erro ao conectar ao hub:', error);
                console.error('URL tentada:', CONFIG.apiUrl + CONFIG.hubUrl);
                console.error('Detalhes do erro:', error.message, error.stack);
                
                let errorMsg = 'Erro ao conectar ao servidor: ';
                if (error.message.includes('Failed to complete negotiation')) {{
                    errorMsg += 'Não foi possível conectar ao servidor. Verifique se a API está rodando e acessível.';
                }} else if (error.message.includes('Load Failed')) {{
                    errorMsg += 'Falha ao carregar conexão. Pode ser problema de rede ou certificado SSL.';
                }} else {{
                    errorMsg += error.message;
                }}
                
                throw new Error(errorMsg);
            }}
        }}

        function setupHubHandlers() {{
            hubConnection.on('UserJoined', async (userId, userType, connectionId) => {{
                console.log('Usuário entrou:', userId, 'connectionId:', connectionId);
                // Apenas o usuário que JÁ estava na sala deve criar oferta
                console.log('Meu connectionId:', hubConnection.connectionId);
                if (hubConnection.connectionId < connectionId) {{
                    console.log('Criando peer connection como iniciador');
                    await createPeerConnection(connectionId, true);
                }} else {{
                    console.log('Aguardando oferta do outro peer');
                }}
            }});

            hubConnection.on('ExistingParticipants', async (participants) => {{
                console.log('Participantes existentes:', participants.length);
                // Não criar ofertas aqui - aguardar o UserJoined no outro lado
                console.log('Lista de participantes:', participants);
            }});

            hubConnection.on('ReceiveOffer', async (offer, fromConnectionId) => {{
                console.log('Recebeu oferta de:', fromConnectionId);
                await handleOffer(offer, fromConnectionId);
            }});

            hubConnection.on('ReceiveAnswer', async (answer, fromConnectionId) => {{
                console.log('Recebeu resposta de:', fromConnectionId);
                await handleAnswer(answer, fromConnectionId);
            }});

            hubConnection.on('ReceiveIceCandidate', async (candidate, fromConnectionId) => {{
                console.log('Recebeu ICE candidate');
                await handleIceCandidate(candidate, fromConnectionId);
            }});

            hubConnection.on('UserLeft', (connectionId) => {{
                console.log('Usuário saiu:', connectionId);
                closePeerConnection(connectionId);
            }});

            hubConnection.onclose(() => {{
                console.log('Hub desconectado');
                updateStatus('Desconectado');
            }});
            
            hubConnection.onreconnecting(() => {{
                console.log('Reconectando...');
                updateStatus('Reconectando...');
            }});
            
            hubConnection.onreconnected(() => {{
                console.log('Reconectado!');
                updateStatus('Conectado');
            }});
        }}

        async function createPeerConnection(connectionId, isInitiator) {{
            console.log('Criando peer connection para:', connectionId, 'isInitiator:', isInitiator);
            
            const pc = new RTCPeerConnection({{ iceServers: CONFIG.iceServers }});
            peerConnections.set(connectionId, pc);

            // Adicionar tracks locais
            localStream.getTracks().forEach(track => {{
                console.log('Adicionando track:', track.kind);
                pc.addTrack(track, localStream);
            }});

            // Receber tracks remotos
            pc.ontrack = (event) => {{
                console.log('Track remoto recebido:', event.track.kind);
                const remoteVideo = document.getElementById('remoteVideo');
                if (remoteVideo.srcObject !== event.streams[0]) {{
                    console.log('Conectando stream remoto ao vídeo');
                    remoteVideo.srcObject = event.streams[0];
                }}
            }};

            // ICE candidates
            pc.onicecandidate = async (event) => {{
                if (event.candidate) {{
                    console.log('Enviando ICE candidate');
                    await hubConnection.invoke('SendIceCandidate', 
                        CONFIG.agendamentoId, 
                        JSON.stringify(event.candidate), 
                        connectionId
                    );
                }}
            }};
            
            // Connection state changes
            pc.onconnectionstatechange = () => {{
                console.log('Connection state:', pc.connectionState);
                if (pc.connectionState === 'connected') {{
                    console.log('✅ Peer conectado com sucesso!');
                }}
            }};
            
            pc.oniceconnectionstatechange = () => {{
                console.log('ICE connection state:', pc.iceConnectionState);
            }};

            // Se for o iniciador, criar e enviar oferta
            if (isInitiator) {{
                console.log('Criando oferta...');
                const offer = await pc.createOffer({{
                    offerToReceiveAudio: true,
                    offerToReceiveVideo: true
                }});
                console.log('Oferta criada, definindo local description');
                await pc.setLocalDescription(offer);
                console.log('Enviando oferta para:', connectionId);
                await hubConnection.invoke('SendOffer', 
                    CONFIG.agendamentoId, 
                    JSON.stringify(offer), 
                    connectionId
                );
                console.log('Oferta enviada!');
            }}
        }}

        async function handleOffer(offerJson, fromConnectionId) {{
            console.log('Processando oferta de:', fromConnectionId);
            const offer = JSON.parse(offerJson);
            let pc = peerConnections.get(fromConnectionId);

            if (!pc) {{
                console.log('Criando nova peer connection para processar oferta');
                await createPeerConnection(fromConnectionId, false);
                pc = peerConnections.get(fromConnectionId);
            }}

            console.log('Estado atual do PC:', pc.signalingState);
            
            if (pc.signalingState !== 'stable') {{
                console.warn('PC não está em estado stable:', pc.signalingState);
            }}

            console.log('Definindo remote description (oferta)');
            await pc.setRemoteDescription(new RTCSessionDescription(offer));
            
            console.log('Criando resposta...');
            const answer = await pc.createAnswer();
            
            console.log('Definindo local description (resposta)');
            await pc.setLocalDescription(answer);

            console.log('Enviando resposta para:', fromConnectionId);
            await hubConnection.invoke('SendAnswer', 
                CONFIG.agendamentoId, 
                JSON.stringify(answer), 
                fromConnectionId
            );
            console.log('Resposta enviada!');
        }}

        async function handleAnswer(answerJson, fromConnectionId) {{
            console.log('Processando resposta de:', fromConnectionId);
            const answer = JSON.parse(answerJson);
            const pc = peerConnections.get(fromConnectionId);
            
            if (!pc) {{
                console.error('Peer connection não encontrado para:', fromConnectionId);
                return;
            }}
            
            console.log('Estado atual do PC:', pc.signalingState);
            
            if (pc.signalingState === 'stable') {{
                console.warn('PC já está em stable, ignorando resposta duplicada');
                return;
            }}
            
            if (pc.signalingState !== 'have-local-offer') {{
                console.error('Estado inesperado:', pc.signalingState, 'esperado: have-local-offer');
                return;
            }}
            
            console.log('Definindo remote description (resposta)');
            await pc.setRemoteDescription(new RTCSessionDescription(answer));
            console.log('Remote description definido! Estado:', pc.signalingState);
        }}

        async function handleIceCandidate(candidateJson, fromConnectionId) {{
            console.log('Processando ICE candidate de:', fromConnectionId);
            const candidate = JSON.parse(candidateJson);
            const pc = peerConnections.get(fromConnectionId);
            
            if (!pc) {{
                console.warn('Peer connection não encontrado, ignorando candidate');
                return;
            }}
            
            try {{
                if (pc.remoteDescription) {{
                    console.log('Adicionando ICE candidate');
                    await pc.addIceCandidate(new RTCIceCandidate(candidate));
                }} else {{
                    console.warn('Remote description ainda não definido, salvando candidate para depois');
                    // Em produção, você pode querer guardar esses candidates
                }}
            }} catch (error) {{
                console.error('Erro ao adicionar ICE candidate:', error);
            }}
        }}

        function closePeerConnection(connectionId) {{
            const pc = peerConnections.get(connectionId);
            if (pc) {{
                pc.close();
                peerConnections.delete(connectionId);
            }}
        }}

        async function toggleAudio() {{
            audioEnabled = !audioEnabled;
            localStream.getAudioTracks().forEach(track => {{
                track.enabled = audioEnabled;
            }});
            document.getElementById('toggleAudio').classList.toggle('muted', !audioEnabled);
            await hubConnection.invoke('ToggleAudio', CONFIG.agendamentoId, audioEnabled);
        }}

        async function toggleVideo() {{
            videoEnabled = !videoEnabled;
            localStream.getVideoTracks().forEach(track => {{
                track.enabled = videoEnabled;
            }});
            document.getElementById('toggleVideo').classList.toggle('muted', !videoEnabled);
            await hubConnection.invoke('ToggleVideo', CONFIG.agendamentoId, videoEnabled);
        }}

        async function endCall() {{
            if (confirm('Deseja encerrar a chamada?')) {{
                try {{
                    if (hubConnection) {{
                        await hubConnection.invoke('LeaveCall', CONFIG.agendamentoId);
                        await hubConnection.stop();
                    }}
                    
                    if (localStream) {{
                        localStream.getTracks().forEach(track => track.stop());
                    }}
                    
                    peerConnections.forEach((pc) => pc.close());
                    
                    // Notifica o MAUI para fechar a página
                    if (window.webkit?.messageHandlers?.closeVideoCall) {{
                        window.webkit.messageHandlers.closeVideoCall.postMessage('close');
                    }} else if (window.chrome?.webview) {{
                        window.chrome.webview.postMessage('close');
                    }} else {{
                        window.location.href = 'app://close';
                    }}
                }} catch (error) {{
                    console.error('Erro ao encerrar chamada:', error);
                }}
            }}
        }}

        function updateStatus(text) {{
            document.getElementById('statusText').textContent = text;
        }}

        function hideLoading() {{
            document.getElementById('loading').style.display = 'none';
        }}

        // Inicializa quando a página carregar
        window.addEventListener('load', initialize);
    </script>
</body>
</html>
";
    }
}