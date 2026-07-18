using System;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using PlayerEvents = Exiled.Events.Handlers.Player;
using ServerEvents = Exiled.Events.Handlers.Server;

namespace ClientCommands
{
    /// <summary>
    /// Entry point for the ClientCommands EXILED plugin.
    /// Manages lifecycle: registration / unregistration of event handlers and coroutines.
    /// </summary>
    public sealed class Plugin : Plugin<Config>
    {
        // ── Singleton ─────────────────────────────────────────────────────────

        /// <summary>Singleton instance, set in <see cref="OnEnabled"/>.</summary>
        public static Plugin Instance { get; private set; } = null!;

        // ── Plugin metadata ───────────────────────────────────────────────────

        /// <inheritdoc/>
        public override string Name        => "ClientCommands";

        /// <inheritdoc/>
        public override string Author      => "thedutchpianoman_";

        /// <inheritdoc/>
        public override string Prefix      => "cc";

        /// <inheritdoc/>
        public override Version Version    => new Version(1, 0, 0);

        /// <inheritdoc/>
        public override Version RequiredExiledVersion => new Version(8, 0, 0);

        // ── Internal state ────────────────────────────────────────────────────

        /// <summary>
        /// Active event-handler instance. Internal so command classes can access
        /// shared state (e.g. the .msg cooldown map) without reflection.
        /// </summary>
        internal EventHandlers? Handlers { get; private set; }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        /// <summary>
        /// Called by EXILED when the plugin is loaded and enabled.
        /// Registers all event subscriptions and game-console commands.
        /// </summary>
        public override void OnEnabled()
        {
            Instance = this;
            Handlers = new EventHandlers(Config);

            // Subscribe to server / player events
            ServerEvents.RoundStarted += Handlers.OnRoundStarted;
            ServerEvents.RoundEnded   += Handlers.OnRoundEnded;
            PlayerEvents.Left         += Handlers.OnPlayerLeft;

            Log.Info($"{Name} v{1.0.0} by {thedutchpianoman_} has been enabled.");
            base.OnEnabled();
        }

        /// <summary>
        /// Called by EXILED when the plugin is disabled or the server shuts down.
        /// Unregisters all event subscriptions and cleans up active coroutines.
        /// </summary>
        public override void OnDisabled()
        {
            if (Handlers is not null)
            {
                ServerEvents.RoundStarted -= Handlers.OnRoundStarted;
                ServerEvents.RoundEnded   -= Handlers.OnRoundEnded;
                PlayerEvents.Left         -= Handlers.OnPlayerLeft;

                Handlers.KillCandyCoroutine();
                Handlers = null;
            }

            Instance = null!;
            Log.Info($"{Name} has been disabled.");
            base.OnDisabled();
        }
    }
}
