using System.Collections;
using UnityEngine;

public class GameStateMetamaskTransaction : GameState
{
    private GameScreenMetamaskTransaction _gameScreenMetamaskTransaction;
    
    public GameStateMetamaskTransaction(int bundleId)
    {
        StoreManager.Instance.BuyBundle(bundleId, TransactionSuccess, TransactionFail);
    }

    private void TransactionSuccess()
    {
        _gameScreenMetamaskTransaction.SetSuccess();
        _gameScreenMetamaskTransaction.StartCoroutine(ClosePopup());
    }

    private void TransactionFail(string reason)
    {
        _gameScreenMetamaskTransaction.SetFail(reason);
        _gameScreenMetamaskTransaction.StartCoroutine(ClosePopup());
    }

    private IEnumerator ClosePopup()
    {
        yield return new WaitForSeconds(2f);
        UserManager.Instance.GetPlayerResourcesAfter(3);
        UserManager.Instance.GetPlayerResourcesAfter(8);
        UserManager.Instance.GetPlayerResourcesAfter(12);
        stateMachine.PushState(new GameStateHome());
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
