using TherapyApp.Helpers.Secrets;

namespace TherapyApp.Extensions;

public static class ConfigureHostBuilderExtensions
{
    public static void ConfigureJwt(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.Configure<JwtVariables>(builder.Configuration.GetSection("Jwt"));
    }
}
