let inputName = document.getElementById("name");
let nameError = document.getElementById("name_error");

inputName.onblur = function () {
    if(inputName.value === ""){
        nameError.textContent = "Поле должно быть заполнено";
    } else if(inputName.value.length <= 2){
        nameError.textContent = "Имя слишком короткое";
    } else if(inputName.value.length >= 16){
        nameError.textContent = "Имя слишком длинное";
    } else if(!(/^\S*$/.test(inputName.value))){
        nameError.textContent = "Имя не должно содержать пробелы";
    } else {
        inputName.value = inputName.value.charAt(0).toUpperCase() + inputName.value.slice(1).toLowerCase();
    }

    if(nameError.textContent !== ""){
        inputName.style.border = '1px solid red';
    }
}

inputName.onfocus = function () {
    inputName.style.border = 'none';
    nameError.textContent = "";
}


let inputNickname = document.getElementById("surname");
let nicknameError = document.getElementById("surname_error");

inputNickname.onblur = function () {
    if(inputNickname.value === ""){
        nicknameError.textContent = "Поле должно быть заполнено";
    } else if(inputNickname.value.length <= 4){
        nicknameError.textContent = "Ник слишком короткий";
    } else if(inputNickname.value.length >= 31){
        nicknameError.textContent = "Ник слишком длинный";
    } else if(!(/^\S*$/.test(inputNickname.value))){
        nicknameError.textContent = "Ник не должен содержать пробелы";
    } else if(!(/^[a-zA-Z_]+$/.test(inputNickname.value))){
        nicknameError.textContent = "Ник может содержать только английские буквы и нижнее подчеркивание"
    } else {
        inputNickname.value = inputNickname.value.toLowerCase();
    }

    if(nicknameError.textContent !== ""){
        inputNickname.style.border = '1px solid red';
    }
}

inputNickname.onfocus = function () {
    inputNickname.style.border = 'none';
    nicknameError.textContent = "";
}


let inputEmail = document.getElementById("email");
let emailError = document.getElementById("email_error");

inputEmail.onblur = function () {
    if(inputEmail.value === ""){
        emailError.textContent = "Поле должно быть заполнено";
    } else if(!(/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(inputEmail.value))){
        emailError.textContent = "Введите адрес электронной почты";
    }

    if(emailError.textContent !== ""){
        inputEmail.style.border = '1px solid red';
    }
}

inputEmail.onfocus = function () {
    inputEmail.style.border = 'none';
    emailError.textContent = "";
}


let inputPassword = document.getElementById("password");
let passwordError = document.getElementById("password_error");

inputPassword.onblur = function () {
    if(inputPassword.value === ""){
        passwordError.textContent = "Поле должно быть заполнено";
    } else if(inputPassword.value.length <= 7){
        passwordError.textContent = "Пароль слишком короткий";
    } else if(!(/^\S*$/.test(inputPassword.value))){
        passwordError.textContent = "Пароль не может содержать пробелы";
    }

    if(passwordError.textContent !== ""){
        inputPassword.style.border = '1px solid red';
    }
}

inputPassword.onfocus = function () {
    inputPassword.style.border = 'none';
    passwordError.textContent = "";
}


let button = document.getElementById("button");
button.addEventListener("click", function() {

    if(!(passwordError.textContent === "" && emailError.textContent === ""
        && nicknameError.textContent === "" && nameError.textContent === "")) {
        return;
    }

    const outputData = { Name: inputName.value,
        Nickname: inputNickname.value,
        Email: inputEmail.value,
        Password: inputPassword.value};

    $.post('http://localhost:5000/add-user', JSON.stringify(outputData), "json")
        .done(function (response){
            console.log(response);

            nameError.textContent = response.Name;
            nicknameError.textContent = response.Nickname;
            emailError.textContent= response.Email;
            passwordError.textContent = response.Password;

            if(passwordError.textContent === "" && emailError.textContent === ""
                && nicknameError.textContent === "" && nameError.textContent === "") {
                alert("Регистрация прошла успешно");
                let url = new URL(`http://localhost:5000/profile`);
                url.searchParams.set('profile-id', inputNickname.value);
                window.location = url;
            }
        });
});
