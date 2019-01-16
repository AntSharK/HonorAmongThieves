"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/realTimeTestHub").build();
var connectionId;
var posX;
var posY;

var dX = 0;
var dY = 0;

var otherPlayersX = {};
var otherPlayersY = {};
var otherPlayersDX = {};
var otherPlayersDY = {};

document.addEventListener('keydown', function (event) {
    if (event.keyCode == 38) {
        dY = -1;
    }

    if (event.keyCode == 40) {
        dY = 1;
    }

    if (event.keyCode == 37) {
        dX = -1;
    }

    if (event.keyCode == 39) {
        dX = 1;
    }
})
document.addEventListener('keyup', function (event) {
    if (event.keyCode == 38) {
        dY = 0;
    }

    if (event.keyCode == 40) {
        dY = 0;
    }

    if (event.keyCode == 37) {
        dX = 0;
    }

    if (event.keyCode == 39) {
        dX = 0;
    }
})

connection.on("EstablishedConnection", function (connectionIdIn, posXIn, posYIn) {
    connectionId = connectionIdIn;
    posX = posXIn;
    posY = posYIn;
});

connection.on("NewPlayer", function (connectionIdIn, posXIn, posYIn) {
    if (connectionIdIn != connectionId) {
        otherPlayersX[connectionIdIn] = posXIn;
        otherPlayersY[connectionIdIn] = posYIn;
        otherPlayersDX[connectionIdIn] = 0;
        otherPlayersDY[connectionIdIn] = 0;
    }
});

connection.on("Disconnect", function (connectionIdIn, posXIn, posYIn) {
    if (connectionIdIn != connectionId) {
        delete otherPlayersX[connectionIdIn];
        delete otherPlayersY[connectionIdIn];
        delete otherPlayersDX[connectionIdIn];
        delete otherPlayersDY[connectionIdIn];
    }
});
connection.on("UpdatePositions", function (parsedPositions) {
    var playersData = parsedPositions.split(",");
    for (let i = 0; i < playersData.length; i++) {
        var playerInfo = playersData[i].split("|");
        var playerId = playerInfo[0];
        var playerPosX = playerInfo[1];
        var playerPosY = playerInfo[2];

        if (playerId != connectionId) {
            otherPlayersDX[playerId] = playerPosX - otherPlayersX[playerId];
            otherPlayersDY[playerId] = playerPosY - otherPlayersY[playerId];
            if (otherPlayersDX[playerId] > -5 && otherPlayersDX[playerId] < 5) {
                otherPlayersDX[playerId] = 0;
            }

            if (otherPlayersDY[playerId] > -5 && otherPlayersDY[playerId] < 5) {
                otherPlayersDY[playerId] = 0;
            }
        }
    }

    connection.invoke("UpdatePosition", posX, posY).catch(function (err) {
        return console.error(err.toString());
    });
});


connection.on("ExistingPlayers", function (parsedPositions) {
    var playersData = parsedPositions.split(",");
    for (let i = 0; i < playersData.length; i++) {
        var playerInfo = playersData[i].split("|");
        var playerId = playerInfo[0];
        var playerPosX = playerInfo[1];
        var playerPosY = playerInfo[2];

        if (playerId != connectionId) {
            otherPlayersX[playerId] = Number(playerPosX);
            otherPlayersY[playerId] = Number(playerPosY);
            otherPlayersDX[playerId] = 0;
            otherPlayersDY[playerId] = 0;
        }
    }
});

// Some drawing thing
function draw() {
    var canvas = document.getElementById("myCanvas");
    var ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.font = "40px Arial";

    posX = posX + dX;
    posY = posY + dY;
    ctx.strokeText("+", posX, posY);
    var serverUpdateRate = 40;

    for (var connectionId in otherPlayersX) {
        ctx.strokeText("O", otherPlayersX[connectionId], otherPlayersY[connectionId]);
        otherPlayersX[connectionId] = otherPlayersX[connectionId] + otherPlayersDX[connectionId] / serverUpdateRate;
        otherPlayersY[connectionId] = otherPlayersY[connectionId] + otherPlayersDY[connectionId] / serverUpdateRate;
    }
}

var timer = setInterval(draw, 10);

connection.start().catch(function (err) {
    return console.error(err.toString());
});