using System;
using System.IdentityModel.Tokens;
using System.Linq;

namespace jpd.ms.TenantTableProvider
{
    public class TableStorageIssuerNameRegistry : ValidatingIssuerNameRegistry
    {
        private static TenantTableContext _issuerTable => new TenantTableContext();

        public static bool ContainsTenant(string tenantId)
        {
            return _issuerTable.Tenants.ToList().Any(x => x.Id == tenantId);
        }

        public static bool ContainsKey(string thumbprint)
        {
            return _issuerTable.IssuingAuthorityKeys.ToList().Any(x => x.Id == thumbprint);
        }

        public static void RefreshKeys(string metadataLocation)
        {
            var ia = GetIssuingAuthority(metadataLocation);
            var newKeys = false;
            var refreshTenant = false;
            if (ia.Thumbprints.Any(thumbprint => !ContainsKey(thumbprint)))
            {
                newKeys = true;
                refreshTenant = true;
            }

            if (ia.Issuers.Any(issuer => !ContainsTenant(GetIssuerId(issuer))))
            {
                refreshTenant = true;
            }

            if (!newKeys && !refreshTenant) return;
            if (newKeys)
            {
                _issuerTable.RemoveRange(_issuerTable.IssuingAuthorityKeys);
                foreach (var thumbprint in ia.Thumbprints)
                {
                    _issuerTable.Add<IssuingAuthorityKey>(new IssuingAuthorityKey { Id = thumbprint });
                }
            }

            if (!refreshTenant) return;
            // Add the default tenant to the registry. 
            // Comment or remove the following code if you do not wish to have the default tenant use the application.
            foreach (var issuerId in ia.Issuers.Select(GetIssuerId).Where(issuerId => !ContainsTenant(issuerId)))
            {
                _issuerTable.Add<Tenant>(new Tenant { Id = issuerId });
            }
        }

        public static bool TryAddTenant(string tenantId, string signupToken)
        {
            if (ContainsTenant(tenantId)) return false;
            var existingToken = _issuerTable.SignupTokens.FirstOrDefault(token => token.Id == signupToken);
            if (existingToken == null) return false;
            _issuerTable.Remove(existingToken);
            _issuerTable.Add<Tenant>(new Tenant { Id = tenantId });
            return true;
        }

        public static void AddSignupToken(string signupToken, DateTimeOffset expirationTime)
        {
            _issuerTable.Add<SignupToken>(new SignupToken
            {
                Id = signupToken,
                ExpirationDate = expirationTime
            });
        }

        public static void CleanUpExpiredSignupTokens()
        {
            var now = DateTimeOffset.UtcNow;
            var tokensToRemove = _issuerTable.SignupTokens.Where(token => token.ExpirationDate <= now);
            if (!tokensToRemove.Any()) return;
            _issuerTable.RemoveRange(tokensToRemove);
        }

        private static string GetIssuerId(string issuer)
        {
            return issuer.TrimEnd('/').Split('/').Last();
        }

        protected override bool IsThumbprintValid(string thumbprint, string issuer)
        {
            return ContainsTenant(GetIssuerId(issuer)) && ContainsKey(thumbprint);
        }
    }
}
