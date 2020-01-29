using System;
using Microsoft.Xna.Framework;
using DsStardewLib.SMAPI;
using DsStardewLib.Utils;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley.Tools;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

using fishing.HarmonyHacks;
using Microsoft.ML;


namespace fishing
{

    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {


        /*
        float bobberBarPosMax = -10000;
        float bobberBarPosMin = 10000;

        int bobberBarHeightMax = -10000;
        int bobberBarHeightMin = 100000;


        float bobberBarSpeedMax = -10000;
        float bobberBarSpeedMin = 100000;

        float bobberPositionMax = -10000;
        float bobberPositionMin = 100000;
        */


       



        private DsModHelper<ModConfig> modHelper = new DsModHelper<ModConfig>();
        private HarmonyWrapper hWrapper = new HarmonyWrapper();


        private Logger log;
        private ModConfig config;

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

        private RLAgent Agent;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {



                modHelper.Init(helper, this.Monitor);


                log = modHelper.Log;


                Agent = new RLAgent(log);

                config = modHelper.Config;

                log.Silly("Created log and config for mod entry.  Loading Harmony.");
                hWrapper.InitHarmony(helper, config, log);

                log.Silly("Loading event handlers");

                helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
                helper.Events.Player.InventoryChanged += OnInventoryChanged;
                helper.Events.Display.MenuChanged += OnMenuChanged;

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

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs args)
        {
            Farmer player = args.Player;

            foreach (Item item in args.Added)
            {
                player.Items.Remove(item);
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

                // this is the bar, the lower end 
                float bobberBarPos = Helper.Reflection.GetField<float>(bar, "bobberBarPos").GetValue();

                // size of fishing bar
                int bobberBarHeight = Helper.Reflection.GetField<int>(bar, "bobberBarHeight").GetValue();

                // velocity of bar
                float bobberBarSpeed =  Helper.Reflection.GetField<float>(bar, "bobberBarSpeed").GetValue();

                // this is the fish 
                float bobberPosition = Helper.Reflection.GetField<float>(bar, "bobberPosition").GetValue();


                // this can be used to calculate the reward each tick
                float distanceFromCatching = Helper.Reflection.GetField<float>(bar, "distanceFromCatching").GetValue();



                float[] state = { bobberBarPos, bobberBarSpeed, bobberPosition };

                Agent.Update(state);



                /*
                if (bobberBarPos > bobberBarPosMax) { bobberBarPosMax = bobberBarPos; }
                if (bobberBarPos < bobberBarPosMin) { bobberBarPosMin = bobberBarPos; }

                if (bobberBarHeight > bobberBarHeightMax) { bobberBarHeightMax = bobberBarHeight; }
                if (bobberBarHeight < bobberBarHeightMin) { bobberBarHeightMin = bobberBarHeight; }

                if (bobberBarSpeed > bobberBarSpeedMax) { bobberBarSpeedMax = bobberBarSpeed; }
                if (bobberBarSpeed < bobberBarSpeedMin) { bobberBarSpeedMin = bobberBarSpeed; }

                if (bobberPosition > bobberPositionMax) { bobberPositionMax = bobberPosition; }
                if (bobberPosition < bobberPositionMin) { bobberPositionMin = bobberPosition; }

                this.Monitor.Log($"bobberbarpos MAX: {bobberBarPosMax} , MIN: {bobberBarPosMin}", LogLevel.Debug);
                this.Monitor.Log($"bobberBarHeight MAX: {bobberBarHeightMax} , MIN: {bobberBarHeightMin}", LogLevel.Debug);
                this.Monitor.Log($"bobberBarSpeed MAX: {bobberBarSpeedMax} , MIN: {bobberBarSpeedMin}", LogLevel.Debug);
                this.Monitor.Log($"bobberPosition MAX: {bobberPositionMax} , MIN: {bobberPositionMin}", LogLevel.Debug);
                */




            }

            // Click away the catched fish.  The conditionals are ordered here in a way
            // the check for the popup happens before the config check so the code can
            // always check the treasure chest.  See the variable doneCaughtFish for more
            // info.
            if (ShouldDoDismissCaughtPopup(rod))
            {
                log.Trace("Tool is sitting at caught fish popup");
                //doneCaughtFish = true;


                log.Trace("Closing popup with Harmony");
                ClickAtAllHack.simulateClick = true;
                
            }


        }










    }
}