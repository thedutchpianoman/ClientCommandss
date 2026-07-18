using System;
using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;

namespace ClientCommands.Commands
{
    /// <summary>
    /// .coinflip — Flips a virtual coin and broadcasts the result (Heads / Tails) to the player.
    /// </summary>
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class CoinFlipCommand : ICommand
    {
        // ── ICommand ──────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public string Command { get; } = "coinflip";

        /// <inheritdoc/>
        public string[] Aliases { get; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string Description { get; } = "Flips a coin — Heads or Tails.";

        // ── Shared RNG ────────────────────────────────────────────────────────

        private static readonly Random Rng = new();

        // ── Execute ───────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // Only real players can use this command.
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "This command can only be used by a player in-game.";
                return false;
            }

            Player player = Player.Get(playerSender.ReferenceHub);

            if (player is null)
            {
                response = "Could not resolve your player object. Please try again.";
                return false;
            }

            // 50/50 coin flip.
            string result = Rng.Next(0, 2) == 0 ? "Heads" : "Tails";

            player.Broadcast(
                duration: 4,
                message: $"🪙 Coin flip: <b>{result}</b>",
                type: Broadcast.BroadcastFlags.Normal,
                shouldClearPrevious: false);

            response = $"🪙 {result}";
            return true;
        }
    }
}
