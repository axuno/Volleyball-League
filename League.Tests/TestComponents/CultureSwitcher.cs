using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace League.Test.TestComponents;

public class CultureSwitcher : IDisposable
{
    private readonly CultureInfo _originalCulture;
    private readonly CultureInfo _originalUiCulture;

    public CultureSwitcher(CultureInfo culture, CultureInfo uiCulture)
    {
        _originalCulture = CultureInfo.CurrentCulture;
        _originalUiCulture = CultureInfo.CurrentUICulture;
        SetCulture(culture, uiCulture);
    }

    private static void SetCulture(CultureInfo culture, CultureInfo uiCulture)
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = uiCulture;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        SetCulture(_originalCulture, _originalUiCulture);
    }
}