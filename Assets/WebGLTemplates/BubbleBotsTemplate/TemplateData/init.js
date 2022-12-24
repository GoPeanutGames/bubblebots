const contractAbi = [
  {
    "inputs": [
      {
        "internalType": "uint256",
        "name": "bundleId",
        "type": "uint256"
      }
    ],
    "name": "purchaseGemsByToken",
    "outputs": [],
    "stateMutability": "nonpayable",
    "type": "function"
  },
  {
    "constant": false,
    "inputs": [
      {
        "name": "guy",
        "type": "address"
      },
      {
        "name": "wad",
        "type": "uint256"
      }
    ],
    "name": "approve",
    "outputs": [
      {
        "name": "",
        "type": "bool"
      }
    ],
    "payable": false,
    "stateMutability": "nonpayable",
    "type": "function"
  },
  {
    "constant": true,
    "inputs": [
      {
        "name": "",
        "type": "address"
      },
      {
        "name": "",
        "type": "address"
      }
    ],
    "name": "allowance",
    "outputs": [
      {
        "name": "",
        "type": "uint256"
      }
    ],
    "payable": false,
    "stateMutability": "view",
    "type": "function"
  },
  {
    "inputs": [
      {
        "internalType": "uint256",
        "name": "bundleId",
        "type": "uint256"
      }
    ],
    "name": "getBundle",
    "outputs": [
      {
        "components": [
          {
            "internalType": "uint256",
            "name": "price",
            "type": "uint256"
          },
          {
            "internalType": "uint256",
            "name": "beforeDiscount",
            "type": "uint256"
          },
          {
            "internalType": "uint256",
            "name": "gems",
            "type": "uint256"
          },
          {
            "internalType": "bool",
            "name": "isActive",
            "type": "bool"
          },
          {
            "internalType": "string",
            "name": "name",
            "type": "string"
          },
          {
            "internalType": "string",
            "name": "description",
            "type": "string"
          }
        ],
        "internalType": "struct Bubbles.Bundle",
        "name": "",
        "type": "tuple"
      }
    ],
    "stateMutability": "view",
    "type": "function"
  },
  {
    "constant": true,
    "inputs": [
      {
        "name": "",
        "type": "address"
      }
    ],
    "name": "balanceOf",
    "outputs": [
      {
        "name": "",
        "type": "uint256"
      }
    ],
    "payable": false,
    "stateMutability": "view",
    "type": "function"
  },
];

const env = {
  dev: {
    chainId: 80001,
    gemsContract: '0xB37F565A4205B6A0dA009262e1d9B3b9d494E166',
    tokenContract: '0xeef2D7cA3d6fD039dF5Eb899378a37F541bDefA0',
    connectionConfig: {
      rpcUrls: ["https://rpc-mumbai.maticvigil.com"],
      chainName: "Matic Mumbai",
      nativeCurrency: {
        name: "MATIC",
        symbol: "MATIC",
        decimals: 18
      },
      blockExplorerUrls: ["https://mumbai.polygonscan.com/"]
    }
  },
  prod: {
    chainId: 137,
    gemsContract: '0xcA0ccb920267C78259CF333959F48D649Be13E34',
    tokenContract: '0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174',
    connectionConfig: {
      rpcUrls: ["https://polygon-rpc.com"],
      chainName: "Polygon Mainnet",
      nativeCurrency: {
        name: "MATIC",
        symbol: "MATIC",
        decimals: 18
      },
      blockExplorerUrls: ["https://polygonscan.com/"]
    }
  }
}

  window.metamaskLogin = async (isDev = true) => {
    const currentEnv = isDev ? env.dev : env.prod

    let chainId = await window.ethereum.request({ method: 'eth_chainId' });

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
        return accounts[0];
      }
      else {
        console.log("No wallet has been connected");
      }
    }

    return false;
}

window.metamaskBundleBuying = async ( bundleId = 1 , isDev = true) => {

  const currentEnv = isDev ? env.dev : env.prod;

  let chainId = await ethereum.request({ method: 'eth_chainId' });

  chainId = parseInt(chainId);

  if( chainId !== currentEnv.chainId ) {

    await window.ethereum.request({
      method: "wallet_addEthereumChain",
      params: [{
        chainId: ethers.utils.hexValue(currentEnv.chainId),
        ...currentEnv.connectionConfig
      }]
    });
  }

  if( chainId === currentEnv.chainId ) {
    const provider = new ethers.providers.Web3Provider(window.ethereum);

    const gemsContract =  new ethers.ethers.Contract(currentEnv.gemsContract, contractAbi, provider);
    const usdcContract = new ethers.ethers.Contract(currentEnv.tokenContract, contractAbi, provider);

    let userAddress = await ethereum.request({method: 'eth_requestAccounts'});
    userAddress = userAddress[0];

    //get the bundle details from contract
    const bundleDetails = await gemsContract.getBundle(bundleId);

    const bundlePrice = bundleDetails.price.toNumber();

    //get the token allowance
    const tokenAllowance = await usdcContract.allowance(userAddress, currentEnv.gemsContract);

    //if not approved, approve the balance, max number so next time user won't have to approve again
    if( tokenAllowance < bundlePrice ){
      let MAX_INT = 11579208923731619542357098500868790785

      const tx = await usdcContract.connect(provider.getSigner()).approve(currentEnv.gemsContract,BigInt(MAX_INT));

      await tx.wait();
    }

    //get the token balance of user
    let userBalance = await usdcContract.balanceOf(userAddress);

    userBalance = userBalance.toNumber();

    //if he has enough balance purchase the gems here
    if( userBalance >= bundlePrice ){

      const tx = await gemsContract.connect(provider.getSigner()).purchaseGemsByToken(bundleId);

      await tx.wait();

      return true;
    }
    else {
      console.log('Not Enough balance to purchase')
    }
  }
}

MEASUREMENT_ID = null;
MEASUREMENT_ID = "G-M7GYWDYGQE"; // mini game

switch (window.location.hostname) {
  case "game.peanutgames.com":
    MEASUREMENT_ID = "G-M7GYWDYGQE"; // mini game
    break;
  case "community.peanutgames.com":
    MEASUREMENT_ID = "G-41NL0CQ277"; // community
    break;
}

if (MEASUREMENT_ID) {
  var analyticsScript = document.getElementById("analytics");
  analyticsScript.onload = function () {
    analyticsScript.src += MEASUREMENT_ID;
  };
  window.dataLayer = window.dataLayer || [];
  function gtag() {
    dataLayer.push(arguments);
  }
  gtag("js", new Date());

  gtag("config", MEASUREMENT_ID);
}
