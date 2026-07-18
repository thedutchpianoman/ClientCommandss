using System;
using CommandSystem;

namespace ClientCommands.Commands
{
    /// <summary>
    /// .help — Lists all client commands provided by this plugin with a short description.
    /// Registered as a GameConsole (client-side) command; invoked with a leading dot: .help
    /// </summary>
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class HelpCommand : ICommand
    {
        // ── ICommand ──────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public string Command { get; } = "help";

        /// <inheritdoc/>
        public string[] Aliases { get; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string Description { get; } = "Shows all available client commands for this plugin.";

        // ── Execute ───────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response =
                "\n=== ClientCommands — Available Commands ===\n" +
                ".help           — Shows this list.\n" +
                ".kill           — Kill yourself with a visual explosion (no area damage).\n" +
                ".suicide        — Alias for .kill.\n" +
                ".roll           — Roll a number from 1–6. Roll a 6 to win a candy!\n" +
                ".coinflip       — Flip a coin: Heads or Tails.\n" +
                ".msg <message>  — Display a message above your head for a few seconds.\n" +
                "                  Max " + Plugin.Instance.Config.Message.MaxLength + " characters · " +
                Plugin.Instance.Config.Message.Cooldown + "s cooldown.\n" +
                "===========================================";

            return true;
        }
    }
}
