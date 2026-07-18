using System;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using InventorySystem.Items.ThrowableProjectiles;
using RemoteAdmin;

namespace ClientCommands.Commands
{
    /// <summary>
    /// .suicide — Alias for <see cref="KillCommand"/>.
    /// Kills the sender with a visual-only explosion effect; nearby players are unharmed.
    /// </summary>
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class SuicideCommand : ICommand
    {
        // ── ICommand ──────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public string Command { get; } = "suicide";

        /// <inheritdoc/>
        public string[] Aliases { get; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string Description { get; } = "Alternative to .kill — kills yourself with a visual explosion.";

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

            if (!player.IsAlive)
            {
                response = "You are already dead.";
                return false;
            }

            // Visual-only explosion particle — no area damage applied.
            ExplosionUtils.ServerSpawnEffect(player.Position, ParticleEffectIntensity.Large);

            // Kill the sending player.
            player.Kill(DamageType.Explosion);

            response = "💥 You killed yourself.";
            return true;
        }
    }
}
