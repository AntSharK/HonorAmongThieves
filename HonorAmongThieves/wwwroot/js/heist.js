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

connection.on("UpdateHeistStatus", function (title, statusMessage, showOkayButton) {
    var elements = document.getElementsByClassName("state");
    for (var i = 0; i < elements.length; i++) {
        elements[i].style.display = "none";
    }

    if (title.length >= 1) {
        document.getElementById("pageName").textContent = title;
    }

    var gamestartarea = document.getElementById("gamestart");
    gamestartarea.style.display = "block";

    var heiststatusarea = document.getElementById("heiststatus");
    heiststatusarea.style.display = "block";
    document.getElementById("heiststatusmessage").textContent = statusMessage;

    var okayButton = document.getElementById("okaybutton");
    if (showOkayButton) {
        okayButton.style.display = "block";
    }
    else {
        okayButton.style.display = "none";
    }
});

document.getElementById("okaybutton").addEventListener("click", function (event) {
    document.getElementById("okaybutton").style.display = "none";
    connection.invoke("OkayButton", roomId, userName).catch(function (err) {
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

connection.on("JoinRoom_UpdateState", function (playersConcat, userJoined) {
    var playerList = document.getElementById("lobbyList");
    playerList.innerHTML = "";
    var players = playersConcat.split("|");
    for (let i = 0; i < players.length; i++) {
        var li = document.createElement("li");
        li.textContent = players[i];
        playerList.appendChild(li);
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
connection.on("StartRoom_UpdateState", function (netWorth, years, displayName, minJailTime, maxJailTime) {
    var elements = document.getElementsByClassName("state");
    for (var i = 0; i < elements.length; i++) {
        elements[i].style.display = "none";
    }

    var gamestartarea = document.getElementById("gamestart");
    gamestartarea.style.display = "block";

    document.getElementById("playername").textContent = "NAME: " + displayName;
    document.getElementById("years").textContent = "YEAR: " + years;
    document.getElementById("networth").textContent = "NETWORTH: $" + netWorth + " MILLION";
    document.getElementById("nextjailtime").textContent = "NEXT JAIL SENTENCE: " + minJailTime + " to " + maxJailTime + " YEARS";
});

connection.on("HeistPrep_ChangeState", function (playerInfos, heistReward, snitchReward) {
    document.getElementById("pageName").textContent = "HEIST SETUP";
    document.getElementById("heistnetworth").textContent = "HEIST REWARD: $" + heistReward + " MILLION";
    document.getElementById("snitchingreward").textContent = "SNITCH REWARD: $" + snitchReward + " MILLION";

    var heistParticipantInfo = document.getElementsByClassName("heistparticipantinfo");
    for(var i = heistParticipantInfo.length - 1; i >= 0; i--) {
        heistParticipantInfo[i].parentNode.removeChild(heistParticipantInfo[i]);
    }

    var murderList = document.getElementById("commitmurderselection");
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
    var victim = document.getElementById("commitmurderselection").value;
    connection.invoke("CommitMurder", roomId, userName, victim).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("makedecision").addEventListener("click", function (event) {
    var turnUpToHeist = document.getElementById("goforheistcheck").checked;
    var snitchToPolice = document.getElementById("snitchtopolicecheck").checked;
    connection.invoke("MakeDecision", roomId, userName, turnUpToHeist, snitchToPolice).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

connection.on("UpdateHeistMeetup", function (playersConcat) {
    var playerList = document.getElementById("heistMemberList");
    playerList.innerHTML = "";
    var players = playersConcat.split("|");
    for (let i = 0; i < players.length; i++) {
        var li = document.createElement("li");
        li.textContent = players[i];
        playerList.appendChild(li);
    }

    document.getElementById("heistmembers").style.display = "block";
});

// ------------------------------
// ----- STATE: END OF GAME -----
// ------------------------------

connection.on("EndGame_Broadcast", function (year, leaderboarddata) {
    var elements = document.getElementsByClassName("state");
    for (var i = 0; i < elements.length; i++) {
        elements[i].style.display = "none";
    }

    document.getElementById("pageName").textContent = "RETIREMENT";
    document.getElementById("finalyear").textContent = "YEAR: " + year;

    var endofgamearea = document.getElementById("endofgame");
    endofgamearea.style.display = "block";

    var leaderboard = document.getElementById("leaderboard");
    var players = leaderboarddata.split("=");
    for (let i = 0; i < players.length; i++) {
        var playerInfo = players[i].split("|");
        var newRow = leaderboard.insertRow(leaderboard.rows.length);
        newRow.className = "leaderboardinfo";
        newRow.insertCell(0).textContent = playerInfo[0];
        newRow.insertCell(1).textContent = playerInfo[1];
        newRow.insertCell(2).textContent = playerInfo[2];
        newRow.insertCell(3).textContent = playerInfo[3];
    }

    sessionStorage.removeItem("username");
    sessionStorage.removeItem("roomid");
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

connection.on("FreshConnection", function () {
    var sessionUserName = sessionStorage.getItem("username");
    var sessionRoomId = sessionStorage.getItem("roomid");

    if (sessionUserName != null && sessionRoomId != null) {
        // Resume the session
        connection.invoke("ResumeSession", sessionRoomId, sessionUserName).catch(function (err) {
            return console.error(err.toString());
        });
    }
});

connection.on("ClearState", function () {
    sessionStorage.removeItem("username");
    sessionStorage.removeItem("roomid");
})