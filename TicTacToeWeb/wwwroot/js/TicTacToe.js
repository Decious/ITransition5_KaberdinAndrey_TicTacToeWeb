const socket = new WebSocket("wss://" + location.host);
const alert = document.getElementById("errorAlert");
const modalResult = document.getElementById("ResultModal");
const resultText = document.getElementById("resultText");
const notification = document.getElementById("notificationAlert");
const user = document.getElementById("userNameNav");
const player1 = document.getElementById("gamePlayer1");
const player2 = document.getElementById("gamePlayer2");
var game;
var id;
var field;
var figure;
var figureOpponent;
var chosenTurn;

window.onbeforeunload = closeConnection;
window.onunload = closeConnection;
function closeConnection() {
    socket.onclose = function () { };
    socket.close(1000, "Client closed connection.");
}
socket.onerror = function (event) {
    alert.innerHTML = "<strong>Ошибка подключения! </strong>";
    alert.classList.remove("d-none");
}
socket.onclose = function (event) {
    alert.innerHTML = "<strong>Соединение потеряно... </strong>";
    alert.classList.remove("d-none");
}
socket.onmessage = function (event) {
    var object = JSON.parse(event.data);
    switch (object.Type) {
        case "Credentials":
            id=object.Value
            handleCredentials();
            break;
        case "GameStatus":
            game = JSON.parse(object.Value);
            handleGameStatus(game);
            break;
        case "ChatMessage":
            var textbox = document.getElementById("chatbox");
            var bold = "";
            if (user.innerHTML.includes(object.From)) bold = "font-weight-bold"
            textbox.innerHTML += "<p class=\"text-info " + bold + "\"> <span class=\"text-primary\">" + object.From + "</span>: " + object.Value + "</p>";
            break;
        case "Notification":
            notification.innerHTML = "<strong>"+object.Value+"</strong>";
            notification.classList.remove("d-none");
            setTimeout(returnToIndex, 3000)
            break;
    }
}
function handleCredentials() {
    user.innerHTML = id;
    sendJoinRequest();
}
function sendJoinRequest() {
    var queryString = window.location.search;
    var urlParams = new URLSearchParams(queryString);
    var gameID = urlParams.get('id');
    var obj = { Type: "Game", Action: "Join", Value: gameID };
    var json = JSON.stringify(obj);
    socket.send(json);
}
function handleGameStatus(game) {
    var players = game.PlayerIDs.split(",");
    UpdateGUI(players,game);
    if (game.Result != 2 && game.Result != undefined) {
        if (game.Result == 1) {
            resultText.innerHTML = "НИЧЬЯ: Оппонент покинул игру!";
        } else if (game.Result == 0) {
            resultText.innerHTML = game.CurrentPlayerMove + " выиграл!";
        }
        $('#ResultModal').modal('show');
    }
}
function UpdateGUI(players,game) {
    player1.innerHTML = "<h2 class=\"h2\"><i class=\"fas fa-times\"></i>" + players[0] + "</h2>";
    player2.innerHTML = "<h2 class=\"h2\"><i class=\"far fa-circle\"></i>" + players[1] + "</h2>";
    if (players[0] == id) figure = "fas fa-times"; else figure = "far fa-circle";
    if (players[0] != id) figureOpponent = "fas fa-times"; else figureOpponent = "far fa-circle";
    field = JSON.parse(game.GameFieldJSON);
    updateField();
}
function sendMessage() {
    var text = document.getElementById("messageInput");
    var obj = { Type: "Message", Action: "SendGame", Value: text.value };
    var json = JSON.stringify(obj);
    socket.send(json);
    text.value = '';
}
function returnToIndex() {
    window.location.href = "../"
}
function updateField() {
    var gamePanels = document.getElementsByName("gamePanelCell");
    for (var i = 0; i < field.length; i++) {
        if (field[i] == id) {
            gamePanels[i].innerHTML = "<i class=\"" + figure + "\"></i>";
            gamePanels[i].classList.remove("btn-outline-primary");
        } else if(field[i] != ""){
            gamePanels[i].innerHTML = "<i class=\"" + figureOpponent + "\"></i>";
            gamePanels[i].classList.remove("btn-outline-primary");
        }
    }
}
function gamePanelClicked(position) {
    if (game != undefined && id != undefined) {
        if (game.CurrentPlayerMove == id) {
            if (checkForEmpty(field[position])) {
                chosenTurn = position
                turn();
                return;
            } else {
                notification.innerHTML = "<strong>Клетка не пустая!</strong>";
                notification.classList.remove("d-none");
            }
        } else {
            notification.innerHTML = "<strong>Не ваш ход!</strong>";
            notification.classList.remove("d-none");
        }
    }
}
function turn() {
    game.CurrentPlayerMove = "";
    var obj = { Type: "Game", Action: "Turn", Value: '' + chosenTurn };
    var json = JSON.stringify(obj);
    socket.send(json);
}
function checkForEmpty(string) {
    if (string == "") return true;
    return false;
}

