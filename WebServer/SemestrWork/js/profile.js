const elName = document.getElementById("account_name");
const elNick = document.getElementById("account_nick");
const elDesc = document.getElementById("account_desc");
const elCity = document.getElementById("account_city");
const elLink = document.getElementById("account_link");
const elAvatar = document.getElementById("avatar");
const elBanner = document.getElementById("banner")

let url = new URL(location);
let profileId = url.searchParams.get('profile-id') ?? null;
$('#profile_text').css({
    'font-weight': '700',
    'text-shadow': ' 5px 4px 4px rgba(0, 0, 0, 0.25)',
    'text-decoration': 'underline'
});

let nickname;
$.get('http://localhost:5000/get-cookies', "json")
    .done(function (response) {
        let responseJson = JSON.parse(response);
        nickname = responseJson.User.Nickname;
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

document.getElementById('back_button').onclick = function () {
    document.cookie = "SessionId=";
    window.location = 'http://localhost:5000/signin';
}

const divLike = document.getElementById("liked_post");
const divUsual = document.getElementById("usual_post");

divLike.onclick = function () {
    $('#liked_post').css({
        'font-weight': '700'
    });

    $('#usual_post').css({
        'font-weight': '500'
    });

    $('#post__underline').css({
        'left': '65.4%'
    });

    $.post('http://localhost:5000/get-like-posts', profileId, "json")
        .done(function (response) {

            showPosts(response);
        });
}

divUsual.onclick = function () {
    $('#liked_post').css({
        'font-weight': '500'
    });

    $('#usual_post').css({
        'font-weight': '700'
    });

    $('#post__underline').css({
        'left': '34.3%'
    });

    $.post('http://localhost:5000/get-posts', profileId, "json")
        .done(function (response) {

            showPosts(response);
        });
}

function showPosts(resp){
    const post = resp.Result;
    posts.innerHTML = '';
    post.forEach(item => addPost(item));

    const isPerson = resp.IsPerson;
    if(!isPerson){
        document.querySelectorAll('.post_item__head__img').forEach((element) => {
            element.style.display = 'none';
        })
    }
}

const postInput = document.getElementById("input_post");
const buttonAdd = document.getElementById("add_post");
buttonAdd.onclick = function () {

    if(postInput.value === '' && selectedImg.style.width !== '100px'){
        return;
    }

    const outputData = {
        Text: postInput.value,
        Image: byteArray
    };

    $.post('http://localhost:5000/write-post', JSON.stringify(outputData), "json")
        .done(function (response){
            posts.innerHTML = '';
            getPosts();
            postInput.value = "";
            selectedImg.src = "../images/profile/camera.svg";
            selectedImg.style.width = "38px";
            selectedImg.style.height = "38px";
        });
}

let posts = document.getElementById("posts");
function addPost(element) {
    let item = document.createElement("div");
    item.className = "post_item";
    item.id = element.Id;
    let imgAvatar = document.createElement("img");
    imgAvatar.src = "/../images/profile/avatar.png";
    imgAvatar.className = "post_item__avatar";
    let itemContainer = document.createElement("div");
    itemContainer.className = "post_item__container";
    let itemHead = document.createElement("div");
    itemHead.className = "post_item__head";
    let itemHeadText = document.createElement("div");
    itemHeadText.className = "post_item__head__text";
    let headName = document.createElement("div");
    headName.className = "post_item__head__name";
    headName.textContent = element.Name;
    let headNickname = document.createElement("div");
    headNickname.className = "post_item__head__nick";
    headNickname.textContent = `@${element.Nickname} · ${element.Date}`;
    itemHeadText.innerHTML += headName.outerHTML + headNickname.outerHTML;
    let divDelete = document.createElement("div");
    divDelete.className = "post_item__head__img";
    let imgMore = document.createElement("img");
    imgMore.src = "/../images/profile/more.svg";
    let divMoreDelete = document.createElement("div");
    divMoreDelete.className = "post_item__head__img div";
    divMoreDelete.textContent = "Удалить";
    divDelete.innerHTML += imgMore.outerHTML + divMoreDelete.outerHTML;
    itemHead.innerHTML += itemHeadText.outerHTML + divDelete.outerHTML;
    let postText = document.createElement("div");
    postText.className = "post_item__text";
    postText.textContent = element.Text;
    let postImg = document.createElement("img");
    postImg.src = element.UrlImage;
    postText.innerHTML += postImg.outerHTML;
    let likeDiv = document.createElement("div");
    likeDiv.className = "post_item__like";
    let imgLike = document.createElement("img");
    if(element.IsLike){
        imgLike.src = "/../images/profile/like.svg"
        imgLike.alt = "click";
    } else {
        imgLike.src = "/../images/profile/empty-like.svg";
    }
    imgLike.className = "post_item__like img";
    let countLike = document.createElement("div");
    countLike.textContent = element.AmountLikes;
    likeDiv.innerHTML += imgLike.outerHTML + countLike.outerHTML;
    itemContainer.innerHTML += itemHead.outerHTML + postText.outerHTML + likeDiv.outerHTML;
    item.innerHTML += imgAvatar.outerHTML + itemContainer.outerHTML;
    posts.appendChild(item);
}

let buttonChange = document.getElementById('button_change');
buttonChange.onclick = function () {
    window.location.href = 'http://localhost:5000/change-profile';
}

let inputPostImg = document.querySelector('#input_img_post');
let selectedImg = document.querySelector('#img_post');
let byteArray;
inputPostImg.onchange = function () {
    changeImage(inputPostImg, selectedImg);
}
function changeImage (input, container) {
    const selected = input.files[0];
    if (!/^image/.test(selected.type)) {
        alert('Выбранный файл не является изображением!');
        return;
    }

    const reader = new FileReader();
    reader.readAsDataURL(selected);
    reader.addEventListener('load', (e) => {
        container.src = e.target.result;
        container.style.width = "100px";
        container.style.height = "auto";

        byteArray = e.target.result;
    });
}

function getPosts() {
    $.post('http://localhost:5000/get-posts', profileId, "json")
        .done(function (response){

            const posts = response.Result;
            posts.forEach(item => addPost(item));

            const user = response.User;
            elName.textContent = user.Name;
            elNick.textContent = `@${user.Nickname}`;
            elDesc.textContent = user.Description;
            elCity.textContent = user.City;
            elLink.textContent = user.Link;
            if(user.UrlAvatar){
                elAvatar.src = user.UrlAvatar;
            }
            if(user.UrlBanner){
                elBanner.src = user.UrlBanner;
            }

            const isPerson = response.IsPerson;
            if(!isPerson){
                $('#add_post').css({
                    'display': 'none'
                });
                $('#form_add_post').css({
                    'display': 'none'
                });
                $('#button_change').css({
                    'display': 'none'
                });
                $('#form').css({
                    'margin': '64px 0 30px'
                });
                document.querySelectorAll('.post_item__head__img').forEach((element) => {
                    element.style.display = 'none';
                })
            }
        });
}
getPosts();

elLink.onclick = linkClick;
function linkClick() {
    window.location = elLink.textContent;
}

posts.addEventListener("click", postClick);

function postClick(event) {
    if (event.target.className === "post_item__head__img div"){
        const postId = event.target.closest('.post_item').id;
        console.log("delete");
        $.post('http://localhost:5000/delete-post', postId)
            .done(function (response){
                posts.innerHTML = '';
                getPosts();
            });
    } else if(event.target.className === "post_item__like img"){
        if(checkAbilityReaction()) {
            return;
        }

        let pNode = event.target.parentNode.querySelector("div");
        if(event.target.alt === "") {{
            $.post('http://localhost:5000/like-post', event.target.closest('.post_item').id, "json")
                .done(function (response){
                    event.target.src = "/../images/profile/like.svg"
                    event.target.alt = "click";
                    pNode.textContent = parseInt(pNode.textContent) + 1;
                });
            }
        } else {
            $.post('http://localhost:5000/unlike-post', event.target.closest('.post_item').id, "json")
                .done(function (response){
                    event.target.src = "/../images/profile/empty-like.svg"
                    event.target.alt = "";
                    pNode.textContent = parseInt(pNode.textContent) - 1;
                });

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