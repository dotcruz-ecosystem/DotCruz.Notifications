using DotCruz.Notifications.CrossCutting.Resources;
using System.Globalization;

namespace DotCruz.Notifications.Application.Common.Utils;

public static class EmailBrandingBuilder
{
    private const string HeaderTemplate = @"<!DOCTYPE html>
<html lang=""{{BRANDING_LANG}}"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{{BRANDING_HEADER_TITLE}}</title>
    <!-- O CSS interno é usado como fallback, mas o ideal em emails é sempre o CSS inline -->
    <style>
        body {{
            margin: 0;
            padding: 0;
            background-color: #f4f5f7;
            font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
            -webkit-font-smoothing: antialiased;
        }}
        table {{
            border-collapse: collapse;
            mso-table-lspace: 0pt;
            mso-table-rspace: 0pt;
        }}
        img {{
            border: 0;
            line-height: 100%;
            outline: none;
            text-decoration: none;
            display: block; /* Remove espaços em branco embaixo de imagens no Gmail/Outlook */
        }}
        a {{
            text-decoration: none;
        }}
    </style>
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f5f7; font-family: Arial, sans-serif;"">

    <!-- Tabela principal que ocupa 100% da tela (Fundo cinza do email) -->
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f4f5f7; width: 100%;"">
        <tr>
            <td align=""center"" style=""padding: 40px 10px;"">
                
                <!-- Container Principal (Limitado a 600px de largura para boa leitura) -->
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #ffffff; width: 100%; max-width: 600px; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.05);"">
                    
                    <!-- ============================================== -->
                    <!-- HEADER DA EMPRESA (INÍCIO)                     -->
                    <!-- ============================================== -->
                    <!-- Dica: O lojista/tenant pode personalizar a cor de fundo (background-color) -->
                    <tr>
                        <td align=""center"" style=""padding: 30px 20px; background-color: {0}; border-bottom: 1px solid #eeeeee;"">
                            <a href=""{1}"" target=""_blank"" style=""text-decoration: none;"">
                                <!-- LOGO DO TENANT -->
                                <!-- 
                                  width=""150"": Tamanho base recomendado 
                                  max-width: Garante que logos não fiquem gigantes no mobile
                                -->
                                <img src=""{2}"" alt=""{3} Logo"" width=""150"" style=""max-width: 200px; height: auto; border: none; display: block; font-size: 20px; font-weight: bold; color: #333333; text-align: center;"" />
                            </a>
                        </td>
                    </tr>
                    <!-- ============================================== -->
                    <!-- HEADER DA EMPRESA (FIM)                        -->
                    <!-- ============================================== -->

                    <tr>
                        <td style=""padding: 40px 30px; font-size: 16px; line-height: 1.6; color: #333333;"">";

    private const string FooterTemplate = @"</td>
                    </tr>
                    <!-- ============================================== -->
                    <!-- FOOTER (INÍCIO)                                -->
                    <!-- ============================================== -->
                    <tr>
                        <td align=""center"" style=""padding: 20px 30px; background-color: #fafafa; border-top: 1px solid #eeeeee; font-size: 12px; line-height: 1.5; color: #888888;"">
                            <p style=""margin: 0 0 10px 0;"">
                                {{BRANDING_FOOTER_RECEIVING_TEXT_START}}<strong>{0}</strong>{{BRANDING_FOOTER_RECEIVING_TEXT_END}}
                            </p>
                            <p style=""margin: 0;"">
                                © {1} {0}. {{BRANDING_FOOTER_RIGHTS_RESERVED}}<br>
                                {2}
                            </p>
                            <p style=""margin-top: 10px;"">
                                <a href=""{3}"" style=""color: #0056b3; text-decoration: underline;"">{{BRANDING_FOOTER_UNSUBSCRIBE}}</a>
                            </p>
                        </td>
                    </tr>
                    <!-- ============================================== -->
                    <!-- FOOTER (FIM)                                   -->
                    <!-- ============================================== -->

                </table>
                <!-- Fim do Container Principal -->

                <!-- Branding da sua própria plataforma (opcional, ex: ""Enviado por SeuServiço"") -->
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""width: 100%; max-width: 600px;"">
                    <tr>
                        <td align=""center"" style=""padding: 20px; font-size: 12px; color: #aaaaaa;"">
                            <a href=""https://dotcruz.com"" style=""color: #aaaaaa; text-decoration: none;"">
                                Powered by <strong>DotCruz</strong>
                            </a>
                        </td>
                    </tr>
                </table>

            </td>
        </tr>
    </table>
</body>
</html>";

    public static (string HeaderHtml, string FooterHtml) Build(
        string tenantName,
        string tenantLogoUrl,
        string tenantWebsite,
        string tenantAddress,
        string unsubscribeUrl,
        string headerBackgroundColor = "#ffffff")
    {
        var headerHtml = string.Format(
            HeaderTemplate,
            string.IsNullOrWhiteSpace(headerBackgroundColor) ? "#ffffff" : headerBackgroundColor,
            tenantWebsite,
            tenantLogoUrl,
            tenantName);

        var footerHtml = string.Format(
            FooterTemplate,
            tenantName,
            DateTime.UtcNow.Year.ToString(),
            tenantAddress,
            unsubscribeUrl);

        return (headerHtml, footerHtml);
    }

    public static (string HeaderHtml, string FooterHtml) GenerateBranding(
        Domain.Entities.Tenants.TenantSettings settings,
        string? cultureName)
    {
        var (headerHtml, footerHtml) = Build(
            settings.TenantName,
            settings.TenantLogoUrl,
            settings.TenantWebsite,
            settings.TenantAddress,
            settings.UnsubscribeUrl,
            settings.HeaderBackgroundColor);

        return (Translate(headerHtml, cultureName), Translate(footerHtml, cultureName));
    }

    public static string Translate(string html, string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(html))
            return html;

        CultureInfo culture;
        try
        {
            culture = string.IsNullOrWhiteSpace(cultureName) ? new CultureInfo("en") : new CultureInfo(cultureName);
        }
        catch
        {
            culture = new CultureInfo("en");
        }

        var replacements = new Dictionary<string, string>
        {
            { "{BRANDING_LANG}", culture.TwoLetterISOLanguageName },
            { "{BRANDING_HEADER_TITLE}", ResourceEmailBranding.ResourceManager.GetString("BRANDING_HEADER_TITLE", culture) ?? "Email Template" },
            { "{BRANDING_FOOTER_RECEIVING_TEXT_START}", ResourceEmailBranding.ResourceManager.GetString("BRANDING_FOOTER_RECEIVING_TEXT_START", culture) ?? "You are receiving this email because you are registered at " },
            { "{BRANDING_FOOTER_RECEIVING_TEXT_END}", ResourceEmailBranding.ResourceManager.GetString("BRANDING_FOOTER_RECEIVING_TEXT_END", culture) ?? "." },
            { "{BRANDING_FOOTER_RIGHTS_RESERVED}", ResourceEmailBranding.ResourceManager.GetString("BRANDING_FOOTER_RIGHTS_RESERVED", culture) ?? "All rights reserved." },
            { "{BRANDING_FOOTER_UNSUBSCRIBE}", ResourceEmailBranding.ResourceManager.GetString("BRANDING_FOOTER_UNSUBSCRIBE", culture) ?? "Unsubscribe" }
        };

        foreach (var replacement in replacements)
        {
            html = html.Replace(replacement.Key, replacement.Value);
        }

        return html;
    }
}
