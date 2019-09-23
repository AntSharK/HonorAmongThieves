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
    // TODO: Upgrades purchased
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

    document.getElementById("bakecookiesamount").value = 0;
    document.getElementById("bakecroissantsamount").value = 0;
    document.getElementById("bakecakesamount").value = 0;

    // Update the available resources and current prices
    var cakeCost = playerState.bakedGoods.cakeCost;
    var croissantCost = playerState.bakedGoods.croissantCost;
    var cookieCost = playerState.bakedGoods.cookieCost;
    document.getElementById("cookiepricebutter").textContent = cookieCost.item1 + "g";
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

document.getElementById("buyingredientsbutton").addEventListener("click", function (event) {
    var butterbought = document.getElementById("buybutteramount").value;
    var flourbought = document.getElementById("buyflouramount").value;
    var sugarbought = document.getElementById("buysugaramount").value;

    // Client-side check to make sure this is possible
    var moneySpent = gameState.currentPrices.butter * butterbought
        + gameState.currentPrices.flour * flourbought
        + gameState.currentPrices.sugar * sugarbought;

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

document.getElementById("switchtoupgradeviewbutton").addEventListener("click", function (event) {
    showUpgradeMenu();
    event.preventDefault();
});

// TODO: Show the upgrade menu
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

document.getElementById("switchtobakeviewbutton").addEventListener("click", function (event) {
    showBakeMenu();
    event.preventDefault();
});

// TODO: Show your setting up shop - all resources, and things to be sold
connection.on("SetUpShop", function (playerResources, playerUpgrades, playerBakedGoods) {
    playerState.resources = playerResources;
    playerState.upgrades = playerUpgrades;
    playerState.bakedGoods = playerBakedGoods;
});

// TODO: Show market report
connection.on("MarketReport", function (currentPrices, currentMarket, playerBakedGoods) {

    // Do stuff
});