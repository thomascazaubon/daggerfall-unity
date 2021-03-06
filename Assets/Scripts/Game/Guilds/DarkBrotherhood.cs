// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2018 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Hazelnut
// Contributors:    

using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Entity;
using System;
using DaggerfallWorkshop.Utility;

namespace DaggerfallWorkshop.Game.Guilds
{
    public class DarkBrotherhood : Guild
    {
        #region Constants

        public const string InitiationQuestName = "L0A01L00";

        protected const int WelcomeMsgId = 5292;    // Not used AFAIK
        protected const int PromotionMsgId = 666;   // How appropriate!
        protected const int PromotionBuyPotionsId = 6611;
        protected const int PromotionMakePotionsId = 6612;
        protected const int PromotionSoulGemsId = 6613;
        protected const int PromotionSpymasterId = 6614;
        protected const int BribesJudgeId = 551;

        private const int factionId = 108;

        private DFLocation revealedDungeon;

        #endregion

        #region Properties & Data

        static string[] rankTitles = new string[] {
                "Apprentice", "Journeyman", "Operator", "Slayer", "Executioner", "Punisher", "Terminator", "Assassin", "Dark Brother", "Master Assassin"
        };
        
        static List<DFCareer.Skills> guildSkills = new List<DFCareer.Skills>() {
                DFCareer.Skills.Archery,
                DFCareer.Skills.Backstabbing,
                DFCareer.Skills.Climbing,
                DFCareer.Skills.CriticalStrike,
                DFCareer.Skills.Daedric,
                DFCareer.Skills.Destruction,
                DFCareer.Skills.ShortBlade,
                DFCareer.Skills.Stealth,
                DFCareer.Skills.Streetwise,
            };

        static List<DFCareer.Skills> trainingSkills = new List<DFCareer.Skills>() {
                DFCareer.Skills.Archery,
                DFCareer.Skills.Backstabbing,
                DFCareer.Skills.Climbing,
                DFCareer.Skills.CriticalStrike,
                DFCareer.Skills.Daedric,
                DFCareer.Skills.Destruction,
                DFCareer.Skills.Dodging,
                DFCareer.Skills.Running,
                DFCareer.Skills.ShortBlade,
                DFCareer.Skills.Stealth,
                DFCareer.Skills.Streetwise,
                DFCareer.Skills.Swimming
            };

        public override string[] RankTitles { get { return rankTitles; } }

        public override List<DFCareer.Skills> GuildSkills { get { return guildSkills; } }

        public override List<DFCareer.Skills> TrainingSkills { get { return trainingSkills; } }

        #endregion

        #region Guild Membership and Faction

        public DarkBrotherhood()
        {
            // Register for location entry events so can auto discover guild houses.
            PlayerGPS.OnEnterLocationRect += PlayerGPS_OnEnterLocationRect;
        }

        public static int FactionId { get { return factionId; } }

        public override int GetFactionId()
        {
            return factionId;
        }

        public override string GetTitle()
        {
            if (GameManager.Instance.PlayerEntity.Gender == Genders.Female)
                if (rank == 8)
                    return "Dark Sister";        // Not calling female chars 'Brother'!

            return IsMember() ? rankTitles[rank] : "non-member";
        }

        #endregion

        #region Guild Ranks

        public override TextFile.Token[] TokensPromotion(int newRank)
        {
            return DaggerfallUnity.Instance.TextProvider.GetRandomTokens(GetPromotionMsgId(newRank));
        }

        private int GetPromotionMsgId(int rank)
        {
            revealedDungeon = GameManager.Instance.PlayerGPS.DiscoverRandomLocation();
            switch (rank)
            {
                case 1:
                    return PromotionBuyPotionsId;
                case 3:
                    return PromotionMakePotionsId;
                case 5:
                    return PromotionSoulGemsId;
                case 7:
                    return PromotionSpymasterId;
                default:
                    return PromotionMsgId;
            }
        }

        protected override int CalculateNewRank(PlayerEntity playerEntity)
        {
            // Dark Brotherhood never expel members (I assume at some point they 'retire' you instead!)
            int newRank = base.CalculateNewRank(playerEntity);
            return (newRank < 0) ? 0 : newRank;
        }

        #endregion

        #region Benefits

        // TESTING ONLY - REMOVE!
        public override bool CanRest()
        {
            return IsMember();
        }

        public override bool HallAccessAnytime()
        {
            return IsMember();
        }

        #endregion

        #region Service Access:

        public override bool CanAccessService(GuildServices service)
        {
            switch (service)
            {
                case GuildServices.Training:
                    return IsMember();
                case GuildServices.Quests:
                    return true;
                case GuildServices.BuyPotions:
                    return (rank >= 1);
                case GuildServices.MakePotions:
                    return (rank >= 3);
                case GuildServices.BuySoulgems:
                    return (rank >= 5);
                case GuildServices.Spymaster:
                    return (rank >= 7);

                default:
                    return false;
            }
        }

        #endregion

        #region Joining

        public override TextFile.Token[] TokensIneligible(PlayerEntity playerEntity)
        {
            throw new NotImplementedException();
        }
        public override TextFile.Token[] TokensEligible(PlayerEntity playerEntity)
        {
            throw new NotImplementedException();
        }
        public override TextFile.Token[] TokensWelcome()
        {
            return DaggerfallUnity.Instance.TextProvider.GetRSCTokens(WelcomeMsgId);
        }

        #endregion

        #region Event handlers

        private void PlayerGPS_OnEnterLocationRect(DFLocation location)
        {
            BuildingDirectory buildingDirectory = GameManager.Instance.StreamingWorld.GetCurrentBuildingDirectory();
            if (buildingDirectory)
                foreach (BuildingSummary building in buildingDirectory.GetBuildingsOfFaction(factionId))
                    GameManager.Instance.PlayerGPS.DiscoverBuilding(building.buildingKey, GetGuildName());
        }

        #endregion


        #region Macro Handling

        public override MacroDataSource GetMacroDataSource()
        {
            return new DarkBrotherhoodMacroDataSource(this);
        }

        /// <summary>
        /// MacroDataSource context sensitive methods for Dark Brotherhood.
        /// </summary>
        protected class DarkBrotherhoodMacroDataSource : GuildMacroDataSource
        {
            private DarkBrotherhood parent;
            public DarkBrotherhoodMacroDataSource(DarkBrotherhood guild) : base(guild)
            {
                parent = guild;
            }

            public override string Dungeon()
            {
                return parent.revealedDungeon.Name;
            }
        }

        #endregion
    }

}