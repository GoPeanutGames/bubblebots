using UnityEngine;

public class GameStateConfirmTransaction : GameState
{
    private GameScreenConfirmTransaction _gameScreenConfirmTransaction;

    private int _bundleId;
    
    public GameStateConfirmTransaction(int bundleId)
    {
        _bundleId = bundleId;
        _gameScreenConfirmTransaction = Screens.Instance.PushScreen<GameScreenConfirmTransaction>();
        _gameScreenConfirmTransaction.SetLoading();
        StoreManager.Instance.GetBundleFromId(bundleId, (bundle) =>
        {
            _gameScreenConfirmTransaction.SetBundleData(bundle);
            _gameScreenConfirmTransaction.RemoveLoading();
        });
    }

    public override void Enable()
    {
        GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
    }

    public override string GetGameStateName()
    {
        return "Game state confirm transaction";
    }

    private void OnGameEvent(GameEventData data)
    {
        if (data.eventName == GameEvents.ButtonTap)
        {
            OnButtonTap(data);
        }
    }

    private void OnButtonTap(GameEventData data)
    {
        GameEventString customButtonData = data as GameEventString;
        switch (customButtonData.stringData)
        {
            case ButtonId.ConfirmTransactionClose:
                stateMachine.PopState();
                break;
            case ButtonId.ConfirmTransactionBuy:
                Debug.Log($"{nameof(GameStateConfirmTransaction)}::{nameof(OnButtonTap)} - {ButtonId.ConfirmTransactionBuy}");
                Screens.Instance.PopScreen<GameScreenStore>();
                stateMachine.PushState(new GameStateMetamaskTransaction(_bundleId));
                break;
            default:
                break;
        }
    }

    public override void Disable()
    {
        GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
        Screens.Instance.PopScreen(_gameScreenConfirmTransaction);
    }
}