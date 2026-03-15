// BridgeServer.cs — TCP server for RL agent communication.
//
// Protocol: newline-delimited JSON over TCP (one JSON object per line).
//   Game → Agent:  state messages (sent when game is idle / awaiting input)
//   Agent → Game:  action messages (PLAY, END_TURN, CHOOSE, POTION)
//
// Threading model:
//   - TCP accept/read loop runs on a background thread (Task.Run)
//   - Game state serialization happens on the main thread via StabilityDetector
//   - Action injection is dispatched to the main thread via Godot's CallDeferred
//
// The server accepts exactly one client at a time. If the client disconnects,
// it goes back to listening for a new connection.

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace STS2BridgeMod;

public class BridgeServer
{
    // Singleton instance — created once, lives for the lifetime of the game process.
    public static readonly BridgeServer Instance = new();

    private TcpListener? _listener;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private readonly object _lock = new();
    private bool _running;
    private CancellationTokenSource? _cts;

    // Buffer for reading newline-delimited messages from the client.
    private readonly byte[] _readBuffer = new byte[8192];
    private string _readRemainder = "";

    /// <summary>
    /// Whether a Python client is currently connected.
    /// </summary>
    public bool IsClientConnected
    {
        get
        {
            lock (_lock)
            {
                return _client?.Connected == true;
            }
        }
    }

    private BridgeServer() { }

    /// <summary>
    /// Start listening for client connections on the given port.
    /// Called once from BridgeMod.Initialize().
    /// </summary>
    public void Start(int port)
    {
        if (_running) return;
        _running = true;
        _cts = new CancellationTokenSource();

        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.Start();
        Logger.Log($"[BridgeServer] Listening on 127.0.0.1:{port}");

        // Run the accept loop on a background thread so we don't block
        // the game's main thread.
        Task.Run(() => AcceptLoopAsync(_cts.Token));
    }

    /// <summary>
    /// Stop the server and disconnect any client.
    /// </summary>
    public void Stop()
    {
        _running = false;
        _cts?.Cancel();
        DisconnectClient();
        _listener?.Stop();
        Logger.Log("[BridgeServer] Server stopped.");
    }

    /// <summary>
    /// Send a game state JSON to the connected client.
    /// Called from the main thread (via StabilityDetector) when the game
    /// is idle and waiting for player input.
    /// </summary>
    public void SendState(string stateJson)
    {
        lock (_lock)
        {
            if (_stream == null || _client?.Connected != true)
                return;

            try
            {
                // Protocol: each message is a single JSON line terminated by \n
                byte[] data = Encoding.UTF8.GetBytes(stateJson + "\n");
                _stream.Write(data, 0, data.Length);
                _stream.Flush();
            }
            catch (Exception ex)
            {
                Logger.Log($"[BridgeServer] Error sending state: {ex.Message}");
                DisconnectClient();
            }
        }
    }

    // ----------------------------------------------------------------
    // Background thread methods
    // ----------------------------------------------------------------

    /// <summary>
    /// Accepts client connections in a loop. Only one client is served
    /// at a time. When a client disconnects, we go back to accepting.
    /// </summary>
    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (_running && !ct.IsCancellationRequested)
        {
            try
            {
                Logger.Log("[BridgeServer] Waiting for client connection...");
                var client = await _listener!.AcceptTcpClientAsync(ct);
                Logger.Log($"[BridgeServer] Client connected from {client.Client.RemoteEndPoint}");

                lock (_lock)
                {
                    _client = client;
                    _stream = client.GetStream();
                    _readRemainder = "";
                }

                // Handle this client until it disconnects
                await HandleClientAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger.Log($"[BridgeServer] Accept error: {ex.Message}");
                await Task.Delay(1000, ct);
            }
        }
    }

    /// <summary>
    /// Read loop for the connected client. Reads newline-delimited JSON
    /// action messages and dispatches them to ActionInjector on the
    /// main thread.
    /// </summary>
    private async Task HandleClientAsync(CancellationToken ct)
    {
        try
        {
            while (_running && !ct.IsCancellationRequested)
            {
                NetworkStream? stream;
                lock (_lock)
                {
                    stream = _stream;
                }
                if (stream == null) break;

                int bytesRead = await stream.ReadAsync(_readBuffer, 0, _readBuffer.Length, ct);
                if (bytesRead == 0)
                {
                    Logger.Log("[BridgeServer] Client disconnected (read 0 bytes).");
                    break;
                }

                _readRemainder += Encoding.UTF8.GetString(_readBuffer, 0, bytesRead);

                // Process all complete lines (newline-delimited JSON messages)
                while (_readRemainder.Contains('\n'))
                {
                    int idx = _readRemainder.IndexOf('\n');
                    string line = _readRemainder[..idx].Trim();
                    _readRemainder = _readRemainder[(idx + 1)..];

                    if (string.IsNullOrEmpty(line))
                        continue;

                    ProcessActionMessage(line);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Logger.Log($"[BridgeServer] Client read error: {ex.Message}");
        }
        finally
        {
            DisconnectClient();
        }
    }

    /// <summary>
    /// Parse an action JSON message and dispatch it to ActionInjector
    /// on the Godot main thread.
    /// </summary>
    private void ProcessActionMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string actionType = root.GetProperty("type").GetString() ?? "";

            // All game actions must execute on the Godot main thread.
            // We use CallDeferred to schedule execution on the next
            // main thread frame.
            switch (actionType.ToUpperInvariant())
            {
                case "PLAY":
                    int cardIndex = root.GetProperty("card_index").GetInt32();
                    int targetIndex = root.TryGetProperty("target_index", out var ti) ? ti.GetInt32() : -1;
                    Godot.Callable.From(() => ActionInjector.InjectPlayCard(cardIndex, targetIndex)).CallDeferred();
                    break;

                case "END_TURN":
                    Godot.Callable.From(() => ActionInjector.InjectEndTurn()).CallDeferred();
                    break;

                case "CHOOSE":
                    int choiceIndex = root.GetProperty("choice_index").GetInt32();
                    Godot.Callable.From(() => ActionInjector.InjectChoice(choiceIndex)).CallDeferred();
                    break;

                case "POTION":
                    int potionSlot = root.GetProperty("slot").GetInt32();
                    int potionTarget = root.TryGetProperty("target_index", out var pt) ? pt.GetInt32() : -1;
                    Godot.Callable.From(() => ActionInjector.InjectPotionUse(potionSlot, potionTarget)).CallDeferred();
                    break;

                case "PING":
                    // Health check — respond with PONG immediately
                    SendState("{\"type\":\"pong\"}");
                    break;

                default:
                    Logger.Log($"[BridgeServer] Unknown action type: {actionType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[BridgeServer] Error parsing action JSON: {ex.Message}");
            Logger.Log($"[BridgeServer] Raw message: {json}");
        }
    }

    private void DisconnectClient()
    {
        lock (_lock)
        {
            _stream?.Close();
            _stream = null;
            _client?.Close();
            _client = null;
        }
    }
}
