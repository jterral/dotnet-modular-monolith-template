using Application.Shared;
using Application.Shared.Messaging;
using Domain.Shared;

namespace Application.Shared.Behaviors;

internal static class ValidationBehavior
{
    internal sealed class CommandHandler<TCommand>(
        ICommandHandler<TCommand> inner,
        IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            foreach (IValidator<TCommand> validator in validators)
            {
                Result validation = await validator.ValidateAsync(command, cancellationToken);

                if (validation.IsFailure)
                {
                    return validation;
                }
            }

            return await inner.HandleAsync(command, cancellationToken);
        }
    }

    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> inner,
        IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            foreach (IValidator<TCommand> validator in validators)
            {
                Result validation = await validator.ValidateAsync(command, cancellationToken);

                if (validation.IsFailure)
                {
                    return Result.Failure<TResponse>(validation.Error);
                }
            }

            return await inner.HandleAsync(command, cancellationToken);
        }
    }
}
