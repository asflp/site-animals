let nickname;
$.get('http://localhost:5000/get-cookies', "json")
    .done(function (response) {
        let responseJson = JSON.parse(response);
        nickname = responseJson.User.Nickname;

        if(nickname === " "){
            document.querySelector('.header_acc__text__one').textContent = "   ";
            document.querySelector('.header_acc__text__two').textContent = `Гость`;
        } else {
            document.querySelector('.header_acc__text__one').textContent = responseJson.User.Name;
            document.querySelector('.header_acc__text__two').textContent = `@${responseJson.User.Nickname}`;
            if(responseJson.User.Avatar){
                document.querySelector('#profile_img').src = responseJson.User.Avatar;
            }
        }
    });

let allItems = document.getElementById('all_questions');
let textHeader = document.getElementById('text_header');
function showPost (element) {
    let item = document.createElement("div");
    item.className = "question_item";
    item.id = element.Id;
    let itemHeader = document.createElement("div");
    itemHeader.className = "question_item__header";
    itemHeader.textContent = element.Title;
    let itemPerson = document.createElement("div");
    itemPerson.className = "question_item__person";
    let imgPerson = document.createElement("img");
    if(element.Avatar){
        imgPerson.src = element.Avatar;
    } else {
        imgPerson.src = "/../images/profile/avatar.png";
    }
    let itemPersonDiv = document.createElement("div");
    let itemPersonName = document.createElement("p");
    itemPersonName.className = "question_item__person__name";
    itemPersonName.textContent = element.Nickname;
    let itemPersonDate = document.createElement("p");
    itemPersonDate.className = "question_item__person__date";
    itemPersonDate.textContent = "now";
    itemPersonDiv.innerHTML += itemPersonName.outerHTML + itemPersonDate.outerHTML;
    let itemPersonCategory = document.createElement("button");
    itemPersonCategory.type = "button";
    itemPersonCategory.textContent = element.Category;
    itemPerson.innerHTML += imgPerson.outerHTML + itemPersonDiv.outerHTML + itemPersonCategory.outerHTML;
    let itemText = document.createElement("div");
    itemText.className = "question_item__text";
    itemText.textContent = element.Text;
    let itemBottom = document.createElement("div");
    itemBottom.className = "question_item__bottom";
    let itemBottomFirst = document.createElement("div");
    itemBottomFirst.className = "question_item__bottom__first";
    let itemBottomBookmark = document.createElement("img");
    if(element.IsBookmark) {
        itemBottomBookmark.src = "/../images/forum/fill-bookmark.svg";
        itemBottomBookmark.alt = "click";
    } else {
        itemBottomBookmark.src = "/../images/forum/empty-bookmark.svg";
    }
    itemBottomBookmark.id = 'bookmark';
    let itemBottomComment = document.createElement("div");
    let itemBottomCommentImg = document.createElement("img");
    itemBottomCommentImg.src = "/../images/forum/comment.svg";
    let itemBottomComment1 = document.createElement("p");
    itemBottomComment1.textContent = "Добавить комментарий";
    let itemBottomComment2 = document.createElement("p");
    itemBottomComment2.textContent = "0";
    itemBottomComment.innerHTML += itemBottomCommentImg.outerHTML + itemBottomComment1.outerHTML
        + itemBottomComment2.outerHTML;
    itemBottomFirst.innerHTML += itemBottomBookmark.outerHTML + itemBottomComment.outerHTML;
    let itemBottomSecond = document.createElement("div");
    itemBottomSecond.className = "question_item__bottom__second";
    let likesDiv = document.createElement("div");
    let likesImg = document.createElement("img");
    if(element.IsLike) {
        likesImg.src = "/../images/forum/like.svg";
        likesImg.alt = "click";
    } else {
        likesImg.src = "/../images/forum/empty-like.svg";
    }
    likesImg.id = 'like';
    let likesP = document.createElement("p");
    likesP.textContent = element.AmountLike;
    likesDiv.innerHTML += likesImg.outerHTML + likesP.outerHTML;
    let dislikeDiv = document.createElement("div");
    let dislikesImg = document.createElement("img");
    if(element.IsDisLike) {
        dislikesImg.src = "/../images/forum/dislike.svg";
        dislikesImg.alt = "click";
    } else {
        dislikesImg.src = "/../images/forum/empty-dislike.svg";
    }
    dislikesImg.id = 'dislike';
    let dislikesP = document.createElement("p");
    dislikesP.textContent = element.AmountDislike;
    dislikeDiv.innerHTML += dislikesImg.outerHTML + dislikesP.outerHTML;
    itemBottomSecond.innerHTML += likesDiv.outerHTML + dislikeDiv.outerHTML;
    itemBottom.innerHTML += itemBottomFirst.outerHTML + itemBottomSecond.outerHTML;
    item.innerHTML += itemHeader.outerHTML + itemPerson.outerHTML + itemText.outerHTML + itemBottom.outerHTML;

    allItems.appendChild(item);
}

$.get('http://localhost:5000/bookmark-questions', "json")
    .done(function (response){

        const questions = response.Result;
        questions.forEach(item => showPost(item));
    });

allItems.addEventListener('click', clickEvent);

function clickEvent(event){
    switch (event.target.id){
        case 'like':
            if(checkAbilityReaction()) {
                return;
            }
            let pNode = event.target.parentNode.querySelector("p");
            if(event.target.alt === "") {
                if(event.target.parentNode.parentNode.querySelector('#dislike').alt !== "click") {
                    event.target.src = "/../images/forum/like.svg"
                    event.target.alt = addReaction('Like', event.target.closest('.question_item').id);
                    pNode.textContent = parseInt(pNode.textContent) + 1;
                }
            } else {
                event.target.src = "/../images/forum/empty-like.svg"
                event.target.alt = deleteReaction('Like', event.target.closest('.question_item').id);
                pNode.textContent = parseInt(pNode.textContent) - 1;
            }
            window.location.reload();
            break;

        case 'dislike':
            if(checkAbilityReaction()) {
                return;
            }
            let nodeP = event.target.parentNode.querySelector("p");
            if(event.target.alt === "") {
                if(event.target.parentNode.parentNode.querySelector('#like').alt !== "click") {
                    event.target.src = "/../images/forum/dislike.svg"
                    event.target.alt = addReaction('Dislike', event.target.closest('.question_item').id);
                    nodeP.textContent = parseInt(nodeP.textContent) + 1;
                }
            } else {
                event.target.src = "/../images/forum/empty-dislike.svg"
                event.target.alt = deleteReaction('Dislike', event.target.closest('.question_item').id);
                nodeP.textContent = parseInt(nodeP.textContent) - 1;
            }
            window.location.reload();
            break;

        case 'bookmark':
            if(checkAbilityReaction()) {
                return;
            }
            if(event.target.alt === "") {
                event.target.src = "/../images/forum/fill-bookmark.svg"
                event.target.alt = addReaction('Bookmark', event.target.closest('.question_item').id);
            } else {
                event.target.src = "/../images/forum/empty-bookmark.svg"
                event.target.alt = deleteReaction('Bookmark', event.target.closest('.question_item').id);
            }
            window.location.reload();
            break;

        default:

            let url = new URL(`http://localhost:5000/question-item`);
            url.searchParams.set('question-item', event.target.closest('.question_item').id)
            window.location = url;
    }
}

function checkAbilityReaction() {
    if(nickname === " ") {
        alert("Вы не авторизованы!");
        return true;
    }
}

function addReaction (type, questionId) {
    const outputData = {
        UserId: nickname,
        QuestionId: questionId,
        QuestionReactionType: type
    };

    $.post('http://localhost:5000/question-reaction', JSON.stringify(outputData), "json")
        .done(function (response){
            return "click";
        });

    return "";
}

function deleteReaction (type, questionId) {
    const outputData = {
        UserId: nickname,
        QuestionId: questionId,
        QuestionReactionType: type
    };

    $.post('http://localhost:5000/question-reaction-delete', JSON.stringify(outputData), "json")
        .done(function (response){
            return "";
        });

    return "click";
}

$('#bookmark_text').css({
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