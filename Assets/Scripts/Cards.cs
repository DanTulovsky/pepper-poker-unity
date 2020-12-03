
using Poker;

public static class Cards {

    // fileForCard returns the filename for the given card
    // This is specific to the PlayingCards plugin we are using.
    public static string FileForCard(Card card) {
        string prefix = "Prefab/BackColor_Red/Red_PlayingCards_";
        string fmt = "00.##";

        // The package starts with ace at the bottom, server has Ace at the top
        int rank = (((int)card.Rank + 1) % 13) + 1;
        string suit = card.Suite.ToString();

        string file = $"{prefix}{suit}{rank.ToString(fmt)}_00";

        return file;
    }

    public static string BlankCard() {
        return "Prefab/BackColor_Red/Red_PlayingCards_Blank_00";
    }
}

