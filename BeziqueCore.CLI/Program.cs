var beziqueGame = new BeziqueGame();


beziqueGame.Start();
while (true)
{
    Console.ReadLine();
    beziqueGame.DispatchEvent(BeziqueGame.EventId.DO);
    Console.WriteLine(beziqueGame.stateId);
}
