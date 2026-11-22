using DroidBox.Core.Models;

namespace DroidBox.Core.Interfaces;

public interface IAndroidLicenseManager
{
    Task<bool> AreLicensesAcceptedAsync(string sdkRoot, CancellationToken cancellationToken = default);
    Task<OperationResult> AcceptAllLicensesAsync(string sdkRoot, CancellationToken cancellationToken = default);
    string GetLicenseAgreementText();
}
