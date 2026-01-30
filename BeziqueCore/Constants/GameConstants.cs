using BeziqueCore.Models;
using BeziqueCore.Interfaces;

namespace BeziqueCore.Constants
{
    /// <summary>
    /// Centralized game constants for Bezique.
    /// Provides single source of truth for all game rules and scoring values.
    /// </summary>
    public static class GameConstants
    {
        // Card dealing
        public const int CardsPerPlayer = 9;
        public const int DealSetSize = 3;

        // Scoring - Melds
        public const int BeziquePoints = 40;
        public const int DoubleBeziquePoints = 500;
        public const int TrumpRunPoints = 250;
        public const int TrumpMarriagePoints = 40;
        public const int MarriagePoints = 20;
        public const int SevenOfTrumpBonus = 10;
        public const int FourAcesPoints = 100;
        public const int FourKingsPoints = 80;
        public const int FourQueensPoints = 60;
        public const int FourJacksPoints = 40;

        // Scoring - Trick bonuses
        public const int LastTrickBonusPoints = 10;
        public const int TimeoutPenalty = 10;

        // Advanced Mode bonuses
        public const int AceAndTenBonusPoints = 10;

        // Card counts
        public const int TotalCardsInDeck = 64; // 52 standard + 12 jokers (6 of each color)
        public const int LastNineCardsThreshold = 9;

        // Game thresholds
        public const int WinningScore = 1000;
    }

    /// <summary>
    /// Meld point mappings for quick lookup using array-based access for O(1) performance.
    /// </summary>
    public static class MeldPoints
    {
        private static readonly int[] _pointLookup;

        static MeldPoints()
        {
            _pointLookup = new int[(int)MeldType.InvalidMeld + 1];
            _pointLookup[(int)MeldType.Bezique] = GameConstants.BeziquePoints;
            _pointLookup[(int)MeldType.DoubleBezique] = GameConstants.DoubleBeziquePoints;
            _pointLookup[(int)MeldType.TrumpRun] = GameConstants.TrumpRunPoints;
            _pointLookup[(int)MeldType.TrumpMarriage] = GameConstants.TrumpMarriagePoints;
            _pointLookup[(int)MeldType.Marriage] = GameConstants.MarriagePoints;
            _pointLookup[(int)MeldType.TrumpSeven] = GameConstants.SevenOfTrumpBonus;
            _pointLookup[(int)MeldType.FourAces] = GameConstants.FourAcesPoints;
            _pointLookup[(int)MeldType.FourKings] = GameConstants.FourKingsPoints;
            _pointLookup[(int)MeldType.FourQueens] = GameConstants.FourQueensPoints;
            _pointLookup[(int)MeldType.FourJacks] = GameConstants.FourJacksPoints;
        }

        /// <summary>
        /// Gets points for a meld type using O(1) array lookup.
        /// </summary>
        public static int GetPointsForType(MeldType type)
        {
            int index = (int)type;
            return index >= 0 && index < _pointLookup.Length ? _pointLookup[index] : 0;
        }
    }

    /// <summary>
    /// Rank values for comparison and sorting using array-based access for O(1) performance.
    /// </summary>
    public static class RankValues
    {
        // Pre-computed array for O(1) lookup instead of switch expression
        private static readonly int[] _valueLookup;
        private static readonly int[] _trickPointsLookup;

        static RankValues()
        {
            const int maxRank = (int)Rank.Ace;
            _valueLookup = new int[maxRank + 1];
            _trickPointsLookup = new int[maxRank + 1];

            _valueLookup[(int)Rank.Seven] = 7;
            _valueLookup[(int)Rank.Eight] = 8;
            _valueLookup[(int)Rank.Nine] = 9;
            _valueLookup[(int)Rank.Jack] = 10;
            _valueLookup[(int)Rank.Queen] = 11;
            _valueLookup[(int)Rank.King] = 12;
            _valueLookup[(int)Rank.Ten] = 13;
            _valueLookup[(int)Rank.Ace] = 14;

            _trickPointsLookup[(int)Rank.Ace] = 11;
            _trickPointsLookup[(int)Rank.Ten] = 10;
        }

        /// <summary>
        /// Gets rank value using O(1) array lookup.
        /// </summary>
        public static int GetValue(Rank rank)
        {
            int index = (int)rank;
            return index >= 0 && index < _valueLookup.Length ? _valueLookup[index] : 0;
        }

        /// <summary>
        /// Gets trick points for a rank using O(1) array lookup.
        /// </summary>
        public static int GetTrickPoints(Rank rank)
        {
            int index = (int)rank;
            return index >= 0 && index < _trickPointsLookup.Length ? _trickPointsLookup[index] : 0;
        }
    }
}
