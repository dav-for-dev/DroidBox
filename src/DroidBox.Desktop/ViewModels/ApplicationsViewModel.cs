using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroidBox.Application.Services;
using DroidBox.Core.Entities;
using DroidBox.Desktop.Services;

namespace DroidBox.Desktop.ViewModels;

public partial class ApplicationsViewModel : ObservableObject
{
    private readonly IApkService _apkService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private InstalledApp? _selectedApp;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<InstalledApp> Applications { get; } = new();

    public ApplicationsViewModel(IApkService apkService, IDialogService dialogService)
    {
        _apkService = apkService;
        _dialogService = dialogService;
        RefreshAppsAsync();
    }

    [RelayCommand]
    public async Task RefreshAppsAsync()
    {
        IsLoading = true;
        Applications.Clear();
        var apps = await _apkService.GetInstalledAppsAsync();
        foreach (var a in apps)
        {
            Applications.Add(a);
        }
        IsLoading = false;
    }

    [RelayCommand]
    public async Task InstallApkAsync()
    {
        var apkPath = _dialogService.OpenApkFileDialog();
        if (!string.IsNullOrWhiteSpace(apkPath))
        {
            await InstallFromPathAsync(apkPath);
        }
    }

    public async Task InstallFromPathAsync(string apkPath)
    {
        StatusMessage = $"Installing {System.IO.Path.GetFileName(apkPath)}...";
        var result = await _apkService.InstallAsync(apkPath);
        StatusMessage = result.Message;
        await RefreshAppsAsync();
    }

    [RelayCommand]
    public async Task LaunchSelectedAppAsync()
    {
        if (SelectedApp == null) return;
        StatusMessage = $"Launching {SelectedApp.AppName}...";
        var res = await _apkService.LaunchAsync(SelectedApp.PackageName);
        StatusMessage = res.Message;
    }

    [RelayCommand]
    public async Task UninstallSelectedAppAsync()
    {
        if (SelectedApp == null) return;
        StatusMessage = $"Uninstalling {SelectedApp.AppName}...";
        var res = await _apkService.UninstallAsync(SelectedApp.PackageName);
        StatusMessage = res.Message;
        await RefreshAppsAsync();
    }
}
