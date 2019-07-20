"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/cakeryHub").build();
connection.start().catch(function (err) {
    return console.error(err.toString());
});

var userName;
var roomId;

connection.on("ShowError", function (errorMessage) {
    window.alert(errorMessage);
});

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
// ----- STATE: PRE-GAME -----
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

connection.on("JoinRoom_TakeOverSession", function (roomJoined, userJoined) {
    userName = userJoined;
    roomId = roomJoined;
})

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
    connection.invoke("StartRoom", roomId /*TODO: OTHER PARAMS*/).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});