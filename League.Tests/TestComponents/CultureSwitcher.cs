using System.Globalization;

namespace League.Tests.TestComponents;

public class CultureSwitcher : IDisposable
{
    private readonly CultureInfo _originalCulture;
    private readonly CultureInfo _originalUiCulture;
    private readonly CultureInfo? _originalDefaultThreadCulture;
    private readonly CultureInfo? _originalDefaultThreadUiCulture;

    public CultureSwitcher(CultureInfo culture, CultureInfo uiCulture)
    {
        _originalCulture = CultureInfo.CurrentCulture;
        _originalUiCulture = CultureInfo.CurrentUICulture;
        _originalDefaultThreadCulture = CultureInfo.DefaultThreadCurrentCulture;
        _originalDefaultThreadUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
        SetCurrentCulture(culture, uiCulture);
        SetThreadDefaultCulture(culture, uiCulture);
    }

    private static void SetCurrentCulture(CultureInfo culture, CultureInfo uiCulture)
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = uiCulture;
    }

    private static void SetThreadDefaultCulture(CultureInfo? culture, CultureInfo? uiCulture)
    {
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        SetCurrentCulture(_originalCulture, _originalUiCulture);
        SetThreadDefaultCulture(_originalDefaultThreadCulture, _originalDefaultThreadUiCulture);
    }
}
