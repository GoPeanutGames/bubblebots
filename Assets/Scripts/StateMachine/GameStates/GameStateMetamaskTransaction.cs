public class GameStateMetamaskTransaction : GameState
{
    private GameScreenMetamaskTransaction _gameScreenMetamaskTransaction;
    
    public GameStateMetamaskTransaction(int bundleId)
    {
        StoreManager.Instance.GetBundleFromId(bundleId, (data) =>
        {
            _gameScreenMetamaskTransaction.SetSuccess();
            stateMachine.PushState(new GameStateMainMenu());
        });
    }

    public override void Enable()
    {
        _gameScreenMetamaskTransaction = Screens.Instance.PushScreen<GameScreenMetamaskTransaction>();
        _gameScreenMetamaskTransaction.SetLoading();
    }

    public override string GetGameStateName()
    {
        return "Game state metamask transaction";
    }

    public override void Disable()
    {
        Screens.Instance.PopScreen(_gameScreenMetamaskTransaction);
    }
}
