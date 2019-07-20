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