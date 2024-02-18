let url = new URL(location);
let questionId = url.searchParams.get('question-item') ?? null;

$.post('http://localhost:5000/get-question-item', questionId, "json")
    .done(function (response){

        const question = response.Result;
        questionTitle.textContent = question.Title;
        questionText.textContent = question.Text;
        questionTheme.textContent = question.Category;
        if(question.IsBookmark){
            questionBookmark.src = "../images/forum/bookmark1.svg";
            questionBookmark.alt = "click";
        }

        if(question.Avatar){
            document.querySelector('#img_profile').src = question.Avatar;
        }
        const user = response.User;
        if(user.Avatar) {
            profileImg.src = user.UrlAvatar;
        }
        profileName.textContent = user.Name;
        profileNickname.textContent = `@${user.Nickname}`;

        amountComments.textContent = question.Comments.length;
        question.Comments.forEach((element) => addComment(element));
    });

let profileImg = document.getElementById('img_profile');
let profileName = document.getElementById('name_profile');
let profileNickname = document.getElementById('nickname_profile');
let questionTheme = document.getElementById('theme');
let questionBookmark = document.getElementById('bookmark');
let questionTitle = document.getElementById('title');
let questionText = document.getElementById('text');
let formAvatar = document.getElementById('form_avatar');
let formNickname = document.getElementById('form_nickname');
let formTextarea = document.getElementById('form_textarea');
let formButton = document.getElementById('form_button');
let comments = document.getElementById('comments');
let amountComments = document.getElementById('amount_comments');
let buttonBack = document.getElementById('button_back');
let divProfile = document.getElementById('div_profile');

let nickname;
$.ajax({
    url: 'http://localhost:5000/get-cookies',
    method: 'GET',
    dataType: "json",
    success: function(response){
        let user = response.User;
        nickname = user.Nickname;
        if(user.Nickname !== " "){
            if(user.UrlAvatar){
                formAvatar.src = user.UrlAvatar;
            }
            formNickname.appendChild(document.createTextNode(`@${user.Nickname}`));
        } else{
            $('#form').css({
                'display': 'none'
            });
        }
    },
    error: function (data) {
        $('#form').css({
            'display': 'none'
        });
    }
});

$('#form_button').css({
    'background-color': '#919191'
});

formTextarea.onchange = function () {
    if(formTextarea.value) {
        $('#form_button').css({
            'background-color': '#f2a346',
            'cursor': 'pointer'
        });
    } else {
        $('#form_button').css({
            'background-color': '#c7c6c6',
            'cursor': 'default'
        });
    }
};

formButton.onclick = function () {
    if(formTextarea.value){

        const outputdata = {
            Text: formTextarea.value,
            QuestionId: url.searchParams.get('question-item')
        };

        $.ajax({
            url: 'http://localhost:5000/add-comment',
            method: 'POST',
            data: JSON.stringify(outputdata),
            dataType: "json",
            done: function(response){
            },
            error: function (data) {
                if(data.Error){
                    alert(data.Error);
                }
            }
        });
        location.reload();
        formTextarea.value = "";
    }
}

function addComment(element) {
    let item = document.createElement("div");
    item.className = "comment_item";
    item.id = element.Id;
    let itemHeader = document.createElement("div");
    itemHeader.className = "comment_item__header";
    itemHeader.id = element.UserId;
    let imgAvatar = document.createElement("img");
    if(element.Avatar){
        imgAvatar.src = element.Avatar;
    } else {
        imgAvatar.src = "/../images/profile/avatar.png";
    }
    let itemHeaderText = document.createElement("div");
    itemHeaderText.className = "comment_item__header__text";
    let itemHeaderTextNick = document.createElement("div");
    itemHeaderTextNick.className = "comment_item__header__text__nick";
    itemHeaderTextNick.textContent = `@${element.UserId}`;
    let itemHeaderTextDate = document.createElement("div");
    itemHeaderTextDate.className = "comment_item__header__text__date";
    var dateSplit = element.Date.split('-');
    itemHeaderTextDate.textContent = `${dateSplit[2]}.${dateSplit[1]}.${dateSplit[0]}`;
    itemHeaderText.innerHTML += itemHeaderTextNick.outerHTML + itemHeaderTextDate.outerHTML;
    itemHeader.innerHTML += imgAvatar.outerHTML + itemHeaderText.outerHTML;

    let itemText = document.createElement("div");
    itemText.className = "comment_item__text";
    itemText.textContent = element.Text;

    let reactionDiv = document.createElement("div");
    reactionDiv.className = "comment_item__reactions";
    let likeDiv = document.createElement("div");
    let imgLike = document.createElement("img");
    if(element.IsLike){
        imgLike.src = "/../images/forum/like.svg"
        imgLike.alt = "click";
    } else {
        imgLike.src = "/../images/forum/empty-like.svg";
    }
    imgLike.id = 'like';
    let countLike = document.createElement("p");
    countLike.textContent = element.AmountLikes;
    likeDiv.innerHTML += imgLike.outerHTML + countLike.outerHTML;
    let dislikeDiv = document.createElement("div");
    let imgDislike = document.createElement("img");
    if(element.IsDislike){
        imgDislike.src = "/../images/forum/dislike.svg"
        imgDislike.alt = "click";
    } else {
        imgDislike.src = "/../images/forum/empty-dislike.svg";
    }
    imgDislike.id = 'dislike';
    let countDislike = document.createElement("p");
    countDislike.textContent = element.AmountDislikes;

    dislikeDiv.innerHTML += imgDislike.outerHTML + countDislike.outerHTML;
    reactionDiv.innerHTML += likeDiv.outerHTML + dislikeDiv.outerHTML;
    item.innerHTML += itemHeader.outerHTML + itemText.outerHTML + reactionDiv.outerHTML;
    comments.appendChild(item);
}

comments.addEventListener("click", clickComment);

function clickComment(event) {

    if(event.target.className === "comment_item__header"
        || event.target.parentNode.className === "comment_item__header"
        || event.target.parentNode.className === "comment_item__header__text"){

        let url = new URL(`http://localhost:5000/profile`);
        url.searchParams.set('profile-id', event.target.closest('.comment_item__header').id);
        window.location = url;

    } else if(event.target.id === 'like'){
        if(checkAbilityReaction()) {
            return;
        }

        let pNode = event.target.parentNode.querySelector("p");
        if(event.target.alt === "") {
            if(event.target.parentNode.parentNode.querySelector('#dislike').alt !== "click") {
                event.target.src = "/../images/forum/like.svg"
                event.target.alt = addCommentReaction('Like', event.target.closest('.comment_item').id);
                pNode.textContent = parseInt(pNode.textContent) + 1;
            }
        } else {
            event.target.src = "/../images/forum/empty-like.svg"
            event.target.alt = deleteCommentReaction('Like', event.target.closest('.comment_item').id);
            pNode.textContent = parseInt(pNode.textContent) - 1;
        }
        window.location.reload();

    } else if(event.target.id === 'dislike'){
        if(checkAbilityReaction()) {
            return;
        }

        let nodeP = event.target.parentNode.querySelector("p");
        if(event.target.alt === "") {
            if(event.target.parentNode.parentNode.querySelector('#like').alt !== "click") {
                event.target.src = "/../images/forum/dislike.svg"
                event.target.alt = addCommentReaction('Dislike', event.target.closest('.comment_item').id);
                nodeP.textContent = parseInt(nodeP.textContent) + 1;
            }
        } else {
            event.target.src = "/../images/forum/empty-dislike.svg"
            event.target.alt = deleteCommentReaction('Dislike', event.target.closest('.comment_item').id);
            nodeP.textContent = parseInt(nodeP.textContent) - 1;
        }
        window.location.reload();
    }
}
function checkAbilityReaction() {
    if(nickname === " ") {
        alert("Вы не авторизованы!");
        return true;
    }
}
function addCommentReaction (type, questionId) {
    const outputData = {
        UserId: nickname,
        QuestionId: questionId,
        QuestionReactionType: type
    };

    $.post('http://localhost:5000/comment-reaction', JSON.stringify(outputData), "json")
        .done(function (response){
            return "click";
        });

    return "";
}
function deleteCommentReaction (type, questionId) {
    const outputData = {
        UserId: nickname,
        QuestionId: questionId,
        QuestionReactionType: type
    };

    $.post('http://localhost:5000/comment-reaction-delete', JSON.stringify(outputData), "json")
        .done(function (response){
            return "";
        });

    return "click";
}

buttonBack.onclick = function (){
    window.location = document.referrer;
}

questionBookmark.onclick = function () {

    if(checkAbilityReaction()) {
        return;
    }

    if(questionBookmark.alt === "click"){

        const outputData = {
            UserId: nickname,
            QuestionId: questionId,
            QuestionReactionType: "Bookmark"
        };

        $.post('http://localhost:5000/question-reaction-delete', JSON.stringify(outputData), "json")
            .done(function (response){
                questionBookmark.alt =  "";
            });
    } else {

        const outputData = {
            UserId: nickname,
            QuestionId: questionId,
            QuestionReactionType: "Bookmark"
        };

        $.post('http://localhost:5000/question-reaction', JSON.stringify(outputData), "json")
            .done(function (response){
                questionBookmark.alt = "click";
            });
    }
}

divProfile.onclick = function () {
    let url = new URL(`http://localhost:5000/profile`);
    url.searchParams.set('profile-id', profileNickname.textContent.substring(1));
    window.location = url;
}

$('#forum_text').css({
    'font-weight': '700',
    'text-shadow': ' 5px 4px 4px rgba(0, 0, 0, 0.25)',
    'text-decoration': 'underline'
});

function addLinks () {
    document.getElementById('profile_text').onclick = function () {
        let url = new URL(`http://localhost:5000/profile`);
        url.searchParams.set('profile-id', nickname);
        window.location = url;
    }
    document.getElementById('main_text').onclick = function () {
        window.location = 'http://localhost:5000/home';
    }
    document.getElementById('bookmark_text').onclick = function () {
        window.location = 'http://localhost:5000/bookmark';
    }
    document.getElementById('forum_text').onclick = function () {
        window.location = 'http://localhost:5000/all-questions';
    }
}
addLinks();