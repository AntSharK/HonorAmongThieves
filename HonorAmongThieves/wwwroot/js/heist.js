"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/heistHub").build();

// ---------------------------
// ----- STATE: PRE-GAME -----
// ---------------------------

// Start button clicked
document.getElementById("startbutton").addEventListener("click", function (event) {
    var betrayalReward = document.getElementById("betrayalreward").value;
    var minGameLength = document.getElementById("mingamelength").value;
    var maxGameLength = document.getElementById("maxgamelength").value;
    var minHeistSize = document.getElementById("minheistsize").value;
    var maxHeistSize = document.getElementById("maxheistsize").value;
    var snitchBlackmailWindow = document.getElementById("snitchblackmailwindow").value;
    var blackmailRewardPercentage = document.getElementById("blackmailrewardpercentage").value;
    var networthFudgePercentage = document.getElementById("networthfudgepercentage").value;
    var jailFinePercentage = document.getElementById("jailfinepercentage").value;
    connection.invoke("StartRoom", roomId, betrayalReward, maxGameLength, minGameLength, maxHeistSize, minHeistSize, snitchBlackmailWindow, networthFudgePercentage, blackmailRewardPercentage, jailFinePercentage).catch(function (err) {
        return console.error(err.toString());
    });
    document.getElementById("startButtonDiv").style.display = "none";
    event.preventDefault();
});

document.getElementById("addbotbutton").addEventListener("click", function (event) {
    connection.invoke("AddBot", roomId).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

// -------------------------
// ----- STATE: LOBBY ------
// -------------------------

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

// ------------------------------
// ----- STATE: HEIST START -----
// ------------------------------
connection.on("StartRoom_UpdateState", function (netWorth, years, displayName, minJailTime, maxJailTime, snitchingEvidence) {
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
    document.getElementById("yearstillsnitchingpurge").textContent = "SNITCHING EVIDENCE PURGED: " + snitchingEvidence;
});

connection.on("StartRoom_UpdateGameInfo", function (maxGameLength, minGameLength, snitchBlackmailWindow, blackmailRewardPercentage, jailFinePercentage) {
    document.getElementById("gamelength").textContent = "CAREER: " + minGameLength + "-" + maxGameLength + " YEARS";
    document.getElementById("blackmailwindow").textContent = "EVIDENCE DURATION: " + snitchBlackmailWindow + " YEARS";
    document.getElementById("blackmailreward").textContent = "BLACKMAIL AMOUNT: " + blackmailRewardPercentage + "%";
    document.getElementById("jailfine").textContent = "JAIL FEE: " + jailFinePercentage + "%";
});

connection.on("HeistPrep_ChangeState", function (playerInfos, heistReward, snitchReward) {
    document.getElementById("pageName").textContent = "HEIST SETUP";
    document.getElementById("heistnetworth").textContent = "HEIST REWARD: $" + heistReward + " MILLION";
    document.getElementById("snitchingreward").textContent = "SNITCH REWARD: $" + snitchReward + " MILLION";

    var heistParticipantInfo = document.getElementsByClassName("heistparticipantinfo");
    for(var i = heistParticipantInfo.length - 1; i >= 0; i--) {
        heistParticipantInfo[i].parentNode.removeChild(heistParticipantInfo[i]);
    }

    var blackmailList = document.createElement("select");
    blackmailList.id = "commitblackmailselection";

    var blankOption = document.createElement("option");
    blankOption.textContent = "";
    blackmailList.appendChild(blankOption);

    var playerList = document.getElementById("heistparticipants");
    var players = playerInfos.split("=");
    for (let i = 0; i < players.length; i++) {
        var playerInfo = players[i].split(",");
        var newRow = playerList.insertRow(playerList.rows.length);
        newRow.className = "heistparticipantinfo";
        newRow.insertCell(0).textContent = playerInfo[0];
        newRow.insertCell(1).textContent = playerInfo[1];
        newRow.insertCell(2).textContent = playerInfo[2];

        if (playerInfo[0] != userName) {
            var newOption = document.createElement("option");
            newOption.textContent = playerInfo[0];
            blackmailList.appendChild(newOption);
        }
    }

    document.getElementById("blackmailselection").innerHTML = "BLACKMAIL: ";
    document.getElementById("blackmailselection").appendChild(blackmailList);

    var heistsetup = document.getElementById("heistsetup");
    heistsetup.style.display = "block";
});


connection.on("UpdateCurrentJail", function (jailPlayerList) {
    var jailPlayers = document.getElementById("jailbuddies");
    jailPlayers.innerHTML = "<u>YOUR FELLOW INMATES</u><br>";
    var names = jailPlayerList.split("|");
    for (let i = 0; i < names.length; i++) {
        jailPlayers.innerHTML = jailPlayers.innerHTML + names[i] + "<br />";
    }

    jailPlayers.style.display = "block";
});

connection.on("UpdateGlobalNews", function (newToJailList, heistSuccessList) {
    var newToJail = document.getElementById("newtojail");
    var newToJailNames = newToJailList.split("|");
    if (newToJailNames.length > 0 && newToJailNames[0].length > 0) {
        newToJail.innerHTML = "<u>SENTENCED TO JAIL</u><br>";
        newToJail.style.display = "block";
    }
    for (let i = 0; i < newToJailNames.length; i++) {
        newToJail.innerHTML = newToJail.innerHTML + newToJailNames[i] + "<br />";
    }

    var successfulHeisters = document.getElementById("successfulheisters");
    var heistSuccessNames = heistSuccessList.split("|");
    if (heistSuccessNames.length > 0 && heistSuccessNames[0].length > 0) {
        successfulHeisters.innerHTML = "<u>SUCCESSFUL HEISTS THIS YEAR BY</u><br>";
        successfulHeisters.style.display = "block";
    }
    for (let i = 0; i < heistSuccessNames.length; i++) {
        successfulHeisters.innerHTML = successfulHeisters.innerHTML + heistSuccessNames[i] + "<br />";
    }
});

connection.on("RoomOkay_Update", function (okayPlayerList) {
    var playerList = document.getElementById("playerReadyList");
    playerList.innerHTML = "";
    var players = okayPlayerList.split("|");
    for (let i = 0; i < players.length; i++) {
        var li = document.createElement("li");
        li.textContent = players[i];
        playerList.appendChild(li);
    }
});

// ----------------------------------
// ----- STATE: HEIST DECISIONS -----
// ----------------------------------

document.getElementById("makedecision").addEventListener("click", function (event) {
    var turnUpToHeist = document.getElementById("goforheistcheck").checked;
    var snitchToPolice = document.getElementById("snitchtopolicecheck").checked;
    var victim = document.getElementById("commitblackmailselection").value;
    connection.invoke("MakeDecision", roomId, userName, turnUpToHeist, snitchToPolice, victim).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("goforheistcheck").addEventListener("change", function (event) {
    updateBlackmailState();
})

document.getElementById("snitchtopolicecheck").addEventListener("change", function (event) {
    updateBlackmailState();
})

var updateBlackmailState = function () {
    var goOnHeist = document.getElementById("goforheistcheck").checked;
    var snitchToPolice = document.getElementById("snitchtopolicecheck").checked;

    if (goOnHeist == true && snitchToPolice != true) {
        document.getElementById("blackmailselection").style.display = "block";
        document.getElementById("blackmaildisabled").style.display = "none";
    }
    else {
        document.getElementById("blackmailselection").style.display = "none";
        document.getElementById("blackmaildisabled").style.display = "block";
        if (goOnHeist == false) {
            document.getElementById("cannotblackmailmessage").innerText = "Must go on heist to Blackmail";
        }
        else if (snitchToPolice == true) {
            document.getElementById("cannotblackmailmessage").innerText = "Cannot Blackmail when Snitching";
        }
    }
}

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

connection.on("UpdateHeistSummary", function (fateSummary) {
    var heistSummary = document.getElementById("heistsummary");
    heistSummary.innerHTML = "<u>SUMMARY</u><br>";
    var fates = fateSummary.split("|");
    for (let i = 0; i < fates.length; i++) {
        heistSummary.innerHTML = heistSummary.innerHTML + fates[i] + "<br />";
    }

    heistSummary.style.display = "block";
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
