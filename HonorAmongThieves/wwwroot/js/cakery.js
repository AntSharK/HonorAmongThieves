"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/cakeryHub").build();
var userName;
var roomId;

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

// Entering the baking menu
connection.on("UpdateProductionState", function (currentPrices, currentMarket, playerResources, playerUpgrades, playerBakedGoods) {
    playerState.resources = playerResources;
    playerState.upgrades = playerUpgrades;
    playerState.bakedGoods = playerBakedGoods;
    document.getElementById("pageName").textContent = "BAKE THINGS";

    // Hide all elements
    var elements = document.getElementsByClassName("state");
    for (var i = 0; i < elements.length; i++) {
        elements[i].style.display = "none";
    }

    // Display the Baking Menu
    var bakingmenu = document.getElementById("bakegoods");
    bakingmenu.style.display = "block";

    var cakeCost = playerState.bakedGoods.cakeCost;
    var croissantCost = playerState.bakedGoods.croissantCost;
    var cookieCost = playerState.bakedGoods.cookieCost;
    document.getElementById("cookieprice").textContent = "Requires: " + cookieCost.item1 + "g butter, "
        + cookieCost.item2 + "g flour, "
        + cookieCost.item3 + "g sugar, "
        + "and $" + cookieCost.item4;
    document.getElementById("croissantprice").textContent = "Requires: " + croissantCost.item1 + "g butter, "
        + croissantCost.item2 + "g flour, "
        + croissantCost.item3 + "g sugar, "
        + "and $" + croissantCost.item4;
    document.getElementById("cakeprice").textContent = "Requires: " + cakeCost.item1 + "g butter, "
        + cakeCost.item2 + "g flour, "
        + cakeCost.item3 + "g sugar, "
        + "and $" + cakeCost.item4;

    document.getElementById("flourprice").textContent = "Cost: $" + currentPrices.flour / 100;
    document.getElementById("butterprice").textContent = "Cost: $" + currentPrices.butter / 100;
    document.getElementById("sugarprice").textContent = "Cost: $" + currentPrices.sugar / 100;

    // Update the available resources and current prices
    document.getElementById("resources").textContent =
        "MONEYS: $" + playerState.resources.money +
        " - FLOUR: " + playerState.resources.flour + "g" +
        " - SUGAR: " + playerState.resources.sugar + "g" +
        " - BUTTER: " + playerState.resources.butter + "g";

    document.getElementById("goodsbaked").textContent =
        playerState.bakedGoods.cookies + " Cookies, " +
        playerState.bakedGoods.croissants + " Croissants, " +
        playerState.bakedGoods.cakes + " Cakes.";
});

// TODO: Each button for buying goods and baking things

// General update from server to show market report
connection.on("MarketReport", function (currentPrices, currentMarket, playerGoods) {
    // Do stuff
});