using ABCDMall.Modules.Events.Application.DTOs.Events;
using ABCDMall.Modules.Events.Application.Mappings;
using ABCDMall.Modules.Events.Application.Services.Events;
using ABCDMall.Modules.Events.Application.Services.Events.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ABCDMall.Modules.Events.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddEventsApplication(
        this IServiceCollection services,
        string? autoMapperLicenseKey)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.LicenseKey = autoMapperLicenseKey;
        }, typeof(EventProfile));

        services.AddScoped<IValidator<EventListQueryDto>, EventListQueryDtoValidator>();
        services.AddScoped<IValidator<CreateEventRequestDto>, CreateEventRequestDtoValidator>();
        services.AddScoped<IValidator<UpdateEventRequestDto>, UpdateEventRequestDtoValidator>();

        services.AddScoped<IEventQueryService, EventQueryService>();
        services.AddScoped<IEventCommandService, EventCommandService>();

        return services;
    }
}