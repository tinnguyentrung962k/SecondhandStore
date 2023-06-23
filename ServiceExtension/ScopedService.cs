using SecondhandStore.Repository;
using SecondhandStore.Services;

namespace SecondhandStore.ServiceExtension;

public static class ScopedService
{
    public static void AddScopedService(this IServiceCollection services)
    {
        services.AddScoped<RoleRepository>();
        services.AddScoped<RoleService>();

        services.AddScoped<AccountRepository>();
        services.AddScoped<AccountService>();

        services.AddScoped<PostRepository>();
        services.AddScoped<PostService>();

        services.AddScoped<TopUpRepository>();
        services.AddScoped<TopUpService>();
    }
}