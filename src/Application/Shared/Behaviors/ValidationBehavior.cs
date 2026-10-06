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
        private readonly ICommandHandler<TCommand> _inner = inner;
        private readonly IEnumerable<IValidator<TCommand>> _validators = validators;

        public async Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            foreach (IValidator<TCommand> validator in _validators)
            {
                Result validation = await validator.ValidateAsync(command, cancellationToken);

                if (validation.IsFailure)
                {
                    return validation;
                }
            }

            return await _inner.HandleAsync(command, cancellationToken);
        }
    }

    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> inner,
        IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly ICommandHandler<TCommand, TResponse> _inner = inner;
        private readonly IEnumerable<IValidator<TCommand>> _validators = validators;

        public async Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            foreach (IValidator<TCommand> validator in _validators)
            {
                Result validation = await validator.ValidateAsync(command, cancellationToken);

                if (validation.IsFailure)
                {
                    return Result.Failure<TResponse>(validation.Error);
                }
            }

            return await _inner.HandleAsync(command, cancellationToken);
        }
    }
}
