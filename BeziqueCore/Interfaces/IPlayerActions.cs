namespace BeziqueCore.Models
{
    public class Card
    {
        public Suit Suit { get; set; }
        public Rank Rank { get; set; }
        public bool IsJoker { get; set; }
    }

    public enum Suit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    public enum Rank
    {
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Jack = 10,
        Queen = 11,
        King = 12,
        Ten = 13,
        Ace = 14
    }
}

namespace BeziqueCore.Interfaces
{
    using BeziqueCore.Models;

    public interface IPlayerActions
    {
        void PlayCard(Player player, Card card);
        void DeclareMeld(Player player, Meld meld);
        void DrawCard(Player player);
        void SwitchSevenOfTrump(Player player);
        void SkipMeld(Player player);
    }

    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public List<Card> Hand { get; set; }
        public bool IsDealer { get; set; }
    }

    public class Meld
    {
        public MeldType Type { get; set; }
        public List<Card> Cards { get; set; }
        public int Points { get; set; }
    }

    public enum MeldType
    {
        TrumpRun,
        TrumpSeven,
        TrumpMarriage,
        Marriage,
        FourAces,
        FourKings,
        FourQueens,
        FourJacks,
        Bezigue,
        DoubleBezigue,
        InvalidMeld
    }
}
