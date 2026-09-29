using Application.Shared.Messaging;
using Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Application.Shared.Behaviors;

internal static class LoggingBehavior
{
    internal sealed class CommandHandler<TCommand>(
        ICommandHandler<TCommand> inner,
        ILogger<CommandHandler<TCommand>> logger) : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            string name = typeof(TCommand).Name;
            logger.LogInformation("Executing command {Command}", name);

            Result result = await inner.HandleAsync(command, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Command {Command} completed", name);
            }
            else
            {
                logger.LogWarning("Command {Command} failed: {Error}", name, result.Error);
            }

            return result;
        }
    }

    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> inner,
        ILogger<CommandHandler<TCommand, TResponse>> logger) : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            string name = typeof(TCommand).Name;
            logger.LogInformation("Executing command {Command}", name);

            Result<TResponse> result = await inner.HandleAsync(command, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Command {Command} completed", name);
            }
            else
            {
                logger.LogWarning("Command {Command} failed: {Error}", name, result.Error);
            }

            return result;
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> inner,
        ILogger<QueryHandler<TQuery, TResponse>> logger) : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
        {
            string name = typeof(TQuery).Name;
            logger.LogInformation("Executing query {Query}", name);

            Result<TResponse> result = await inner.HandleAsync(query, cancellationToken);

            if (result.IsFailure)
            {
                logger.LogWarning("Query {Query} failed: {Error}", name, result.Error);
            }

            return result;
        }
    }
}
