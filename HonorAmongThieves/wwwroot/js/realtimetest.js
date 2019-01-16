"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/realTimeTestHub").build();
var connectionId;
var posX;
var posY;

connection.on("EstablishedConnection", function (connectionIdIn, posXIn, posYIn) {
    connectionId = connectionIdIn;
    posX = posXIn;
    posY = posYIn;
});

connection.on("UpdatePositions", function (parsedPositions) {

    connection.invoke("UpdatePosition", posX, posY).catch(function (err) {
        return console.error(err.toString());
    });
});


connection.start().catch(function (err) {
    return console.error(err.toString());
});