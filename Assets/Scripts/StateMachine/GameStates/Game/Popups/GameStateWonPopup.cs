using System;

public class GameStateWonPopup : GameState
{
	private GameWonPopup _gameWonPopup;
	private readonly string _descriptionText;
	private readonly string _buttonText;
	private readonly string _buttonId;
	private readonly Action _onAction;

	public GameStateWonPopup(string descriptionText, string buttonId, string buttonText, Action onAction = null)
	{
		_buttonId = buttonId;
		_descriptionText = descriptionText;
		_buttonText = buttonText;
		_onAction = onAction;
	}
	public override string GetGameStateName()
	{
		return "Game State won";
	}
	
	public override void Enter()
	{
		_gameWonPopup = Screens.Instance.PushScreen<GameWonPopup>();
		_gameWonPopup.SetDescriptionText(_descriptionText);
		_gameWonPopup.SetActionButtonId(_buttonId);
		_gameWonPopup.SetActionButtonText(_buttonText);
		_gameWonPopup.StartOpen();
		Screens.Instance.BringToFront<GameWonPopup>();
	}

	public override void Enable()
	{
		GameEventsManager.Instance.AddGlobalListener(OnGameEvent);
	}

	private void OnGameEvent(GameEventData data)
	{
		if (data.eventName == GameEvents.ButtonTap)
		{
			OnButtonTap(data);
		} 
        else if (data.eventName == GameEvents.UpdateBubblesNumber)
        {
			int bubbles = (data as GameEventInt).intData;

			_gameWonPopup.SetDescriptionText("You earned <color=#FFCB5E>" + bubbles + "</color> Points and extra <color=#FFCB5E>20,000</color> Points for completing all levels!");
        }
    }

	private void OnButtonTap(GameEventData data)
	{
		GameEventString customButtonData = data as GameEventString;
		switch (customButtonData.stringData)
		{
			case ButtonId.LevelCompleteContinue:
				stateMachine.PopState();
				_onAction?.Invoke();
				break;
			case ButtonId.GameEndGoToMainMenu:
				stateMachine.PopAll();
				_onAction?.Invoke();
				break;
		}
	}
	
	public override void Disable()
	{
		GameEventsManager.Instance.RemoveGlobalListener(OnGameEvent);
	}

	public override void Exit()
	{
		_gameWonPopup.StartClose();
	}
}
