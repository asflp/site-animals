let inputNickname = document.getElementById("nickname");
let errorNick = document.getElementById("nickname_error");

inputNickname.onblur = function () {
    if(inputNickname.value === ""){
        errorNick.textContent = "Поле должно быть заполнено";
    } else if(!(/^\S*$/.test(inputNickname.value))){
        errorNick.textContent = "Ник не должен содержать пробелы";
    } else if(!(/^[a-zA-Z_]+$/.test(inputNickname.value))){
        errorNick.textContent = "Ник может содержать только английские буквы и нижнее подчеркивание"
    } else {
        inputNickname.value = inputNickname.value.toLowerCase();
    }


    if(errorNick.textContent !== ""){
        inputNickname.style.border = '1px solid red';
    }
}

inputNickname.onfocus = function () {
    inputNickname.style.border = 'none';
    errorNick.textContent = "";
}

let inputPassword = document.getElementById("password");
let passwordError = document.getElementById("password_error");

inputPassword.onblur = function () {
    if(inputPassword.value === ""){
        passwordError.textContent = "Поле должно быть заполнено";
    }

    if(passwordError.textContent !== ""){
        inputPassword.style.border = '1px solid red';
    }
}

inputPassword.onfocus = function () {
    inputPassword.style.border = 'none';
    passwordError.textContent = "";
}

let button = document.getElementById('button');
button.addEventListener("click", function() {

    if(!(passwordError.textContent === "" && errorNick.textContent === "")) {
        return;
    }

    const outputData = { Name: " ",
        Nickname: inputNickname.value,
        Email: " ",
        Password: inputPassword.value };

    $.post('http://localhost:5000/enter', JSON.stringify(outputData), "json")
        .done(function (response){

            errorNick.textContent = response.Nickname;
            passwordError.textContent = response.Password;

            if(passwordError.textContent === "" && errorNick.textContent === "") {
                alert("Вход выполнен");
                let url = new URL(`http://localhost:5000/profile`);
                url.searchParams.set('profile-id', inputNickname.value);
                window.location = url;
            }
        });
});