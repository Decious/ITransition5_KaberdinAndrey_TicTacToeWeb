var socket = new WebSocket("wss://" + location.host);
var tagsModal = document.getElementById("tagsModal");
var gameCardTagInputs = document.getElementsByName("GameTagCardInput");
var gameCards;
var joinable = false;
gameCardTagInputs.forEach(e => {
    var tag = new Tagify(e);
})
initTagsInput();
initTagsModal();

socket.onerror = function (event) {
    var alert = document.getElementById("errorAlert");
    alert.innerHTML = "<strong>Ошибка подключения! </strong>" + event.data;
    alert.classList.remove("d-none");
}
socket.onclose = function (event) {
    var alert = document.getElementById("errorAlert");
    alert.innerHTML = "<strong>Соединение потеряно... </strong>";
    alert.classList.remove("d-none");
}
socket.onmessage = function (event) {
    var object = JSON.parse(event.data);
    console.log(object);
    var user = document.getElementById("userNameNav");
    switch (object.Type) {
        case "Credentials":
            user.innerHTML = object.Value;
            break;
        case "GameAdded":
            location.assign(location.href + "Game?id=" + object.Value);
        case "ChatMessage":
            var textbox = document.getElementById("chatbox");
            var bold = "";
            if(user.innerHTML.includes(object.From)) bold="font-weight-bold"
            textbox.innerHTML += "<p class=\"text-info "+bold+"\"> <span class=\"text-primary\">"+object.From + "</span>: " + object.Value+"</p>";
            break;
    }
}
window.onbeforeunload = function () {
    socket.onclose = function () { };
    socket.close(1000, "Client closed connection.");
};
function sendMessage() {
    var text = document.getElementById("messageInput");
    var obj = { Type: "Message", Action: "Send", Value: text.value };
    var json = JSON.stringify(obj);
    console.log(json);
    socket.send(json);
    text.value = '';
}
function addGame() {
    var gameNameInput = document.getElementById("GameName");
    var tags = tagsModal.value;
    var gameName = gameNameInput.value;
    var GameObject = { Name: gameName, Tags: tags };
    var obj = { Type: "Game", Action: "Add", Value: JSON.stringify(GameObject) }
    var json = JSON.stringify(obj);
    socket.send(json);
}
function initTagsInput() {
    var tagsInput = document.getElementById("tagsInput");
    var tagifyInput = initTags(tagsInput);
    tagifyInput.on('add', e => {
        enforceMaxChars(tagifyInput);
        filter();
    });
    tagifyInput.on('remove', filter);
}
function initTagsModal() {
    var tagifyModal = initTags(tagsModal);
    tagifyModal.on('add', e => enforceMaxChars(tagifyModal));
}
function initTags(tagsElement) {
    var allTags = getAllActiveTags();
    var tagifyElement = new Tagify(tagsElement, {
        whitelist: allTags,
        maxTags: 3,
        dropdown: {
            maxItems: 20,
            classname: "tags-look",
            enabled: 0,
            closeOnSelect: false
        }
    })
    return tagifyElement
}
function getAllActiveTags() {
    var allTags = [];
    gameCardTagInputs.forEach(e => {
        var gameCardTags;
        if (e.value == "") gameCardTags = e.value; else {
            gameCardTags = JSON.parse(e.value);
        }
        for (var i = 0; i < gameCardTags.length; i++) {
            if (isTagUnique(allTags, gameCardTags[i]))
                allTags.push(gameCardTags[i]);
        }
    })
    return allTags;
}
function isTagUnique(allTags, gameCardTag) {
    var isIncluded = false;
    allTags.forEach(el => {
        if (el.value == gameCardTag) isIncluded = true;
    })
    return !isIncluded;
}
function enforceMaxChars(tagify) {
    if (tagify.DOM.input.textContent.length > 11)
        tagify.removeTag();
}
function joinableChanged() {
    joinable = !joinable;
    filter();
}
function filter() {
    gameCards = gameCards = document.getElementsByName("GameCard");
    removeDisplayNone();
    filterByTags();
    filterByJoinable();
}
function removeDisplayNone() {
    gameCards.forEach(e => {
        if(e.classList.contains("d-none"))
        e.classList.remove("d-none");
    })
}
function filterByTags() {
    var tags = getTags();
    if (tags.length == 0) {
        return;
    }
    gameCards.forEach(el => {
        var tagifiers = el.getElementsByClassName("tagify__tag");
        var hasAllFilterTags = true;
            for (var i = 0; i < tags.length; i++) {
                for (var j = 0; j < tagifiers.length; j++) {
                    hasAllFilterTags = true;
                    if (tagifiers[j].attributes["title"].value == tags[i]) break;
                    hasAllFilterTags = false;
                }
                if (!hasAllFilterTags) break;
        }
        if (!hasAllFilterTags) addNoDisplay(el);
    })
}
function getTags() {
    var tagsInputDiv = document.getElementById("tagsInputDiv");
    var filtertags = tagsInputDiv.getElementsByClassName("tagify__tag");
    var tags = [];
    for (var i = 0; i < filtertags.length; i++) {
        tags.push(filtertags[i].attributes["title"].value);
    }
    return tags;
}
function filterByJoinable() {
    if (joinable) {
        gameCards.forEach(e => {
            var playerCount = e.getElementsByClassName("playerCount")[0];
            if (playerCount.value >= 2) addNoDisplay(e);
        })
    } else return;
}
function addNoDisplay(element) {
    if (element.classList.contains("d-none")) return;
    element.classList.add("d-none");
}

