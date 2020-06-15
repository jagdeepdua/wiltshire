using System;
using System.Configuration;

namespace OidcWebApp_Multiple.Utils
{
    public static class Globals
    {
        public static bool UseB2C = Convert.ToBoolean(ConfigurationManager.AppSettings["UseB2C"]);
    }

    public static class B2C_Globals
    {
        // App config settings
        public static string ClientId = ConfigurationManager.AppSettings["b2c:ClientId"];
        public static string ClientSecret = ConfigurationManager.AppSettings["b2c:ClientSecret"];
        public static string AadInstance = ConfigurationManager.AppSettings["b2c:AadInstance"];
        public static string Tenant = ConfigurationManager.AppSettings["b2c:Tenant"];
        public static string TenantId = ConfigurationManager.AppSettings["b2c:TenantId"];
        public static string RedirectUri = ConfigurationManager.AppSettings["b2c:RedirectUri"];

        // public static string ServiceUrl = ConfigurationManager.AppSettings["api:TaskServiceUrl"];

        private static string AppKey = ConfigurationManager.AppSettings["b2c:ClientSecret"];

        // B2C policy identifiers
        public static string SignUpSignInPolicyId = ConfigurationManager.AppSettings["b2c:SignUpSignInPolicyId"];
        public static string EditProfilePolicyId = ConfigurationManager.AppSettings["b2c:EditProfilePolicyId"];
        public static string ResetPasswordPolicyId = ConfigurationManager.AppSettings["b2c:ResetPasswordPolicyId"];

        public static string DefaultPolicy = SignUpSignInPolicyId;

        // API Scopes
        public static string ApiIdentifier = ConfigurationManager.AppSettings["api:ApiIdentifier"];
        public static string ReadTasksScope = ApiIdentifier + ConfigurationManager.AppSettings["api:ReadScope"];
        public static string WriteTasksScope = ApiIdentifier + ConfigurationManager.AppSettings["api:WriteScope"];
        public static string[] Scopes = new string[] { ReadTasksScope, WriteTasksScope };

        // OWIN auth middleware constants
        public const string ObjectIdElement = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        // Authorities
        public static string Authority = string.Format(AadInstance, Tenant, DefaultPolicy);
        public static string WellKnownMetadata = $"{AadInstance}/v2.0/.well-known/openid-configuration";
    }

    public static class AAD_Globals
    {
        // App config settings
        public static string ClientId = ConfigurationManager.AppSettings["aad:ClientId"];
        public static string ClientSecret = ConfigurationManager.AppSettings["aad:ClientSecret"];
        public static string AadInstance = EnsureTrailingSlash(ConfigurationManager.AppSettings["aad:AadInstance"]);
        public static string TenantId = ConfigurationManager.AppSettings["aad:TenantId"];
        public static string RedirectUri = ConfigurationManager.AppSettings["aad:RedirectUri"];

        // Authorities
        public static string Authority = AadInstance + TenantId;

        // public static string WellKnownMetadata = $"{Authority}/.well-known/openid-configuration";
        
        private static string EnsureTrailingSlash(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }

            return value;
        }
    }
}