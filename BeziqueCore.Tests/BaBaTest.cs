using BeziqueCore;
using Xunit.Abstractions;

public class BaBaTest
{
    private readonly ITestOutputHelper _output;

    public BaBaTest(ITestOutputHelper output)
    {
        _output = output;

        var baba = new BeziqueConcruite();
        foreach (var card in baba.Dealer)
        {
            _output.WriteLine(card.ToString());
        }
    }

    [Fact]
    public void DeckCardCheck()
    {
        var baba = new BeziqueConcruite();

        // Act
        var cards = baba.Dealer;

    }
}