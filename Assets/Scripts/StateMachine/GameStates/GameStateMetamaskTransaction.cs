using System.Collections;
using UnityEngine;

public class GameStateMetamaskTransaction : GameState
{
    private GameScreenMetamaskTransaction _gameScreenMetamaskTransaction;
    
    public GameStateMetamaskTransaction(int bundleId)
    {
        StoreManager.Instance.GetBundleFromId(bundleId, (data) =>
        {
            bool isDev = EnvironmentManager.Instance.IsDevelopment();
            MetamaskManager.Instance.BuyStoreBundle(bundleId, isDev, TransactionSuccess, TransactionFail);
        });
    }

    private void TransactionSuccess()
    {
        _gameScreenMetamaskTransaction.SetSuccess();
        _gameScreenMetamaskTransaction.StartCoroutine(ClosePopup());
    }

    private void TransactionFail()
    {
        _gameScreenMetamaskTransaction.SetFail();
        _gameScreenMetamaskTransaction.StartCoroutine(ClosePopup());
    }

    private IEnumerator ClosePopup()
    {
        yield return new WaitForSeconds(5f);
        UserManager.Instance.GetPlayerResources();
        stateMachine.PushState(new GameStateMainMenu());
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
