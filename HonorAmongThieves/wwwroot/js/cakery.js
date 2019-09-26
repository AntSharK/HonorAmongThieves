"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/cakeryHub").build();
var userName;
var roomId;
var baking = true;

// -------------------------
// ----- GAME OBJECTS ------
// -------------------------
// The player 
var roundActions = {
    goldSpent: 0,
    cakesBaked: 0,
    croissantsBaked: 0,
    cookiesBaked: 0,
    // TODO (Upgrades): Upgrades purchased
}

// The player state currently, not including this round's purchases
var playerState = {
    resources: {},
    upgrades: {},
    bakedGoods: {},
}

var gameState = {
    currentPrices: {},
    currentMarket: {}
}

// -------------------------
// ----- STATE: CONNECTING -
// -------------------------
connection.on("ShowError", function (errorMessage) {
    window.alert(errorMessage);
});

connection.on("FreshConnection", function () {
    var sessionUserName = sessionStorage.getItem("username");
    var sessionRoomId = sessionStorage.getItem("roomid");

    if (sessionUserName != null && sessionRoomId != null) {
        // Resume the session
        userName = sessionUserName;
        roomId = sessionRoomId;

        connection.invoke("ResumeSession", roomId, userName).catch(function (err) {
            return console.error(err.toString());
        });
    }
});

connection.on("ClearState", function () {
    sessionStorage.removeItem("username");
    sessionStorage.removeItem("roomid");
    location.reload();
})

var conditionalReload = function () {
    var sessionUserName = sessionStorage.getItem("username");
    var sessionRoomId = sessionStorage.getItem("roomid");

    if (sessionUserName != null && sessionRoomId != null) {
        var elements = document.getElementsByClassName("state");
        for (var i = 0; i < elements.length; i++) {
            elements[i].style.display = "none";
        }

        document.getElementById("pageName").textContent = "RECONNECTING...";
    }
}

connection.start().catch(function (err) {
    return console.error(err.toString());
});

window.onload = conditionalReload;

// -------------------------
// ----- STATE: LOBBY ------
// -------------------------
document.getElementById("createroombutton").addEventListener("click", function (event) {
    var userNameIn = document.getElementById("username").value;

    connection.invoke("CreateRoom", userNameIn).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

// ---------------------------
// ----- STATE: GAME SETUP ---
// ---------------------------
connection.on("JoinRoom_ChangeState", function (roomJoined, userJoined) {
    // Hide the start button by default
    var startButton = document.getElementById("startButtonDiv");
    startButton.style.display = "none";

    // This event is only sent to the caller on joining a room
    userName = userJoined;
    roomId = roomJoined;

    // Write to session storage
    sessionStorage.setItem("username", userName);
    sessionStorage.setItem("roomid", roomId);

    var elements = document.getElementsByClassName("state");
    for (var i = 0; i < elements.length; i++) {
        elements[i].style.display = "none";
    }

    document.getElementById("pageName").textContent = "LOBBY: " + roomJoined;
    document.getElementById("startLobby").style.display = "block";
});

// Taking over an existing user
connection.on("JoinRoom_TakeOverSession", function (roomJoined, userJoined) {
    userName = userJoined;
    roomId = roomJoined;
})

// Player joins the room
connection.on("JoinRoom_UpdateState", function (playersConcat, userJoined) {
    var playerList = document.getElementById("lobbyList");
    playerList.innerHTML = "";
    var players = playersConcat.split("|");
    for (let i = 0; i < players.length; i++) {
        var li = document.createElement("li");
        li.textContent = players[i];
        playerList.appendChild(li);
    }

    document.getElementById("lobbyplayercount").textContent = players.length + "/20";
});

document.getElementById("joinroombutton").addEventListener("click", function (event) {
    var userNameIn = document.getElementById("username").value;
    var roomIdIn = document.getElementById("roomid").value;

    connection.invoke("JoinRoom", roomIdIn, userNameIn).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

// Start button
connection.on("JoinRoom_CreateStartButton", function () {
    var startButton = document.getElementById("startButtonDiv");
    startButton.style.display = "block";
});

document.getElementById("startbutton").addEventListener("click", function (event) {
    var gamelength = document.getElementById("gamelength").value;
    var startingcash = document.getElementById("startingcash").value;
    connection.invoke("StartRoom", roomId, gamelength, startingcash).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

function changeUiState(title, stateToChange) {
    document.getElementById("pageName").textContent = title;

    // Hide all elements
    var elements = document.getElementsByClassName("state");
    for (var i = 0; i < elements.length; i++) {
        elements[i].style.display = "none";
    }

    // Display the new UI
    var bakingmenu = document.getElementById(stateToChange);
    bakingmenu.style.display = "block";
}

// -------------------------
// ----- STATE: BAKING -----
// -------------------------
// Entering the baking menu
connection.on("UpdateProductionState", function (currentPrices, currentMarket, playerResources, playerUpgrades, playerBakedGoods) {
    gameState.currentPrices = currentPrices;
    gameState.currentMarket = currentMarket;
    playerState.resources = playerResources;
    playerState.upgrades = playerUpgrades;
    playerState.bakedGoods = playerBakedGoods;
    if (baking) {
        showBakeMenu();
    }
    else {
        showUpgradeMenu();
    }
});

function showBakeMenu() {
    baking = true;
    changeUiState("BAKE!!", "bakegoods");

    // Reset the input forms
    document.getElementById("buybutteramount").value = 0;
    document.getElementById("buyflouramount").value = 0;
    document.getElementById("buysugaramount").value = 0;
    document.getElementById("ingredientcost").textContent = "";
    document.getElementById("buyingredientsbutton").disabled = true;

    document.getElementById("bakecookiesamount").value = 0;
    document.getElementById("bakecroissantsamount").value = 0;
    document.getElementById("bakecakesamount").value = 0;
    document.getElementById("moneyforbaking").textContent = "";
    document.getElementById("flourforbaking").textContent = "";
    document.getElementById("sugarforbaking").textContent = "";
    document.getElementById("butterforbaking").textContent = "";
    document.getElementById("bakethingsbutton").disabled = true;

    // Update the available resources and current prices
    var cakeCost = playerState.bakedGoods.cakeCost;
    var croissantCost = playerState.bakedGoods.croissantCost;
    var cookieCost = playerState.bakedGoods.cookieCost;
    document.getElementById("cookiepricebutter").textContent = parseFloat(cookieCost.item1) + "g";
    document.getElementById("cookiepriceflour").textContent = cookieCost.item2 + "g";
    document.getElementById("cookiepricesugar").textContent = cookieCost.item3 + "g";
    document.getElementById("cookiepricemoney").textContent = "$" + (cookieCost.item4 / 100).toFixed(2);
    document.getElementById("croissantpricebutter").textContent = croissantCost.item1 + "g";
    document.getElementById("croissantpriceflour").textContent = croissantCost.item2 + "g";
    document.getElementById("croissantpricesugar").textContent = croissantCost.item3 + "g";
    document.getElementById("croissantpricemoney").textContent = "$" + (croissantCost.item4 / 100).toFixed(2);
    document.getElementById("cakepricebutter").textContent = cakeCost.item1 + "g";
    document.getElementById("cakepriceflour").textContent = cakeCost.item2 + "g";
    document.getElementById("cakepricesugar").textContent = cakeCost.item3 + "g";
    document.getElementById("cakepricemoney").textContent = "$" + (cakeCost.item4 / 100).toFixed(2);

    document.getElementById("moneyowned").textContent = "Cash Available: $" + (playerState.resources.money / 100).toFixed(2);
    document.getElementById("flourowned").textContent = playerState.resources.flour + "g";
    document.getElementById("sugarowned").textContent = playerState.resources.sugar + "g";
    document.getElementById("butterowned").textContent = playerState.resources.butter + "g";

    document.getElementById("flourprice").textContent = "$" + (gameState.currentPrices.flour / 100).toFixed(2);
    document.getElementById("butterprice").textContent = "$" + (gameState.currentPrices.butter / 100).toFixed(2);
    document.getElementById("sugarprice").textContent = "$" + (gameState.currentPrices.sugar / 100).toFixed(2);

    document.getElementById("cookiesbaked").textContent = playerState.bakedGoods.cookies;
    document.getElementById("croissantsbaked").textContent = playerState.bakedGoods.croissants;
    document.getElementById("cakesbaked").textContent = playerState.bakedGoods.cakes;

    document.getElementById("currentyear").textContent = "YEAR: " + (gameState.currentMarket.currentYear + 1) + "/" + gameState.currentMarket.maxYears;
}

// Click on buying ingredients
document.getElementById("buyingredientsbutton").addEventListener("click", function (event) {
    var moneySpent = getIngredientsCost();

    var butterbought = document.getElementById("buybutteramount").value;
    var flourbought = document.getElementById("buyflouramount").value;
    var sugarbought = document.getElementById("buysugaramount").value;

    if (moneySpent > playerState.resources.money) {
        window.alert("NOT ENOUGH MONEY! Ingredients cost $" + (moneySpent / 100).toFixed(2)
            + " but you only have $" + (playerState.resources.money / 100).toFixed(2));
    }
    else {
        connection.invoke("BuyIngredients", roomId, userName, butterbought, flourbought, sugarbought).catch(function (err) {
            return console.error(err.toString());
        });
    }

    event.preventDefault();
});

function getIngredientsCost() {
    var butterbought = document.getElementById("buybutteramount").value;
    var flourbought = document.getElementById("buyflouramount").value;
    var sugarbought = document.getElementById("buysugaramount").value;

    // Client-side check to make sure this is possible
    var moneySpent = gameState.currentPrices.butter * butterbought
        + gameState.currentPrices.flour * flourbought
        + gameState.currentPrices.sugar * sugarbought;

    return moneySpent;
}

document.getElementById("buyflouramount").addEventListener("change", function (event) {
    updateIngredientsCost();
});
document.getElementById("buysugaramount").addEventListener("change", function (event) {
    updateIngredientsCost();
});
document.getElementById("buybutteramount").addEventListener("change", function (event) {
    updateIngredientsCost();
});

function updateIngredientsCost() {
    var ingredientsCost = getIngredientsCost();

    // Early abort if nothing is being bought
    if (ingredientsCost <= 0) {
        document.getElementById("ingredientcost").textContent = "";
        document.getElementById("buyingredientsbutton").disabled = true;
        return;
    }

    document.getElementById("ingredientcost").textContent = "$" + (ingredientsCost / 100).toFixed(2) + "/$" + (playerState.resources.money / 100).toFixed(2);

    if (ingredientsCost > playerState.resources.money) {
        document.getElementById("ingredientcost").style.color = "red";
        document.getElementById("buyingredientsbutton").disabled = true;
    }
    else {
        document.getElementById("ingredientcost").style.color = "blue";
        document.getElementById("buyingredientsbutton").disabled = false;
    }
}

// Click on baking things
document.getElementById("bakethingsbutton").addEventListener("click", function (event) {
    var cookiesBaked = document.getElementById("bakecookiesamount").value;
    var croissantsBaked = document.getElementById("bakecroissantsamount").value;
    var cakesBaked = document.getElementById("bakecakesamount").value;

    var cakeCost = playerState.bakedGoods.cakeCost;
    var croissantCost = playerState.bakedGoods.croissantCost;
    var cookieCost = playerState.bakedGoods.cookieCost;

    // Client-side check to make sure this is possible
    var butterUsed = cookieCost.item1 * cookiesBaked + croissantCost.item1 * croissantsBaked + cakeCost.item1 * cakesBaked;
    var flourUsed = cookieCost.item2 * cookiesBaked + croissantCost.item2 * croissantsBaked + cakeCost.item2 * cakesBaked;
    var sugarUsed = cookieCost.item3 * cookiesBaked + croissantCost.item3 * croissantsBaked + cakeCost.item3 * cakesBaked;
    var moneyUsed = cookieCost.item4 * cookiesBaked + croissantCost.item4 * croissantsBaked + cakeCost.item4 * cakesBaked;

    if (moneyUsed > playerState.resources.money
        || flourUsed > playerState.resources.flour
        || butterUsed > playerState.resources.butter
        || sugarUsed > playerState.resources.sugar) {
        window.alert("NOT ENOUGH RESOURCES! Ingredients cost $" + (moneyUsed / 100).toFixed(2)
            + ", " + butterUsed + "g butter, " + flourUsed + "g flour, " + sugarUsed + "g sugar.");
    }
    else {
        connection.invoke("BakeGoods", roomId, userName, cookiesBaked, croissantsBaked, cakesBaked).catch(function (err) {
            return console.error(err.toString());
        });
    }

    event.preventDefault();
});

document.getElementById("bakecookiesamount").addEventListener("change", function (event) {
    updateBakingCost();
});
document.getElementById("bakecroissantsamount").addEventListener("change", function (event) {
    updateBakingCost();
});
document.getElementById("bakecakesamount").addEventListener("change", function (event) {
    updateBakingCost();
});

function updateBakingCost() {
    // This is a copy+paste of a function that exists when clicking the "Bake" button
    var cookiesBaked = document.getElementById("bakecookiesamount").value;
    var croissantsBaked = document.getElementById("bakecroissantsamount").value;
    var cakesBaked = document.getElementById("bakecakesamount").value;

    // Early abort if nothing is being baked
    if (cookiesBaked + croissantsBaked + cakesBaked <= 0) {
        document.getElementById("bakethingsbutton").disabled = true;
        document.getElementById("moneyforbaking").textContent = "";
        document.getElementById("flourforbaking").textContent = "";
        document.getElementById("sugarforbaking").textContent = "";
        document.getElementById("butterforbaking").textContent = "";
        return;
    }

    var cakeCost = playerState.bakedGoods.cakeCost;
    var croissantCost = playerState.bakedGoods.croissantCost;
    var cookieCost = playerState.bakedGoods.cookieCost;

    var butterUsed = cookieCost.item1 * cookiesBaked + croissantCost.item1 * croissantsBaked + cakeCost.item1 * cakesBaked;
    var flourUsed = cookieCost.item2 * cookiesBaked + croissantCost.item2 * croissantsBaked + cakeCost.item2 * cakesBaked;
    var sugarUsed = cookieCost.item3 * cookiesBaked + croissantCost.item3 * croissantsBaked + cakeCost.item3 * cakesBaked;
    var moneyUsed = cookieCost.item4 * cookiesBaked + croissantCost.item4 * croissantsBaked + cakeCost.item4 * cakesBaked;
    // Copy+paste ends here

    document.getElementById("butterforbaking").textContent = butterUsed + "g";
    document.getElementById("flourforbaking").textContent = flourUsed + "g";
    document.getElementById("sugarforbaking").textContent = sugarUsed + "g";
    document.getElementById("moneyforbaking").textContent = "$" + (moneyUsed / 100).toFixed(2);

    var bakingEnabled = true;

    if (butterUsed > playerState.resources.butter) {
        document.getElementById("butterforbaking").style.color = "red";
        bakingEnabled = false;
    }
    else {
        document.getElementById("butterforbaking").style.color = "blue";
    }

    if (flourUsed > playerState.resources.flour) {
        document.getElementById("flourforbaking").style.color = "red";
        bakingEnabled = false;
    }
    else {
        document.getElementById("flourforbaking").style.color = "blue";
    }

    if (sugarUsed > playerState.resources.sugar) {
        document.getElementById("sugarforbaking").style.color = "red";
        bakingEnabled = false;
    }
    else {
        document.getElementById("sugarforbaking").style.color = "blue";
    }

    if (moneyUsed > playerState.resources.money) {
        document.getElementById("moneyforbaking").style.color = "red";
        bakingEnabled = false;
    }
    else {
        document.getElementById("moneyforbaking").style.color = "blue";
    }

    if (bakingEnabled) {
        document.getElementById("bakethingsbutton").disabled = false;
    }
    else {
        document.getElementById("bakethingsbutton").disabled = true;
    }
}

// Click on "Upgrade" view
document.getElementById("switchtoupgradeviewbutton").addEventListener("click", function (event) {
    showUpgradeMenu();
    event.preventDefault();
});

// TODO (Upgrades): Show the actual list of upgrades upgrade menu
function showUpgradeMenu() {
    baking = false;
    changeUiState("UPGRADE!!", "upgrademenu");

    document.getElementById("moneyowned2").textContent = "Cash Available: $" + (playerState.resources.money / 100).toFixed(2);
    document.getElementById("flourowned2").textContent = (playerState.resources.flour / 1000) + "g";
    document.getElementById("sugarowned2").textContent = (playerState.resources.sugar / 1000) + "g";
    document.getElementById("butterowned2").textContent = (playerState.resources.butter / 1000) + "g";

    document.getElementById("cookiesbaked2").textContent = playerState.bakedGoods.cookies;
    document.getElementById("croissantsbaked2").textContent = playerState.bakedGoods.croissants;
    document.getElementById("cakesbaked2").textContent = playerState.bakedGoods.cakes;
}

// Switch back to "Baking" view
document.getElementById("switchtobakeviewbutton").addEventListener("click", function (event) {
    showBakeMenu();
    event.preventDefault();
});

document.getElementById("gobacktobakeviewbutton").addEventListener("click", function (event) {
    showBakeMenu();
    event.preventDefault();
});

// Go to set up shop screen
document.getElementById("endturnbutton").addEventListener("click", function (event) {
    summarizeGoodsBaked();
    document.getElementById("confirmationbuttons").style.display = "block";
    document.getElementById("readylist").style.display = "none";

    event.preventDefault();
});

// Summarize the elements in goods baked
function summarizeGoodsBaked() {
    changeUiState("SET UP SHOP", "goodsbaked");
    document.getElementById("currentyearsummary").textContent = "YEAR: " + (gameState.currentMarket.currentYear + 1) + "/" + gameState.currentMarket.maxYears;

    document.getElementById("cookiesbeingsold").textContent = playerState.bakedGoods.cookies;
    document.getElementById("croissantsbeingsold").textContent = playerState.bakedGoods.croissants;
    document.getElementById("cakesbeingsold").textContent = playerState.bakedGoods.cakes;

    // TODO (Upgrades): List upgrades
}

// Confirm ending of turn
document.getElementById("reallyendturnbutton").addEventListener("click", function (event) {
    connection.invoke("SetUpShop", roomId, userName).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

// Show setting up shop screen
connection.on("SetUpShop", function (currentPrices, currentMarket, playerResources, playerUpgrades, playerBakedGoods, readyPlayers, notReadyPlayers) {
    gameState.currentPrices = currentPrices;
    gameState.currentMarket = currentMarket;
    playerState.resources = playerResources;
    playerState.upgrades = playerUpgrades;
    playerState.bakedGoods = playerBakedGoods;

    summarizeGoodsBaked();
    updatePlayerList(readyPlayers, notReadyPlayers);
});

connection.on("UpdatePlayerList", function (readyPlayers, notReadyPlayers) {
    updatePlayerList(readyPlayers, notReadyPlayers);
})

function updatePlayerList(readyPlayers, slowBastards) {
    document.getElementById("confirmationbuttons").style.display = "none";
    document.getElementById("readylist").style.display = "block";

    var readyList = document.getElementById("playerreadylist");
    readyList.innerHTML = "";
    for (let i = 0; i < readyPlayers.length; i++) {
        var li = document.createElement("li");
        li.textContent = readyPlayers[i];
        readyList.appendChild(li);
    }

    var slowBastardList = document.getElementById("playerwaitinglist");
    slowBastardList.innerHTML = "";
    for (let i = 0; i < slowBastards.length; i++) {
        var li = document.createElement("li");
        li.textContent = slowBastards[i];
        slowBastardList.appendChild(li);
    }
}

// Show market report
connection.on("ShowMarketReport", function (newsReport, playerSales, goodPrices, playerProfit, currentMarket) {
    gameState.currentMarket = currentMarket;
    baking = true;
    changeUiState("MARKET REPORT", "marketreport");

    document.getElementById("marketreportnews").textContent = newsReport;
    document.getElementById("salestabletitle").textContent = "YEAR: " + gameState.currentMarket.currentYear + "/"
        + gameState.currentMarket.maxYears + " SALES REPORT";

    if (playerSales.item1 > 0) {
        document.getElementById("marketreportcookieprice").textContent = "$" + (goodPrices.item1 / 100).toFixed(2)
        document.getElementById("marketreportcookieamount").textContent = playerSales.item1;
        document.getElementById("marketreportcookierevenue").textContent = "$" + (playerSales.item1 * goodPrices.item1 / 100).toFixed(2);
    }
    else {
        document.getElementById("marketreportcookieprice").textContent = ""
        document.getElementById("marketreportcookieamount").textContent = ""
        document.getElementById("marketreportcookierevenue").textContent = ""
    }

    if (playerSales.item2 > 0) {
        document.getElementById("marketreportcroissantprice").textContent = "$" + (goodPrices.item2 / 100).toFixed(2)
        document.getElementById("marketreportcroissantamount").textContent = playerSales.item2;
        document.getElementById("marketreportcroissantrevenue").textContent = "$" + (playerSales.item2 * goodPrices.item2 / 100).toFixed(2);
    }
    else {
        document.getElementById("marketreportcroissantprice").textContent = ""
        document.getElementById("marketreportcroissantamount").textContent = ""
        document.getElementById("marketreportcroissantrevenue").textContent = ""
    }

    if (playerSales.item3 > 0) {
        document.getElementById("marketreportcakeprice").textContent = "$" + (goodPrices.item3 / 100).toFixed(2)
        document.getElementById("marketreportcakeamount").textContent = playerSales.item3;
        document.getElementById("marketreportcakerevenue").textContent = "$" + (playerSales.item3 * goodPrices.item3 / 100).toFixed(2);
    }
    else {
        document.getElementById("marketreportcakeprice").textContent = ""
        document.getElementById("marketreportcakeamount").textContent = ""
        document.getElementById("marketreportcakerevenue").textContent = ""
    }

    document.getElementById("salestablesummary").textContent = "TOTAL REVENUE: $" + (playerProfit / 100).toFixed(2);
});

// Stop viewing market report
document.getElementById("endmarketreportbutton").addEventListener("click", function (event) {
    connection.invoke("EndMarketReport", roomId, userName).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

// End of game
connection.on("EndGame", function (totalSales, playerSales) {
    changeUiState("END OF GAME", "endgame");

    // TODO: Fix display bug
    var leaderboard = document.getElementById("endgameleaderboard");
    for (var i = 0; i < totalSales.length; i++) {
        var tr = document.createElement("TR");

        var name = document.createElement("TD");
        name.appendChild(document.createTextNode(totalSales[i].item1));
        tr.appendChild(name);
        var cookies = document.createElement("TD");
        cookies.appendChild(document.createTextNode(totalSales[i].item2));
        tr.appendChild(cookies);
        var croissants = document.createElement("TD");
        croissants.appendChild(document.createTextNode(totalSales[i].item3));
        tr.appendChild(croissants);
        var cakes = document.createElement("TD");
        cakes.appendChild(document.createTextNode(totalSales[i].item4));
        tr.appendChild(cakes);
        var total = document.createElement("TD");
        total.appendChild(document.createTextNode("$" + (totalSales[i].item5 / 100).toFixed(2)));
        tr.appendChild(total);
        leaderboard.appendChild(tr);
    }

    var annualreport = document.getElementById("annualreport");
    for (var i = 0; i <= playerSales.length; i++) {
        var tr = document.createElement("TR");

        var year = document.createElement("TD");
        year.appendChild(document.createTextNode(i + 1));
        tr.appendChild(year);

        var cookies = document.createElement("TD");
        cookies.appendChild(document.createTextNode(playerSales[i].item1));
        tr.appendChild(cookies);
        var croissants = document.createElement("TD");
        croissants.appendChild(document.createTextNode(playerSales[i].item2));
        tr.appendChild(croissants);
        var cakes = document.createElement("TD");
        cakes.appendChild(document.createTextNode(playerSales[i].item3));
        tr.appendChild(cakes);
        var total = document.createElement("TD");
        total.appendChild(document.createTextNode("$" + (playerSales[i].item4 / 100).toFixed(2)));
        tr.appendChild(total);

        annualreport.appendChild(tr);
    }

    sessionStorage.removeItem("username");
    sessionStorage.removeItem("roomid");
});