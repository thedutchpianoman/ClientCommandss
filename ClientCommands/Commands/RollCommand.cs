using System;
using CommandSystem;
using Exiled.API.Features;
using InventorySystem.Items.Usables.Scp330;
using RemoteAdmin;

namespace ClientCommands.Commands
{
    /// <summary>
    /// .roll — Rolls a random number from 1–6 and broadcasts the result to the player.
    /// Rolling a 6 optionally grants one random SCP:SL candy (controlled by config).
    /// </summary>
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class RollCommand : ICommand
    {
        // ── ICommand ──────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public string Command { get; } = "roll";

        /// <inheritdoc/>
        public string[] Aliases { get; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string Description { get; } = "Rolls a random number from 1 to 6. Roll a 6 to win a candy!";

        // ── Shared RNG (thread-safe enough for game use) ──────────────────────

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

            // Roll 1–6 inclusive.
            int roll = Rng.Next(1, 7);

            if (roll == 6)
            {
                // ── Jackpot! ──────────────────────────────────────────────────

                CandyKindID candy = EventHandlers.PickRandomCandy();
                string candyName  = EventHandlers.GetCandyName(candy);

                // Give candy if the feature is enabled in config.
                if (Plugin.Instance.Config.Candy.IsEnabled && Plugin.Instance.Config.Candy.GiveOnRollSix)
                    EventHandlers.GiveCandyToPlayer(player, candy);

                player.Broadcast(
                    duration: 5,
                    message: $"🎲 You rolled a 6!\n🍬 Lucky! You received <color=yellow>{candyName}</color>",
                    type: Broadcast.BroadcastFlags.Normal,
                    shouldClearPrevious: false);

                response = $"🎲 You rolled a 6! 🍬 Lucky! You received {candyName}";
            }
            else
            {
                // ── Regular roll ──────────────────────────────────────────────

                player.Broadcast(
                    duration: 4,
                    message: $"🎲 You rolled a {roll}!",
                    type: Broadcast.BroadcastFlags.Normal,
                    shouldClearPrevious: false);

                response = $"🎲 You rolled a {roll}!";
            }

            return true;
        }
    }
}
