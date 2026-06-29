using DotCruz.Notifications.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace DotCruz.Notifications.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthenticatedServiceAttribute : TypeFilterAttribute
{
    public AuthenticatedServiceAttribute() : base(typeof(AuthenticatedServiceFilter)) { } 
}
