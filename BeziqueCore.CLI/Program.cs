using BeziqueCore.CLI;

var beziqueGame = new BeziqueGame();
var concreateBezicGame = new ConcreateBezicGame();
beziqueGame.addapter = concreateBezicGame;


beziqueGame.Start();
while (true)
{
    Console.ReadLine();
    beziqueGame.DispatchEvent(BeziqueGame.EventId.DO);
    Console.WriteLine(beziqueGame.stateId);
}
