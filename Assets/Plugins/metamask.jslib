mergeInto(LibraryManager.library, {
	Login: async function()
	{
		const accounts = await ethereum.request({ method: 'eth_requestAccounts' });
		
		if(accounts.length > 0)
		{
			myGameInstance.SendMessage("Canvas/PnlUI", "NotifyMetamaskSuccess");
			myGameInstance.SendMessage("Canvas/PnlUI", "InitSession", accounts[0]);
		} else {
			console.log("No wallet has been connected");
		}
	},

	LoginWithMetamask: function()
	{
		//LoginWithMetamask();
	},
	
	DisplayDebug: function()
	{
		myGameInstance.SendMessage("Canvas/PnlGame", "DisplayDebug");
	},
	
	DisplayTiles: function()
	{
		myGameInstance.SendMessage("Canvas/PnlGame", "DebugTileList");
	},
});