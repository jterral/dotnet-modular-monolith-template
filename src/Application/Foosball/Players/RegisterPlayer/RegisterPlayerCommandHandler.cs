using Application.Shared.Messaging;
using Domain.Foosball.Players;
using Domain.Shared;

namespace Application.Foosball.Players.RegisterPlayer;

internal sealed class RegisterPlayerCommandHandler(
    IPlayerRepository players,
    IFoosballUnitOfWork unitOfWork) : ICommandHandler<RegisterPlayerCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(
        RegisterPlayerCommand command,
        CancellationToken cancellationToken = default)
    {
        Result<PlayerName> name = PlayerName.Create(command.Name);

        if (name.IsFailure)
        {
            return Result.Failure<Guid>(name.Error);
        }

        if (await players.NameExistsAsync(name.Value, cancellationToken))
        {
            return Result.Failure<Guid>(PlayerErrors.NameAlreadyTaken);
        }

        var player = Player.Create(name.Value);
        players.Add(player);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return player.Id;
    }
}
