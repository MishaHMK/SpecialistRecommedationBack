using TherapyApp.Helpers.Secrets;

namespace TherapyApp.Extensions;

public static class ConfigureHostBuilderExtensions
{
    public static void ConfigureJwt(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.Configure<JwtVariables>(builder.Configuration.GetSection("Jwt"));
    }

    public static void ConfigureOpenAI(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.Configure<OpenAIVariables>(builder.Configuration.GetSection("OpenAI"));
    }
}
