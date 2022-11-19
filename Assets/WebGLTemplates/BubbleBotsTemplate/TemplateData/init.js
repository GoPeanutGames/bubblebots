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
