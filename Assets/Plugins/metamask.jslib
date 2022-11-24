mergeInto(LibraryManager.library, {
	Login: async function()
	{
		const accounts = await ethereum.request({ method: 'eth_requestAccounts' });
		
		if(accounts.length > 0)
		{
			console.warn(accounts[0]);
			myGameInstance.SendMessage("Managers/WalletManager", "MetamaskLoginSuccess", accounts[0]);
		} else {
			console.log("No wallet has been connected");
		}
	},
	
	DisplayDebug: function()
	{
		myGameInstance.SendMessage("Canvas/PnlGame", "DisplayDebug");
	},
	
	DisplayTiles: function()
	{
		myGameInstance.SendMessage("Canvas/PnlGame", "DebugTileList");
	},
	
	Premint: function()
	{
		window.open("https://www.premint.xyz/peanutgames-bubble-bots-mini-game/");
	},
	
	Airdrop: function()
	{
		window.open("https://peanutgames.com/airdrop");
	},
	
	Reload: function()
	{
		window.location.reload();
	},
	
	DisplayHelp: function()
	{
		window.open("https://www.youtube.com/watch?v=w10rwbbQVr8");
	},
});