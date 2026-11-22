using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using DroidBox.Application.Services;
using DroidBox.Shared.Constants;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DroidBox.Bootstrapper;

public record IpcMessage(string Command, Dictionary<string, string>? Arguments = null);
public record IpcResponse(bool Success, string Message, string? PayloadJson = null);

public class IpcServer : BackgroundService
{
    private readonly IEmulatorService _emulatorService;
    private readonly IApkService _apkService;
    private readonly IDeviceService _deviceService;
    private readonly ILogger<IpcServer> _logger;

    public IpcServer(
        IEmulatorService emulatorService,
        IApkService apkService,
        IDeviceService deviceService,
        ILogger<IpcServer> logger)
    {
        _emulatorService = emulatorService;
        _apkService = apkService;
        _deviceService = deviceService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting DroidBox IPC Named Pipe Server: {Pipe}", AppConstants.PipeName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var server = new NamedPipeServerStream(
                    AppConstants.PipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync(stoppingToken);
                await HandleClientAsync(server, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IPC Server listener");
                await Task.Delay(1000, stoppingToken);
            }
        }

        _logger.LogInformation("DroidBox IPC Server stopped");
    }

    private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken ct)
    {
        try
        {
            var lenBuffer = new byte[4];
            await pipe.ReadExactlyAsync(lenBuffer, ct);
            var reqLen = BitConverter.ToInt32(lenBuffer, 0);

            var reqBuffer = new byte[reqLen];
            await pipe.ReadExactlyAsync(reqBuffer, ct);
            var reqJson = Encoding.UTF8.GetString(reqBuffer);

            var req = JsonSerializer.Deserialize<IpcMessage>(reqJson);
            IpcResponse response;

            if (req == null)
            {
                response = new IpcResponse(false, "Invalid IPC request");
            }
            else
            {
                response = await ProcessCommandAsync(req, ct);
            }

            var resJson = JsonSerializer.Serialize(response);
            var resBytes = Encoding.UTF8.GetBytes(resJson);

            await pipe.WriteAsync(BitConverter.GetBytes(resBytes.Length), ct);
            await pipe.WriteAsync(resBytes, ct);
            await pipe.FlushAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle IPC client request");
        }
    }

    private async Task<IpcResponse> ProcessCommandAsync(IpcMessage req, CancellationToken ct)
    {
        _logger.LogInformation("Processing IPC command: {Command}", req.Command);

        return req.Command.ToLowerInvariant() switch
        {
            "status" => new IpcResponse(true, $"State: {await _emulatorService.GetStateAsync(ct)}"),
            "stop" => new IpcResponse(true, (await _emulatorService.StopAsync(ct)).Message),
            "start" => new IpcResponse(true, (await _emulatorService.StartAsync(null, ct)).Message),
            _ => new IpcResponse(false, $"Unknown IPC command: {req.Command}")
        };
    }
}
