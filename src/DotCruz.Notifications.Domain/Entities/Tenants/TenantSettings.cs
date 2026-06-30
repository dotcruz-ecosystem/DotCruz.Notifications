using DotCruz.Notifications.Domain.Entities.Base;
using System;

namespace DotCruz.Notifications.Domain.Entities.Tenants
{
    public class TenantSettings : TenantEntity
    {
        public string TenantName { get; private set; } = string.Empty;
        public string TenantLogoUrl { get; private set; } = string.Empty;
        public string TenantWebsite { get; private set; } = string.Empty;
        public string TenantAddress { get; private set; } = string.Empty;
        public string UnsubscribeUrl { get; private set; } = string.Empty;
        public string HeaderBackgroundColor { get; private set; } = string.Empty;

        private TenantSettings() { }

        public TenantSettings(
            Guid tenantId,
            string tenantName,
            string tenantLogoUrl,
            string tenantWebsite,
            string tenantAddress,
            string unsubscribeUrl,
            string headerBackgroundColor)
        {
            SetTenantId(tenantId);
            TenantName = tenantName;
            TenantLogoUrl = tenantLogoUrl;
            TenantWebsite = tenantWebsite;
            TenantAddress = tenantAddress;
            UnsubscribeUrl = unsubscribeUrl;
            HeaderBackgroundColor = headerBackgroundColor;
        }

        public void UpdateBranding(
            string tenantName,
            string tenantLogoUrl,
            string tenantWebsite,
            string tenantAddress,
            string unsubscribeUrl,
            string headerBackgroundColor)
        {
            TenantName = tenantName;
            TenantLogoUrl = tenantLogoUrl;
            TenantWebsite = tenantWebsite;
            TenantAddress = tenantAddress;
            UnsubscribeUrl = unsubscribeUrl;
            HeaderBackgroundColor = headerBackgroundColor;
            Touch();
        }
    }
}
