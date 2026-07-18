using System.ComponentModel;
using Exiled.API.Interfaces;

namespace ClientCommands
{
    /// <summary>
    /// Root configuration for the ClientCommands plugin.
    /// Loaded from and saved to the EXILED config file automatically.
    /// </summary>
    public sealed class Config : IConfig
    {
        // ── IConfig ────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        [Description("Whether the plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        /// <inheritdoc/>
        [Description("Whether debug messages are printed to the server console.")]
        public bool Debug { get; set; } = false;

        // ── Sub-configs ────────────────────────────────────────────────────────

        /// <summary>Candy auto-distribution settings.</summary>
        [Description("Settings for the automatic candy distribution feature.")]
        public CandyConfig Candy { get; set; } = new CandyConfig();

        /// <summary>Settings for the .msg command.</summary>
        [Description("Settings for the .msg client command.")]
        public MessageConfig Message { get; set; } = new MessageConfig();
    }

    // ── Sub-config classes ─────────────────────────────────────────────────────

    /// <summary>
    /// Controls automatic candy handouts and roll-six candy rewards.
    /// </summary>
    public sealed class CandyConfig
    {
        /// <summary>Master switch for all candy-related features.</summary>
        [Description("Enable automatic candy distribution and roll-six candy rewards.")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>Seconds between each automatic candy handout (default: 300).</summary>
        [Description("How often (in seconds) each alive player receives a free candy.")]
        public float Interval { get; set; } = 300f;

        /// <summary>Give a candy when a player rolls a six.</summary>
        [Description("Give a random candy to a player who rolls a 6 with .roll.")]
        public bool GiveOnRollSix { get; set; } = true;
    }

    /// <summary>
    /// Controls the .msg overhead-text command.
    /// </summary>
    public sealed class MessageConfig
    {
        /// <summary>How many seconds the message stays above the player's head.</summary>
        [Description("Duration (in seconds) that a .msg message is displayed above the sender's head.")]
        public float Duration { get; set; } = 6f;

        /// <summary>Per-player cooldown in seconds.</summary>
        [Description("Cooldown (in seconds) between .msg uses for a single player.")]
        public float Cooldown { get; set; } = 10f;

        /// <summary>Maximum allowed character length for a message.</summary>
        [Description("Maximum character length allowed for a .msg message (rich-text tags are stripped before checking).")]
        public int MaxLength { get; set; } = 60;
    }
}
