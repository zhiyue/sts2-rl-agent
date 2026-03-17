// BridgeServer.cs -- TCP server for RL agent communication.
//
// Protocol: newline-delimited JSON over TCP (one JSON object per line).
//   Game -> Agent:  state messages (combat_action, map_select, card_reward, etc.)
//   Agent -> Game:  action messages (play, end_turn, choose, skip)
//
// Threading model:
//   - TCP accept/read loop runs on a background thread (Task.Run)
//   - Game handlers call SendState() and WaitForActionAsync() from the game thread
//   - WaitForActionAsync() blocks the calling async context until a response arrives
//
// The server accepts exactly one client at a time. If the client disconnects,
// it goes back to listening for a new connection.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace STS2BridgeMod;

public class BridgeServer
{
    public static readonly BridgeServer Instance = new();

    private TcpListener? _listener;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private readonly object _lock = new();
    private bool _running;
    private CancellationTokenSource? _cts;

    private readonly byte[] _readBuffer = new byte[8192];
    private string _readRemainder = "";

    // Pending action response mechanism: when a handler sends state and waits
    // for a response, it sets _pendingAction. The read loop sets the result
    // when a complete line arrives.
    private TaskCompletionSource<string>? _pendingAction;
    private string? _pendingRequestId;
    private readonly object _pendingLock = new();
    private long _requestCounter;

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
    /// </summary>
    public void Start(int port)
    {
        if (_running) return;
        _running = true;
        _cts = new CancellationTokenSource();

        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.Start();
        Logger.Log($"[BridgeServer] Listening on 127.0.0.1:{port}");

        Task.Run(() => AcceptLoopAsync(_cts.Token));
    }

    /// <summary>
    /// Stop the server and disconnect any client.
    /// </summary>
    public void Stop()
    {
        _running = false;
        _cts?.Cancel();
        CancelPendingAction("Server stopped");
        DisconnectClient();
        _listener?.Stop();
        Logger.Log("[BridgeServer] Server stopped.");
    }

    /// <summary>
    /// Send a state JSON message to the connected client.
    /// Thread-safe; can be called from any thread.
    /// </summary>
    public void SendState(string stateJson)
    {
        SendStateInternal(stateJson);
    }

    private bool SendStateInternal(string stateJson)
    {
        lock (_lock)
        {
            if (_stream == null || _client?.Connected != true)
                return false;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(stateJson + "\n");
                _stream.Write(data, 0, data.Length);
                _stream.Flush();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"[BridgeServer] Error sending state: {ex.Message}");
                DisconnectClient();
                return false;
            }
        }
    }

    public async Task<string?> SendStateAndWaitForActionAsync(
        string stateJson, TimeSpan timeout, CancellationToken ct)
    {
        string requestId = Interlocked.Increment(ref _requestCounter).ToString();
        TaskCompletionSource<string> tcs;
        lock (_pendingLock)
        {
            _pendingAction?.TrySetCanceled();
            tcs = new TaskCompletionSource<string>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingAction = tcs;
            _pendingRequestId = requestId;
        }

        try
        {
            string payload = AttachRequestId(stateJson, requestId);
            if (!SendStateInternal(payload))
            {
                return null;
            }
            return await WaitForPendingActionAsync(tcs, timeout, ct);
        }
        finally
        {
            lock (_pendingLock)
            {
                if (_pendingAction == tcs)
                {
                    _pendingAction = null;
                    _pendingRequestId = null;
                }
            }
        }
    }

    /// <summary>
    /// Wait for the next action message from the Python client.
    /// This is the primary mechanism for handlers to receive agent decisions.
    ///
    /// Returns the raw JSON string of the action message, or null on timeout.
    /// </summary>
    public async Task<string?> WaitForActionAsync(
        TimeSpan timeout, CancellationToken ct)
    {
        TaskCompletionSource<string> tcs = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            lock (_pendingLock)
            {
                _pendingAction?.TrySetCanceled();
                _pendingAction = tcs;
                _pendingRequestId = null;
            }
            return await WaitForPendingActionAsync(tcs, timeout, ct);
        }
        finally
        {
            lock (_pendingLock)
            {
                if (_pendingAction == tcs)
                {
                    _pendingAction = null;
                    _pendingRequestId = null;
                }
            }
        }
    }

    private static async Task<string?> WaitForPendingActionAsync(
        TaskCompletionSource<string> tcs, TimeSpan timeout, CancellationToken ct)
    {
        try
        {
            using var timeoutCts = new CancellationTokenSource(timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                ct, timeoutCts.Token);

            using var reg = linkedCts.Token.Register(() =>
            {
                tcs.TrySetCanceled();
            });

            return await tcs.Task;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            Logger.Log($"[BridgeServer] WaitForAction error: {ex.Message}");
            return null;
        }
    }

    // ----------------------------------------------------------------
    // Background thread methods
    // ----------------------------------------------------------------

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (_running && !ct.IsCancellationRequested)
        {
            try
            {
                Logger.Log("[BridgeServer] Waiting for client connection...");
                var client = await _listener!.AcceptTcpClientAsync(ct);
                Logger.Log(
                    $"[BridgeServer] Client connected from {client.Client.RemoteEndPoint}");

                lock (_lock)
                {
                    client.SendTimeout = 5000;
                    client.ReceiveTimeout = 5000;
                    _client = client;
                    _stream = client.GetStream();
                    _stream.WriteTimeout = 5000;
                    _stream.ReadTimeout = 5000;
                    _readRemainder = "";
                }

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

                int bytesRead = await stream.ReadAsync(
                    _readBuffer, 0, _readBuffer.Length, ct);
                if (bytesRead == 0)
                {
                    Logger.Log("[BridgeServer] Client disconnected (read 0 bytes).");
                    break;
                }

                _readRemainder += Encoding.UTF8.GetString(_readBuffer, 0, bytesRead);

                while (_readRemainder.Contains('\n'))
                {
                    int idx = _readRemainder.IndexOf('\n');
                    string line = _readRemainder[..idx].Trim();
                    _readRemainder = _readRemainder[(idx + 1)..];

                    if (string.IsNullOrEmpty(line))
                        continue;

                    ProcessIncomingMessage(line);
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
            CancelPendingAction("Client disconnected");
            DisconnectClient();
        }
    }

    /// <summary>
    /// Process an incoming message from the Python client.
    /// If there's a pending WaitForActionAsync, deliver the message to it.
    /// Otherwise handle special messages (PING).
    /// </summary>
    private void ProcessIncomingMessage(string json)
    {
        try
        {
            // Check for PING
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("action", out var actionProp))
            {
                string action = actionProp.GetString() ?? "";
                if (action.Equals("ping", StringComparison.OrdinalIgnoreCase))
                {
                    SendState("{\"type\":\"pong\"}");
                    return;
                }
            }

            // Also support legacy "type" field for ping
            if (root.TryGetProperty("type", out var typeProp))
            {
                string type = typeProp.GetString() ?? "";
                if (type.Equals("PING", StringComparison.OrdinalIgnoreCase))
                {
                    SendState("{\"type\":\"pong\"}");
                    return;
                }
            }
        }
        catch
        {
            // If we can't parse, still deliver it to the pending action
        }

        // Deliver to pending WaitForActionAsync
        lock (_pendingLock)
        {
            if (_pendingAction != null)
            {
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    string? requestId = root.TryGetProperty("request_id", out var requestProp)
                        ? requestProp.GetString()
                        : null;
                    if (_pendingRequestId != null && requestId != _pendingRequestId)
                    {
                        Logger.Log(
                            $"[BridgeServer] Dropping stale action for request_id={requestId ?? "null"}, expected {_pendingRequestId}");
                        return;
                    }
                }
                catch
                {
                    if (_pendingRequestId != null)
                    {
                        Logger.Log("[BridgeServer] Dropping unparsable action while waiting for a correlated request.");
                        return;
                    }
                }
                _pendingAction.TrySetResult(json);
                _pendingAction = null;
                _pendingRequestId = null;
                return;
            }
        }

        // No pending action -- log and discard
        Logger.Log($"[BridgeServer] Received action with no handler waiting: {json}");
    }

    private void CancelPendingAction(string reason)
    {
        lock (_pendingLock)
        {
            _pendingAction?.TrySetCanceled();
            _pendingAction = null;
            _pendingRequestId = null;
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

    private static string AttachRequestId(string stateJson, string requestId)
    {
        try
        {
            Dictionary<string, object?> payload =
                JsonSerializer.Deserialize<Dictionary<string, object?>>(stateJson)
                ?? new Dictionary<string, object?>();
            payload["request_id"] = requestId;
            return JsonSerializer.Serialize(payload);
        }
        catch
        {
            return stateJson;
        }
    }
}
