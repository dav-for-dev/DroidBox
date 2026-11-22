using DroidBox.Core.Models;

namespace DroidBox.Core.Interfaces;

public interface IAvdManager
{
    Task<bool> AvdExistsAsync(string avdName, CancellationToken cancellationToken = default);
    Task<OperationResult> EnsureAvdsCreatedAsync(string sdkRoot, CancellationToken cancellationToken = default);
    Task<OperationResult> CreateMobileAvdAsync(string sdkRoot, int ramMb = 2048, int storageMb = 4096, CancellationToken cancellationToken = default);
    Task<OperationResult> CreateTabletAvdAsync(string sdkRoot, int ramMb = 2048, int storageMb = 4096, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListAvdsAsync(CancellationToken cancellationToken = default);
}
