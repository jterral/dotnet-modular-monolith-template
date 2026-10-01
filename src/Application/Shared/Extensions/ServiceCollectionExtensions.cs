using Application.Shared.Behaviors;
using Application.Shared.Events;
using Application.Shared.Messaging;
using Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Shared.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddCommandHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : ICommand
        where THandler : class, ICommandHandler<TCommand>
    {
        services.AddScoped<THandler>();

        services.AddScoped(sp =>
        {
            ICommandHandler<TCommand> handler = sp.GetRequiredService<THandler>();

            handler = new ValidationBehavior.CommandHandler<TCommand>(
                handler,
                sp.GetServices<IValidator<TCommand>>());

            handler = new LoggingBehavior.CommandHandler<TCommand>(
                handler,
                sp.GetRequiredService<ILogger<LoggingBehavior.CommandHandler<TCommand>>>());

            return handler;
        });

        return services;
    }

    internal static IServiceCollection AddCommandHandler<TCommand, TResponse, THandler>(this IServiceCollection services)
        where TCommand : ICommand<TResponse>
        where THandler : class, ICommandHandler<TCommand, TResponse>
    {
        services.AddScoped<THandler>();

        services.AddScoped(sp =>
        {
            ICommandHandler<TCommand, TResponse> handler = sp.GetRequiredService<THandler>();

            handler = new ValidationBehavior.CommandHandler<TCommand, TResponse>(
                handler,
                sp.GetServices<IValidator<TCommand>>());

            handler = new LoggingBehavior.CommandHandler<TCommand, TResponse>(
                handler,
                sp.GetRequiredService<ILogger<LoggingBehavior.CommandHandler<TCommand, TResponse>>>());

            return handler;
        });

        return services;
    }

    internal static IServiceCollection AddQueryHandler<TQuery, TResponse, THandler>(this IServiceCollection services)
        where TQuery : IQuery<TResponse>
        where THandler : class, IQueryHandler<TQuery, TResponse>
    {
        services.AddScoped<THandler>();

        services.AddScoped(sp =>
        {
            IQueryHandler<TQuery, TResponse> handler = sp.GetRequiredService<THandler>();

            handler = new LoggingBehavior.QueryHandler<TQuery, TResponse>(
                handler,
                sp.GetRequiredService<ILogger<LoggingBehavior.QueryHandler<TQuery, TResponse>>>());

            return handler;
        });

        return services;
    }

    internal static IServiceCollection AddDomainEventHandler<TDomainEvent, THandler>(this IServiceCollection services)
        where TDomainEvent : IDomainEvent
        where THandler : class, IDomainEventHandler<TDomainEvent>
    {
        services.AddScoped<IDomainEventHandler<TDomainEvent>, THandler>();

        return services;
    }

    internal static IServiceCollection AddValidator<TCommand, TValidator>(this IServiceCollection services)
        where TCommand : ICommand
        where TValidator : class, IValidator<TCommand>
    {
        services.AddScoped<IValidator<TCommand>, TValidator>();
        return services;
    }

    internal static IServiceCollection AddValidator<TCommand, TResponse, TValidator>(this IServiceCollection services)
        where TCommand : ICommand<TResponse>
        where TValidator : class, IValidator<TCommand>
    {
        services.AddScoped<IValidator<TCommand>, TValidator>();
        return services;
    }
}
