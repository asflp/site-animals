const buttonDone = document.getElementById("button_done");

const inputAvatar = document.getElementById('input_avatar');
const containerAvatar = document.querySelector('#avatar_container');
let byteAvatar;
let checkChangeImages = false;
inputAvatar.onchange = function () {
    getImage(inputAvatar, containerAvatar, byteAvatar);
    changeButton();
}
const inputBanner = document.getElementById('input_banner');
const containerBanner = document.querySelector('#banner_container');
let byteBanner;
inputBanner.onchange = function () {
    getImage(inputBanner, containerBanner, byteBanner);
    changeButton();
}

const inputName = document.getElementById("input_name");
const errorName = document.getElementById("error_name");

inputName.onblur = function () {
    if(inputName.value.length <= 2  && inputName.value.length !== 0){
        errorName.textContent = "Имя слишком короткое";
    } else if(inputName.value.length >= 16){
        errorName.textContent = "Имя слишком длинное";
    } else {
        inputName.value = inputName.value.charAt(0).toUpperCase() + inputName.value.slice(1).toLowerCase();
    }

    if(errorName.textContent !== ""){
        inputName.style.border = '1px solid red';
    }
    changeButton();
}

inputName.onfocus = function () {
    inputName.style.border = 'none';
    errorName.textContent = "";
}

const inputCity = document.getElementById("input_city");
const errorCity = document.getElementById("error_city");

inputCity.onblur = function () {
    if(inputCity.value.length < 2 && inputCity.value.length !== 0){
        errorCity.textContent = "Название города слишком короткое";
    } else if(inputCity.value.length > 168){
        errorCity.textContent = "Название города слишком длинное";
    } else if(!(/^[а-яА-Яa-zA-Z\s-]+$/.test(inputCity.value)) && inputCity.value.length !== 0){
        errorCity.textContent = "Название города может содержать буквы, пробелы и тире";
    } else {
        inputName.value = inputName.value.charAt(0).toUpperCase() + inputName.value.slice(1).toLowerCase();
    }

    if(errorCity.textContent !== ""){
        inputCity.style.border = '1px solid red';
    }
    changeButton();
}

inputCity.onfocus = function () {
    inputCity.style.border = 'none';
    errorCity.textContent = "";
}

const inputNickname = document.getElementById("input_nickname");
const errorNickname = document.getElementById("error_nickname");

inputNickname.onblur = function () {
    if(inputNickname.value.length <= 4 && inputNickname.value.length !== 0){
        errorNickname.textContent = "Ник слишком короткий";
    } else if(inputNickname.value.length >= 31){
        errorNickname.textContent = "Ник слишком длинный";
    } else if(!(/^\S*$/.test(inputNickname.value))){
        errorNickname.textContent = "Ник не должен содержать пробелы";
    } else if(!(/^[a-zA-Z_]+$/.test(inputNickname.value)) && inputNickname.value.length !== 0){
        errorNickname.textContent = "Ник может содержать только английские буквы и нижнее подчеркивание"
    } else {
        inputNickname.value = inputNickname.value.toLowerCase();
    }

    if(errorNickname.textContent !== ""){
        inputNickname.style.border = '1px solid red';
    }
    changeButton()
}

inputNickname.onfocus = function () {
    inputNickname.style.border = 'none';
    errorNickname.textContent = "";
}

const inputLink = document.getElementById("input_link");
const errorLink = document.getElementById("error_link");

inputLink.onblur = function () {
    if(inputLink.value.length <= 4 && inputLink.value.length !== 0){
        errorLink.textContent = "Ссылка слишком короткая";
    } else if(inputLink.value.length >= 31){
        errorLink.textContent = "Ссылка слишком длинная";
    } else if(!(/^\S*$/.test(inputLink.value))){
        errorLink.textContent = "Ссылка не должна содержать пробелы";
    } else if(!(/^(ftp|http|https):\/\/[^ "]+$/.test(inputLink.value)) && inputLink.value.length !== 0){
        errorLink.textContent = "Это не ссылка"
    } else {
        errorLink.value = inputName.value.toLowerCase();
    }

    if(errorLink.textContent !== ""){
        inputLink.style.border = '1px solid red';
    }
    changeButton();
}

inputLink.onfocus = function () {
    inputLink.style.border = 'none';
    errorLink.textContent = "";
}

const inputDescription = document.getElementById("input_description");
const errorDescription = document.getElementById("error_description");

inputDescription.onblur = function () {
    if(inputDescription.value.length <= 3 && inputDescription.value.length !== 0){
        errorDescription.textContent = "Описание профиля слишком короткое";
    } else if(inputDescription.value.length >= 1001){
        errorDescription.textContent = "Описание слишком длинное";
    }

    if(errorDescription.textContent !== ""){
        inputDescription.style.border = '1px solid red';
    }
    changeButton();
}

inputDescription.onfocus = function () {
    inputDescription.style.border = 'none';
    errorDescription.textContent = "";
}

function changeButton() {
    if((inputName.value !== "" || inputCity.value !== "" || inputNickname.value !== ""
        || inputLink.value !== "" || inputDescription.value !== "" || checkChangeImages)
        && (errorLink.textContent === "" && errorName.textContent === "" && errorCity.textContent === ""
        && errorNickname.textContent === "" && errorDescription.textContent === "")){

        buttonDone.style.backgroundColor = '#DA40DD';
        buttonDone.style.cursor = 'pointer';
    } else {
        buttonDone.style.backgroundColor = '#818181';
        buttonDone.style.cursor = 'default';
    }
}

buttonDone.onclick = function () {
    if(buttonDone.style.cursor === 'pointer') {
        let user = {
            Name : inputName.value,
            Nickname : inputNickname.value,
            City: inputCity.value,
            Link: inputLink.value,
            Description: inputDescription.value,
            Avatar: byteAvatar,
            Banner: byteBanner
        };

        $.post('http://localhost:5000/update-user', JSON.stringify(user), "json")
            .done(function (response) {
                    if(response.Error === "Ваша сессия устарела") {
                        alert(response.Error);
                        window.location = 'http://localhost:5000/signin';
                    } else {
                        window.location = 'http://localhost:5000/profile';
                    }
                });
    }
}


const back = document.getElementById('back_to_profile');
back.onclick = function () {
    window.location.href = 'http://localhost:5000/profile';
}

function getImage (input, container, byte) {
    const selected = input.files[0];
    if (!/^image/.test(selected.type)) {
        alert('Выбранный файл не является изображением!');
        return;
    }

    const reader = new FileReader();
    reader.readAsDataURL(selected);
    reader.addEventListener('load', (e) => {
        let children = container.children;
        for(let i = 0; i < children.length; i++) {
            children[i].style.display = "none";
        }
        let image = document.createElement("img");
        image.src = e.target.result;
        image.style.width = "100px";
        image.style.height = "auto";
        if(input == inputAvatar) {
            byteAvatar = e.target.result;
        } else {
            byteBanner = e.target.result;
        }
        container.appendChild(image);
    });
    checkChangeImages = true;
}


const buttonBack = document.getElementById("button_back");
buttonBack.onclick = function () {
    window.location.href = 'http://localhost:5000/profile';
}

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