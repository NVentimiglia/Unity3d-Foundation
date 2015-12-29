using System.Security.Principal;

namespace Foundation.Server.Infrastructure
{
    /// <summary>
    /// Overrides default ASP.NET Identity with Foundation AppSession
    /// </summary>
    public class FoundationIdentity : IIdentity
    {

        public string AuthenticationType
        {
            get { return "Foundation"; }
        }

        public bool IsAuthenticated { get; set; }

        public string Name { get; set; }
    }

    /// <summary>
    /// Overrides default ASP.NET Provider with Foundation AppSession
    /// </summary>
    public class FoundationPrincipal : IPrincipal
    {
        public IIdentity Identity { get; set; }

        public bool IsInRole(string role)
        {
            return false;
        }
    }
}