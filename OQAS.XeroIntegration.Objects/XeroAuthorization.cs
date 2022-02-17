namespace OQAS.XeroIntegration.Objects
{
    public class XeroAuthorization
    {
        public System.Uri GetAuthorizationBaseUri { get; set; }
        public string ResponseType { get; set; }
        public string CodeChallengeMethod { get; set; }
        public string ClientId { get; set; }
        public string Scope { get; set; }
        public string RedirectUri { get; set; }
        public string State { get; set; }
        public string CodeChallenge { get; set; }
        public System.Uri GeneratedUri { get; set; }

        public System.Uri PostTokenBaseUri { get; set; }
        public string GrantType { get; set; }
        public string Code { get; set; }
        public string CodeVerifier { get; set; }

        public string IdToken { get; set; }
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string ExpiresDateTime { get; set; }
        public string TokenType { get; set; }
        public string RefreshToken { get; set; }
        public System.Uri TenantUri { get; set; }
        public string XeroTenantId { get; set; }
        public string XeroTenantName { get; set; }

        public System.Uri XeroBillsApiUri { get; set; }
        public string XeroBillsUriQueryParameters { get; set; }
        public System.Uri XeroAccountsApiUri { get; set; }
    }
}