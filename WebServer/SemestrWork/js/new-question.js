let inputTitle = document.getElementById('input_title');
let inputCategory = document.getElementById('input_category');
let inputText = document.getElementById('input_text');
let errorTitle = document.getElementById('error_title');
let errorCategory = document.getElementById('error_category');
let errorText = document.getElementById('error_text');
let categoryOptions = document.querySelector('#input_category_options');
let buttonAdd = document.getElementById('button_add');

categoryOptions.addEventListener('click', selectOption);
function selectOption(event){
    inputCategory.textContent = event.target.textContent;
}

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

buttonAdd.addEventListener("click", function() {

    const outputData = { Title: inputTitle.value ?? " ",
        Category: inputCategory.textContent ?? " ",
        Text: inputText.value ?? " " };

    $.post('http://localhost:5000/add-question', JSON.stringify(outputData), "json")
        .done(function (response){

            if(response.Error === "Вы не авторизованы") {
                alert(response.Error);
                return;
            }

            errorTitle.textContent = response.Title;
            errorCategory.textContent = response.Category;
            errorText.textContent = response.Text;
            if(errorText.textContent === '' && errorCategory.textContent === '' && errorText.textContent === ''){
                alert('Ваш запрос отправлен!');
                window.location = 'http://localhost:5000/all-questions';
            }

        });
});

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