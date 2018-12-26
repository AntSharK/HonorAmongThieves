"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/heistHub").build();
var userName = null;
var roomId = null;

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

    var elements = document.getElementsByClassName("state");
    for (var i = 0; i < elements.length; i++) {
        elements[i].style.display = "none";
    }

    document.getElementById("pageName").textContent = "LOBBY: " + roomJoined;
    document.getElementById("startLobby").style.display = "block";    
});

connection.on("JoinRoom_UpdateState", function (playersConcat, userJoined) {
    var playerList = document.getElementById("lobbyList");
    playerList.innerHTML = "";
    var players = playersConcat.split("|");
    for (let i = 0; i < players.length; i++) {
        var li = document.createElement("li");
        li.textContent = players[i];
        playerList.appendChild(li);
    }

    var li = document.createElement("li");
    li.textContent = userJoined + " has joined room.";
    var messageList = document.getElementById("messagesList")

    if (messageList != null) {
        messageList.appendChild(li);
    }
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
    var betrayalReward = document.getElementById("betrayalreward").value;
    var maxGameLength = document.getElementById("maxgamelength").value;
    var maxHeistSize = document.getElementById("maxheistsize").value;
    connection.invoke("StartRoom", roomId, betrayalReward, maxGameLength, maxHeistSize).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

// -------------------------------
// ----- STATE: HEIST SIGNUP -----
// -------------------------------
connection.on("StartRoom_ChangeState", function (roomStarted) {
    var elements = document.getElementsByClassName("state");
    for (var i = 0; i < elements.length; i++) {
        elements[i].style.display = "none";
    }

    document.getElementById("pageName").textContent = "GAME STARTED: " + roomStarted;
    var gamestartarea = document.getElementById("gamestart");
    gamestartarea.style.display = "block";
});

connection.on("StartRoom_UpdateState", function (netWorth, years, displayName) {
    document.getElementById("playername").textContent = "NAME: " + displayName;
    document.getElementById("years").textContent = "YEAR: " + years;
    document.getElementById("networth").textContent = "NETWORTH: $" + netWorth + " MILLION";
});

connection.on("HeistPrep_ChangeState", function (playerInfos) {
    var heistParticipantInfo = document.getElementsByClassName("heistparticipantinfo");
    for(var i = heistParticipantInfo.length - 1; i >= 0; i--)
    {
        heistParticipantInfo[i].parentNode.removeChild(heistParticipantInfo[i]);
    }

    var playerList = document.getElementById("heistparticipants");
    var players = playerInfos.split("=");
    for (let i = 0; i < players.length; i++) {
        var playerInfo = players[i].split("|");
        var newRow = playerList.insertRow(playerList.rows.length);
        newRow.className = "heistparticipantinfo";
        newRow.insertCell(0).textContent = playerInfo[0];
        newRow.insertCell(1).textContent = playerInfo[1];
        newRow.insertCell(2).textContent = playerInfo[2];
    }

    var heistsetup = document.getElementById("heistsetup");
    heistsetup.style.display = "block";
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});