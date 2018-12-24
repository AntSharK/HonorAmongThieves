"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/heistHub").build();
var userName = null;
var roomId = null;

connection.on("JoinRoom", function (roomJoined, userJoined) {
    if (userName == null) {
        userName = userJoined;
        roomId = roomJoined;

        document.getElementById("joinroom").style.display = "none";
        document.getElementById("pageName").textContent = "LOBBY: " + roomJoined;
    }

    document.getElementById("startLobby").style.display = "block";
    var li = document.createElement("li");
    li.textContent = userJoined + " has joined room.";
    var messageList = document.getElementById("messagesList")

    if (messageList != null) {
        messageList.appendChild(li);
    }
});

connection.on("UpdateRoomList", function (playersConcat) {
    var playerList = document.getElementById("lobbyList");
    playerList.innerHTML = "";
    var players = playersConcat.split("|");
    for (let i = 0; i < players.length; i++) {
        var li = document.createElement("li");
        li.textContent = players[i];
        playerList.appendChild(li);
    }
});

connection.on("CreateStartButton", function (ownerName) {
    if (userName == ownerName) {

        var startButton = document.getElementById("startButtonDiv");
        startButton.style.display = "block";
    }
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("joinroombutton").addEventListener("click", function (event) {
    var userNameIn = document.getElementById("username").value;
    var roomIdIn = document.getElementById("roomid").value;

    connection.invoke("JoinRoom", roomIdIn, userNameIn).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("createroombutton").addEventListener("click", function (event) {
    var userNameIn = document.getElementById("username").value;

    connection.invoke("CreateRoom", userNameIn).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});


document.getElementById("startbutton").addEventListener("click", function (event) {
    connection.invoke("StartRoom", roomId).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});