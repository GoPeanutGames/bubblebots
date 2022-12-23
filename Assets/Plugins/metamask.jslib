mergeInto(LibraryManager.library, {
	Login: async function(isDev)
	{
	    const currentEnv = isDev ? env.dev : env.prod

        let chainId = await ethereum.request({ method: 'eth_chainId' });

        chainId = parseInt(chainId);

        if (chainId !== currentEnv.chainId) {
          await window.ethereum.request({
            method: "wallet_addEthereumChain",
            params: [
              {
                chainId: ethers.utils.hexValue(currentEnv.chainId),
                ...currentEnv.connectionConfig,
              },
            ],
          });
        }

        if (chainId === currentEnv.chainId) {
            const accounts = await ethereum.request({ method: 'eth_requestAccounts' });

            if(accounts.length > 0)
            {
                 myGameInstance.SendMessage("Managers/JSLibConnectionManager", "MetamaskLoginSuccess", accounts[0]);
            }  else {
                console.log("No wallet has been connected");
            }
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
