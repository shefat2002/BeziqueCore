namespace BeziqueCore;

public static class TrickEvaluator
{
    public static int GetWinner(Card[] played, Suit trump)
    {
        if (played.Length == 0) return -1;

        var leadCard = played[0];
        var leadSuit = leadCard.IsJoker ? Suit.None : leadCard.Suit;
        var leadIsJoker = leadCard.IsJoker;

        int winnerIndex = 0;
        Card winningCard = leadCard;
        bool currentWinnerWasLeadJoker = leadIsJoker;

        for (int i = 1; i < played.Length; i++)
        {
            var challenger = played[i];

            if (ChallengerBeatsWinner(winningCard, challenger, leadSuit, trump, currentWinnerWasLeadJoker))
            {
                winnerIndex = i;
                winningCard = challenger;
                currentWinnerWasLeadJoker = false;
            }
        }

        return winnerIndex;
    }

    public static int GetWinner(IReadOnlyList<Card> played, Suit trump)
    {
        if (played.Count == 0) return -1;

        var leadCard = played[0];
        var leadSuit = leadCard.IsJoker ? Suit.None : leadCard.Suit;
        var leadIsJoker = leadCard.IsJoker;

        int winnerIndex = 0;
        Card winningCard = leadCard;
        bool currentWinnerWasLeadJoker = leadIsJoker;

        for (int i = 1; i < played.Count; i++)
        {
            var challenger = played[i];

            if (ChallengerBeatsWinner(winningCard, challenger, leadSuit, trump, currentWinnerWasLeadJoker))
            {
                winnerIndex = i;
                winningCard = challenger;
                currentWinnerWasLeadJoker = false;
            }
        }

        return winnerIndex;
    }

    private static bool ChallengerBeatsWinner(Card winner, Card challenger, Suit leadSuit, Suit trump, bool winnerWasLeadJoker)
    {
        bool challengerIsJoker = challenger.IsJoker;
        bool winnerIsJoker = winner.IsJoker;

        if (challengerIsJoker)
        {
            if (winnerWasLeadJoker) return false;
            if (winner.Suit == trump) return false;
            return false;
        }

        if (winnerIsJoker)
        {
            if (challenger.Suit == trump) return true;
            return false;
        }

        bool challengerIsTrump = challenger.Suit == trump;
        bool winnerIsTrump = winner.Suit == trump;

        if (challengerIsTrump && !winnerIsTrump) return true;
        if (!challengerIsTrump && winnerIsTrump) return false;
        if (challengerIsTrump && winnerIsTrump) return challenger.Rank > winner.Rank;

        bool challengerIsLeadSuit = challenger.Suit == leadSuit;
        bool winnerIsLeadSuit = winner.Suit == leadSuit;

        if (challengerIsLeadSuit && winnerIsLeadSuit) return challenger.Rank > winner.Rank;
        if (challengerIsLeadSuit && !winnerIsLeadSuit) return true;

        return false;
    }
}
