using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using DroidBox.Shared.Constants;

namespace DroidBox.Cli.Services;

public record IpcMessage(string Command, Dictionary<string, string>? Arguments = null);
public record IpcResponse(bool Success, string Message, string? PayloadJson = null);

public class IpcClient
{
    public static async Task<IpcResponse> SendCommandAsync(string command, Dictionary<string, string>? args = null, int timeoutMs = 5000, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", AppConstants.PipeName, PipeDirection.InOut);
            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await client.ConnectAsync(linked.Token);

            var req = new IpcMessage(command, args);
            var json = JsonSerializer.Serialize(req);
            var bytes = Encoding.UTF8.GetBytes(json);

            await client.WriteAsync(BitConverter.GetBytes(bytes.Length), linked.Token);
            await client.WriteAsync(bytes, linked.Token);
            await client.FlushAsync(linked.Token);

            // Read response
            var lenBuffer = new byte[4];
            await client.ReadExactlyAsync(lenBuffer, linked.Token);
            var resLen = BitConverter.ToInt32(lenBuffer, 0);

            var resBuffer = new byte[resLen];
            await client.ReadExactlyAsync(resBuffer, linked.Token);
            var resJson = Encoding.UTF8.GetString(resBuffer);

            return JsonSerializer.Deserialize<IpcResponse>(resJson) ?? new IpcResponse(false, "Invalid response from service");
        }
        catch (TimeoutException)
        {
            return new IpcResponse(false, "Timeout connecting to DroidBox service");
        }
        catch (Exception ex)
        {
            return new IpcResponse(false, $"IPC communication error: {ex.Message}");
        }
    }
}
