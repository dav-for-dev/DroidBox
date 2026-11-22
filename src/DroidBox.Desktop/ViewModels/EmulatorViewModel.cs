using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidBox.Application.Services;
using DroidBox.Core.Entities;
using DroidBox.Core.Enums;
using DroidBox.Display;

namespace DroidBox.Desktop.ViewModels;

public partial class EmulatorViewModel : ObservableObject
{
    private readonly IEmulatorService _emulatorService;
    private readonly IDeviceService _deviceService;
    private readonly IScreenCaptureService _screenService;

    [ObservableProperty]
    private EmulatorState _state = EmulatorState.Stopped;

    [ObservableProperty]
    private string _activeProfileName = "Tablet";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public EmulatorViewModel(
        IEmulatorService emulatorService,
        IDeviceService deviceService,
        IScreenCaptureService screenService)
    {
        _emulatorService = emulatorService;
        _deviceService = deviceService;
        _screenService = screenService;
        CheckStateAsync();
    }

    public async void CheckStateAsync()
    {
        State = await _emulatorService.GetStateAsync();
        var p = await _deviceService.GetActiveProfileAsync();
        if (p != null) ActiveProfileName = p.DisplayName;
    }

    [RelayCommand]
    public async Task StartAsync()
    {
        StatusMessage = "Starting...";
        var res = await _emulatorService.StartAsync();
        State = await _emulatorService.GetStateAsync();
        StatusMessage = res.Message;
    }

    [RelayCommand]
    public async Task StopAsync()
    {
        StatusMessage = "Stopping...";
        var res = await _emulatorService.StopAsync();
        State = await _emulatorService.GetStateAsync();
        StatusMessage = res.Message;
    }

    [RelayCommand]
    public async Task SwitchToTabletAsync()
    {
        await _deviceService.SwitchProfileAsync("tablet");
        CheckStateAsync();
    }

    [RelayCommand]
    public async Task SwitchToMobileAsync()
    {
        await _deviceService.SwitchProfileAsync("mobile");
        CheckStateAsync();
    }
}
