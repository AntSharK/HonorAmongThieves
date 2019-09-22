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

// Entering the baking menu
connection.on("UpdateProductionState", function (currentPrices, currentMarket, playerResources, playerUpgrades, playerBakedGoods) {
    playerState.resources = playerResources;
    playerState.upgrades = playerUpgrades;
    playerState.bakedGoods = playerBakedGoods;
    document.getElementById("pageName").textContent = "BAKE!!";

    // Hide all elements
    var elements = document.getElementsByClassName("state");
    for (var i = 0; i < elements.length; i++) {
        elements[i].style.display = "none";
    }

    // Display the Baking Menu
    var bakingmenu = document.getElementById("bakegoods");
    bakingmenu.style.display = "block";

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

    document.getElementById("moneyowned").textContent = "$" + (playerState.resources.money / 100).toFixed(2);
    document.getElementById("flourowned").textContent = (playerState.resources.flour / 1000) + "g";
    document.getElementById("sugarowned").textContent = (playerState.resources.sugar / 1000) + "g";
    document.getElementById("butterowned").textContent = (playerState.resources.butter / 1000) + "g";

    document.getElementById("flourprice").textContent = "$" + (currentPrices.flour / 100).toFixed(2);
    document.getElementById("butterprice").textContent = "$" + (currentPrices.butter / 100).toFixed(2);
    document.getElementById("sugarprice").textContent = "$" + (currentPrices.sugar / 100).toFixed(2);

    document.getElementById("cookiesbaked").textContent = playerState.bakedGoods.cookies;
    document.getElementById("croissantsbaked").textContent = playerState.bakedGoods.croissants;
    document.getElementById("cakesbaked").textContent = playerState.bakedGoods.cakes;
});

// General update from server to show market report
connection.on("MarketReport", function (currentPrices, currentMarket, playerGoods) {
    // Do stuff
});