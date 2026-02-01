using System.Diagnostics;

namespace GrpcService.Services;

/*
 Joker, K, Q, J, 10, 9, 8, 7, A
 Hearts, Spades, Clubs, Diamond
*/

static class CardChecker
{
    private const int RanksPerSuit = 8;
    private const int SuitsPerDeck = 4;

    public static int GetNumber(int suit, int rank) => suit * SuitsPerDeck + rank; // Convert suit/rank to card number

    public static string GetRank(this int number)
    {
        var rank = number % RanksPerSuit;
        return rank switch
        {
            0 => "A",
            1 => "7",
            2 => "8",
            3 => "9",
            4 => "10",
            5 => "J",
            6 => "Q",
            7 => "K",
            8 => "Joker",
            _ => throw new ArgumentOutOfRangeException(nameof(number), $"Invalid rank: {rank}")
        };
    }

    public static string GetSuit(this int number)
    {
        var suit = number / RanksPerSuit % SuitsPerDeck;
        return suit switch
        {
            0 => "Hearts",
            1 => "Spades",
            2 => "Clubs",
            3 => "Diamond",
            _ => throw new ArgumentOutOfRangeException(nameof(number), $"Invalid suit: {suit}")
        };
    }

    public static string ToString(this int number) => $"{number.GetRank()} of {number.GetSuit()}";
}