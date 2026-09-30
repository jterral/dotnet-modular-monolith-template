using Domain.Shared;

namespace Application.Shared;

public interface IValidator<in T>
{
    Task<Result> ValidateAsync(T instance, CancellationToken cancellationToken = default);
}
