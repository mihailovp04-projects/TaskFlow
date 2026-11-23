namespace TaskFlow.Web.Auth;

// Защита от open redirect: после входа переходим только по относительному пути внутри приложения
public static class SafeReturnUrl
{
    public static string From(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return "";
        }

        // "//evil.com", "\evil.com", "https://evil.com", "javascript:..." отбрасываем
        if (returnUrl.StartsWith('/') || returnUrl.StartsWith('\\') || returnUrl.Contains(':'))
        {
            return "";
        }

        return returnUrl;
    }
}
