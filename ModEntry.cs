using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley.Tools;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace fishing
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);
        }


        private void OnMenuChanged(object sender, MenuChangedEventArgs args)
        {
            Farmer player = Game1.player;
            if (player == null || !player.IsLocalPlayer)
            {
                return;
            }

            if (args.NewMenu is BobberBar bar)
            {
                
                // No treasures to mess with training!
                Helper.Reflection.GetField<bool>(bar, "treasure").SetValue(false);
                


            }
        }


        private void OnUpdateTicked(object sender, UpdateTickedEventArgs args)
        {
            Farmer player = Game1.player;
            if (player == null || !player.IsLocalPlayer)
            {
                return;
            }

            if (player.CurrentTool is FishingRod rod)
            {

                if (!rod.isNibbling && rod.isFishing && !rod.isReeling && !rod.pullingOutOfWater && !rod.hit)
                {
                    rod.timeUntilFishingBite = 0;
                }

                if (rod.isNibbling && rod.isFishing && !rod.isReeling && !rod.pullingOutOfWater && !rod.hit)
                {
                    Farmer.useTool(player);
                }

            }

            if (Game1.activeClickableMenu is BobberBar bar)
            {
                float bobberBarPos = Helper.Reflection.GetField<float>(bar, "bobberBarPos").GetValue();
                int bobberBarHeight = Helper.Reflection.GetField<int>(bar, "bobberBarHeight").GetValue();


                this.Monitor.Log($"{Game1.player.Name} fishing state: {bobberBarPos},{bobberBarHeight}.", LogLevel.Debug);
            }
        }










    }
}