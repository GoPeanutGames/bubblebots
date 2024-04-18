mergeInto(LibraryManager.library, {
  Login: async function (isDev) {
    try {
      const userAddress = await metamaskLogin(isDev);
      if (userAddress) {
        myGameInstance.SendMessage(
          "Managers/JSLibConnectionManager",
          "MetamaskLoginSuccess",
          userAddress
        );
      } else {
        console.log("Error Connecting, show modal");
      }
    } catch (e) {
      console.log("Error Connecting, show modal for catch");
    }
  },
  
  RequestWalletAdress_test: async function () {
	try {
        myGameInstance.SendMessage(
          "Managers/JSLibConnectionManager",
          "RequestWalletAdressSuccess",
          "test_adress"
        );
    } catch (e) {
      console.log("Something went wrong when getting wallet adress");
    }
  },
  
    RequestSignature_test: async function () {
	try {
        myGameInstance.SendMessage(
          "Managers/JSLibConnectionManager",
          "RequestSignatureSuccess",
          "test_signature"
        );
    } catch (e) {
      console.log("Something went wrong when getting signature");
    }
  },

  RequestSignature: async function (schema, account) {
    account = UTF8ToString(account);
    schema = UTF8ToString(schema);

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

  BuyBundle: async function (bundleId, isDev) {
    try {
      const success = await window.metamaskBundleBuying(bundleId, isDev);
      if (success) {
        myGameInstance.SendMessage(
          "Managers/JSLibConnectionManager",
          "BundleBuySuccess"
        );
      } else {
        myGameInstance.SendMessage(
          "Managers/JSLibConnectionManager",
          "BundleBuyFailBalance"
        );
      }
    } catch (error) {
      myGameInstance.SendMessage(
        "Managers/JSLibConnectionManager",
        "BundleBuyFail"
      );
    }
  }
});
