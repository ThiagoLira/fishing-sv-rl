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


        /// <summary>
        /// Check if the player is waiting for the caught fish popup to go away
        /// </summary>
        /// <param name="currentTool">A FishingRod that is being used to fish</param>
        /// <returns>true if the mod should click the popup away</returns>
        private bool ShouldDoDismissCaughtPopup(FishingRod fr)
        {
            return (!Context.CanPlayerMove) && fr.fishCaught && fr.inUse() && !fr.isCasting && !fr.isFishing &&
                   !fr.isReeling && !fr.isTimingCast && !fr.pullingOutOfWater && !fr.showingTreasure;
        }




        /// <summary>
        /// Test whether the mod can automatically cast the fishing rod for the user.
        /// </summary>
        /// <param name="currentTool">A FishingRod that is being used to fish</param>
        /// <returns>true if the mod should cast for the user</returns>
        private bool ShouldDoAutoCast(FishingRod fr)
        {
            return Context.CanPlayerMove && Game1.activeClickableMenu is null && !fr.castedButBobberStillInAir &&
                   !fr.hit && !fr.inUse() && !fr.isCasting && !fr.isFishing && !fr.isNibbling && !fr.isReeling &&
                   !fr.isTimingCast && !fr.pullingOutOfWater;
        }


        public bool autoCastRod = true;



   
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>

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


            if (!(Game1.player.CurrentTool is FishingRod))
                return;


            FishingRod rod = Game1.player.CurrentTool as FishingRod;


            if (player == null || !player.IsLocalPlayer)
            {
                return;
            }




            if( autoCastRod & ShouldDoAutoCast(rod) )
            {
               rod.beginUsing(Game1.currentLocation,
                                Game1.player.getStandingX(),
                                Game1.player.getStandingY(),
                                Game1.player);
            }

            if (!rod.isNibbling && rod.isFishing && !rod.isReeling && !rod.pullingOutOfWater && !rod.hit)
            {
                rod.timeUntilFishingBite = 0;
            }

            if (rod.isNibbling && rod.isFishing && !rod.isReeling && !rod.pullingOutOfWater && !rod.hit)
            {
                Farmer.useTool(player);
            }

        

            if (Game1.activeClickableMenu is BobberBar bar)
            {
                float bobberBarPos = Helper.Reflection.GetField<float>(bar, "bobberBarPos").GetValue();
                int bobberBarHeight = Helper.Reflection.GetField<int>(bar, "bobberBarHeight").GetValue();
                float bobberBarSpeed =  Helper.Reflection.GetField<float>(bar, "bobberBarSpeed").GetValue();
                float bobberPosition = Helper.Reflection.GetField<float>(bar, "bobberPosition").GetValue();

                // this can be used to calculate the reward each tick
                float distanceFromCatching = Helper.Reflection.GetField<float>(bar, "distanceFromCatching").GetValue();

                this.Monitor.Log($"{Game1.player.Name} fishing state: {bobberBarPos},{bobberBarHeight}.", LogLevel.Debug);
            }
        }










    }
}