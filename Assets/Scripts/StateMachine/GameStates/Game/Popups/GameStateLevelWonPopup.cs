using System;



//to do: should be named level end because it's used both for win or lose
public class GameStateLevelWonPopup : GameState
{
	private GameWonPopup _gameWonPopup;
	private readonly string _descriptionText;
	private readonly string _buttonText;
	private readonly string _buttonId;
	private readonly Action _onAction;

	private readonly bool _showLose = false;

	public GameStateLevelWonPopup(string descriptionText, string buttonId, string buttonText, Action onAction = null, bool showLose = false)
	{
		_buttonId = buttonId;
		_descriptionText = descriptionText;
		_buttonText = buttonText;
		_onAction = onAction;
		_showLose = showLose;
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
		_gameWonPopup.loseImage.SetActive(_showLose);
        _gameWonPopup.winImage.SetActive(!_showLose);
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
