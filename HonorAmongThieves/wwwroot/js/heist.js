"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/heistHub").build();
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

connection.on("UpdateHeistStatus", function (statusMessage) {
    var elements = document.getElementsByClassName("state");
    for (var i = 0; i < elements.length; i++) {
        elements[i].style.display = "none";
    }

    document.getElementById("pageName").textContent = "HEIST";
    var gamestartarea = document.getElementById("gamestart");
    gamestartarea.style.display = "block";

    var heiststatusarea = document.getElementById("heiststatus");
    heiststatusarea.style.display = "block";
    document.getElementById("heiststatusmessage").textContent = statusMessage;
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

// ------------------------------
// ----- STATE: HEIST START -----
// ------------------------------
connection.on("StartRoom_ChangeState", function (roomStarted) {
    var elements = document.getElementsByClassName("state");
    for (var i = 0; i < elements.length; i++) {
        elements[i].style.display = "none";
    }

    document.getElementById("pageName").textContent = "GAME STARTED: " + roomStarted;
    var gamestartarea = document.getElementById("gamestart");
    gamestartarea.style.display = "block";
});

connection.on("StartRoom_UpdateState", function (netWorth, years, displayName, minJailTime, maxJailTime) {
    document.getElementById("playername").textContent = "NAME: " + displayName;
    document.getElementById("years").textContent = "YEAR: " + years;
    document.getElementById("networth").textContent = "NETWORTH: $" + netWorth + " MILLION";
    document.getElementById("nextjailtime").textContent = "NEXT JAIL SENTENCE: " + minJailTime + " to " + maxJailTime + " YEARS";
});

connection.on("HeistPrep_ChangeState", function (playerInfos, heistReward, snitchReward) {
    document.getElementById("heistnetworth").textContent = "TOTAL REWARD: $" + heistReward + " MILLION";
    document.getElementById("snitchingreward").textContent = "REWARD FOR SNITCHING: $" + snitchReward + " MILLION";

    var heistParticipantInfo = document.getElementsByClassName("heistparticipantinfo");
    for(var i = heistParticipantInfo.length - 1; i >= 0; i--) {
        heistParticipantInfo[i].parentNode.removeChild(heistParticipantInfo[i]);
    }

    var murderList = document.getElementById("commitmurder");
    for (var i = 0; i < murderList.options.length; i++) {
        murderList.options[i] = null;
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

        if (playerInfo[0] != userName) {
            var newOption = document.createElement("option");
            newOption.textContent = playerInfo[0];
            newOption.textContent = playerInfo[0];
            murderList.appendChild(newOption);
        }
    }

    var heistsetup = document.getElementById("heistsetup");
    heistsetup.style.display = "block";
});

// ----------------------------------
// ----- STATE: HEIST DECISIONS -----
// ----------------------------------

document.getElementById("commitmurder").addEventListener("click", function (event) {
    var victim = document.getElementById("commitmurder").value;
    connection.invoke("CommitMurder", roomId, userName, victim).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});