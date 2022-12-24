mergeInto(LibraryManager.library, {
	Login: async function(isDev)
	{
	    try {
	        const userAddress = await metamaskLogin(isDev);
	        if( userAddress ) {
	            myGameInstance.SendMessage("Managers/JSLibConnectionManager", "MetamaskLoginSuccess", userAddress);
	        }
	        else{
	            console.log('Error Connecting, show modal')
	        }
	    }
	    catch(e) {
	        console.log('Error Connecting, show modal for catch')
	    }
	},

	RequestSignature: async function (schema, account) {

        account = UTF8ToString(account);
        schema = UTF8ToString(schema);
        console.log(account, schema);

        const signature = await ethereum.request({
            method: "eth_signTypedData_v4",
            params: [account, schema],
            from: account,
        });

        myGameInstance.SendMessage(
            "Managers/JSLibConnectionManager",
            "SignatureLoginSuccess",
            signature
        );
    },

    BuyBundle: async function(bundleId, isDev){
        console.log(bundleId);
        console.log(isDev);

        try{
            const success = await window.metamaskBundleBuying(bundleId, isDev);
            if(success){
                myGameInstance.SendMessage("Managers/JSLibConnectionManager","BundleBuySuccess");
            }else{
                myGameInstance.SendMessage("Managers/JSLibConnectionManager","BundleBuyFailBalance");
            }
        }catch(error){
            myGameInstance.SendMessage("Managers/JSLibConnectionManager","BundleBuyFail");
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
