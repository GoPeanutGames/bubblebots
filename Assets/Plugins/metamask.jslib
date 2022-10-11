mergeInto(LibraryManager.library, {
	Login: async function()
	{
		const accounts = await ethereum.request({ method: 'eth_requestAccounts' });
		
		if(accounts.length > 0)
		{
			myGameInstance.SendMessage("Canvas/PnlUI", "InitSession", accounts[0]);
		} else {
			console.log("No wallet has been connected");
		}
	},

	LoginWithMetamask: function()
	{
		//LoginWithMetamask();
	},
});